using System;
using System.Collections.Generic;
using System.IO;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.Forms.Auto;
using Leadtools.Ocr;
using Leadtools.Ocr.LEADEngine;
using Leadtools.ImageProcessing;
using Leadtools.ImageProcessing.Core;
using Leadtools.Forms.Recognition.Ocr;
using NLog;
//using OCR_DLL_Invoker;
using Leadtools.Forms.Processing;

namespace FormOcrWcf
{

    public class FormAutoRecognitionApi : IDisposable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private RasterCodecs rasterCodecs;
        private List<IOcrEngine> ocrEngines = new List<IOcrEngine>();
        private IOcrEngine cleanUpOcrEngine;
        private DiskMasterFormsRepository workingRepository;
        private AutoFormsEngine autoEngine;

        public int MasterFormsLoaded { get; private set; }



        public FormAutoRecognitionApi(string masterFormsPath, OcrEngineType engineType, string licensePath, string developerKey)
        {
            if (!TrySetLicense(licensePath, developerKey))
            {
                throw new Exception("Failed to setup license data.");
            }

            if (!TryStartUpRasterCodecs())
            {
                throw new Exception("Failed to startup raster codecs.");
            }

            if (!TryStartUpOcrEngine(engineType))
            {
                throw new Exception("Failed to startup ocr engine.");
            }


            if (!TryCreateRepository(masterFormsPath))
            {
                throw new Exception("Failed to load master forms.");
            }
        }

        //public AutoFormsRecognizeFormResult ProcessForm(string targetDocPath)
        public AutoFormsRecognizeFormResult ProcessForm(string targetDocPath)
        {

            //load image
            var imageInfo = rasterCodecs.GetInformation(targetDocPath, true);
            var pageCount = imageInfo.TotalPages;
            // var pageCount = 1;


            var targetImage = rasterCodecs.Load(targetDocPath, 0, CodecsLoadByteOrder.Bgr, 1, pageCount);

            //setup engine
            if (!TrySetupAutoFormsEngine())
            {
                throw new Exception("Failed to setup forms auto processing engine.");
            }

            //process image
            targetImage.ChangeViewPerspective(RasterViewPerspective.TopLeft);
            CleanupImage(targetImage, 1, targetImage.PageCount);
            var result = autoEngine.Run(targetImage, null, targetImage, null);
            //var result = autoEngine.Run(targetDocPath, null);

            if (result != null)
            {

                if (result.RecognitionResult.Properties.Pages - targetImage.PageCount > 0)
                {
                    RasterImage remainingPages = rasterCodecs.Load(targetDocPath, 0, CodecsLoadByteOrder.Bgr, pageCount + 1, result.RecognitionResult.Properties.Pages);
                    CleanupImage(remainingPages, 1, remainingPages.PageCount);
                    targetImage.AddPages(remainingPages, 1, -1);
                }

                // log.Debug($"Image processing finished. {Path.GetFileName(targetDocPath)}");
                return result.RecognitionResult;
            }

            return null;
        }

        private bool TrySetLicense(string licensePath, string developerKey)
        {
            try
            {
                RasterSupport.SetLicense(licensePath, developerKey); //try to set license
            }
            catch (Exception ex)
            {
                // log.Error(ex, "Error while setup license");
            }

            if (RasterSupport.KernelExpired)
            {
                // log.Error("Your license file is missing, invalid or expired. LEADTOOLS will not function. Please contact LEAD Sales for information on obtaining a valid license.");
                return false;
            }

            if (RasterSupport.IsLocked(RasterSupportType.Forms))
            {
                // log.Error("Forms support must be unlocked!");
                return false;
            }

            if (RasterSupport.IsLocked(RasterSupportType.OcrLEAD))
            {
                // log.Error("OCR support must be unlocked!");
                return false;
            }

            //  log.Info("Licnse is valid.");
            return true;
        }

        private bool TryStartUpRasterCodecs()
        {
            try
            {
                rasterCodecs = new RasterCodecs();
                RasterDefaults.DitheringMethod = RasterDitheringMethod.None;

                //To ensure better results from OCR engine, set the loading resolution to 300 DPI 
                rasterCodecs.Options.Load.Resolution = 300;
            }
            catch (Exception ex)
            {
                // log.Error(ex, "Error during startup codecs.");
                return false;
            }

            //log.Info("Raster codecs started successfully.");
            return true;
        }

        private bool TryStartUpOcrEngine(OcrEngineType engineType)
        {
            try
            {
                if (engineType == OcrEngineType.LEAD)
                {
                    IOcrEngine engine = OcrEngineManager.CreateEngine(engineType, true);
                    ocrEngines.Add(engine);
                    ocrEngines[0].Startup(null, null, null, null);
                    SetEngineSettings(engine);
                }
                else
                {

                    var engine = OcrEngineManager.CreateEngine(engineType, true);
                    ocrEngines.Add(engine);
                    ocrEngines[0].Startup(null, null, null, null);
                }

                cleanUpOcrEngine = OcrEngineManager.CreateEngine(engineType, true); //engine for cleanup image
                cleanUpOcrEngine.Startup(null, null, null, null);
            }
            catch (Exception ex)
            {
                // log.Error(ex, "Error occurs while startup engines.");
                return false;
            }

            //  log.Info($"{engineType} engine started successfully!");
            return true;
        }

        private bool TryCreateRepository(string masterPath)
        {
            try
            {
                workingRepository = new DiskMasterFormsRepository(rasterCodecs, masterPath);

                MasterFormsLoaded = GetMasterFormCount(workingRepository.RootCategory);
            }
            catch (Exception ex)
            {
                //  log.Error(ex, "Error occurs during master forms loading.");
                return false;
            }

            //  log.Info($"{MasterFormsLoaded} master forms loaded.");
            return true;
        }

        private bool TrySetupAutoFormsEngine()
        {
            try
            {
                if (autoEngine != null)
                {
                    autoEngine.Dispose();
                    autoEngine = null;
                }


                var managers = AutoFormsRecognitionManager.Ocr | AutoFormsRecognitionManager.Default;

                autoEngine = new AutoFormsEngine(workingRepository, ocrEngines[0], null, managers, 30, 80, false)
                {
                    UseThreadPool = ocrEngines != null && ocrEngines.Count == 1 && ocrEngines[0].EngineType == OcrEngineType.LEAD //Set thread pull using for advantage engine
                };

                autoEngine.Progress += AutoEngineOnProgress;

            }
            catch (Exception ex)
            {
                //  log.Error(ex, "Error while setup forms auto processing engine.");
                return false;
            }

            //log.Info("Form auto processing engine setup successfully.");
            return true;
        }

        private void AutoEngineOnProgress(object sender, AutoFormsProgressEventArgs e)
        {
            var operation = e.Operation.ToString();

            switch (e.Operation)
            {
                case AutoFormsOperation.Generating:
                    operation = "Generating Form Attributes...";
                    break;
                case AutoFormsOperation.Recognizing:
                    operation = "Recognizing Form...";
                    break;
                case AutoFormsOperation.Processing:
                    operation = "Processing Form...";
                    break;
            }

            //log.Debug($"Engine: {operation} - {e.Percentage}%");
        }

        private int GetMasterFormCount(IMasterFormsCategory rootCategory)
        {
            try
            {
                int count = rootCategory.MasterFormsCount;
                foreach (IMasterFormsCategory childCategory in rootCategory.ChildCategories)
                    count = count + GetMasterFormCount(childCategory);
                return count;
            }
            catch
            {
                return 0;
            }
        }

        private void SetEngineSettings(IOcrEngine engine)
        {
            try
            {
                engine.SettingManager.SetEnumValue("Recognition.Fonts.DetectFontStyles", 0);
                engine.SettingManager.SetBooleanValue("Recognition.Fonts.RecognizeFontAttributes", false);

                if (engine.SettingManager.IsSettingNameSupported("Recognition.RecognitionModuleTradeoff"))
                    engine.SettingManager.SetEnumValue("Recognition.RecognitionModuleTradeoff", "Accurate");

                //if (engine.SettingManager.IsSettingNameSupported("Recognition.Zoning.EnableDoubleZoning")) 
                //   engine.SettingManager.SetBooleanValue("Recognition.Zoning.EnableDoubleZoning", false);
            }
            catch (Exception ex)
            {
                // log.Error(ex, "Error occurs while setting engine settings.");
                Console.WriteLine("Not Recognized");
            }
        }

        private void CleanupImage(RasterImage imageToClean, int startIndex, int count)
        {
            try
            {
                //Deskew
                if (cleanUpOcrEngine != null && cleanUpOcrEngine.IsStarted)
                {
                    using (IOcrDocument document = cleanUpOcrEngine.DocumentManager.CreateDocument())
                    {
                        for (var i = startIndex; i < startIndex + count; i++)
                        {
                            imageToClean.Page = i;
                            document.Pages.AddPage(imageToClean, null);
                            int angle = -document.Pages[0].GetDeskewAngle();
                            RotateCommand cmd = new RotateCommand(angle * 10, RotateCommandFlags.Bicubic, RasterColor.FromKnownColor(RasterKnownColor.White));
                            cmd.Run(imageToClean);
                            document.Pages.Clear();
                        }
                    }
                }
                else
                {
                    for (var i = startIndex; i < startIndex + count; i++)
                    {
                        imageToClean.Page = i;

                        var deskewCommand = new DeskewCommand();
                        if (imageToClean.Height > 3500)
                        {
                            deskewCommand.Flags = DeskewCommandFlags.DocumentAndPictures | DeskewCommandFlags.DoNotPerformPreProcessing | DeskewCommandFlags.UseNormalDetection | DeskewCommandFlags.DoNotFillExposedArea;
                        }
                        else
                        {
                            deskewCommand.Flags = DeskewCommandFlags.DeskewImage | DeskewCommandFlags.DoNotFillExposedArea;
                        }

                        deskewCommand.Run(imageToClean);
                    }
                }
            }
            catch (Exception ex)
            {
                //log.Error(ex);
                Console.WriteLine("Not recognized");
            }
        }

        private void Dispose(bool disposed)
        {
            if (autoEngine != null)
            {
                autoEngine.Dispose();
                autoEngine = null;
            }

            if (ocrEngines != null)
            {
                foreach (var ocrEngine in ocrEngines)
                {
                    ocrEngine.Shutdown();
                    ocrEngine.Dispose();
                }
            }

            if (rasterCodecs != null)
            {
                rasterCodecs.Dispose();
                rasterCodecs = null;
            }

            if (cleanUpOcrEngine != null)
            {
                cleanUpOcrEngine.Shutdown();
                cleanUpOcrEngine.Dispose();
                cleanUpOcrEngine = null;
            }

            ocrEngines = null;
        }

        public void Dispose()
        {
            Dispose(false);
        }

        ~FormAutoRecognitionApi()
        {
            Dispose(true);
        }
    }
}
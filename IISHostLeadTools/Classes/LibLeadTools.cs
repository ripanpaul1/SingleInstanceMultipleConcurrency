using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.Forms.Auto;
using Leadtools.Ocr;
using Leadtools.ImageProcessing.Core;
using Leadtools.ImageProcessing;
using Leadtools.Forms.Processing;

namespace IISHostLeadTools
{
    public class LeadToolsUtilities : IDisposable
    {
        private string TheEngineType { get; set; }
        private IOcrEngine TheOcrEngine { get; set; }
        private RasterCodecs TheRasterCodecs { get; set; }
        private DiskMasterFormsRepository TheDiskMasterFormsRepository { get; set; }
        private AutoFormsEngine TheAutoFormsEngine { get; set; }

        public LeadToolsUtilities()
        {
            this.TheEngineType = Convert.ToString(ConfigurationManager.AppSettings["OrcEngineType"]);
            string EnvironmentVariable = Convert.ToString(ConfigurationManager.AppSettings["EnvironmentVariable"]);
            if (!this.SetEnvironmentVariable(EnvironmentVariable)) return;
            LibUtilities.TheEnvironmentVariablePath = LibUtilities.GetEnvironmentVariablePath(EnvironmentVariable);
            if (!this.IsExistEnvironmentDir()) return;

            if (!this.SetLicense()) return;
            if (!this.StartOcrEngine()) return;
            if (!this.CreateRasterCodecs(300)) return;
            if (!this.CreateRepository(TheEnvVarSubDir.OCRMasterFormSets)) return;
            if (!this.SetupAutoFormsEngine()) return;
        }

        private bool SetEnvironmentVariable(string EnvironmentVariable)
        {
            try
            {
                if (Environment.GetEnvironmentVariable(EnvironmentVariable) == null)
                    Environment.SetEnvironmentVariable(EnvironmentVariable, Path.Combine("C:\\", EnvironmentVariable));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool IsExistEnvironmentDir()
        {
            if (!LibUtilities.IsDirectoryExists(LibUtilities.TheEnvironmentVariablePath)) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.License))) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRArchived))) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRInput))) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRMasterFormSets))) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRNotRecognized))) return false;
            if (!LibUtilities.IsDirectoryExists(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCROutput))) return false;
            return true;
        }
        private bool SetLicense()
        {
            var _files = Directory.GetFiles(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.License), "*.*", SearchOption.AllDirectories)
                .Where(file => (file.ToLower().EndsWith("lic") || file.ToLower().EndsWith("key")))
                .ToArray();
            if (_files.ToList().Count < 2) return false;
            if (string.IsNullOrWhiteSpace(_files.Where(file => file.ToLower().EndsWith("lic")).ToList()[0]) ||
                string.IsNullOrWhiteSpace(_files.Where(file => file.ToLower().EndsWith("key")).ToList()[0])) return false;

            RasterSupport.SetLicense(_files.Where(file => file.ToLower().EndsWith("lic")).ToList()[0],
                File.ReadAllText(_files.Where(file => file.ToLower().EndsWith("key")).ToList()[0]));

            if (RasterSupport.KernelExpired) return false;
            if (RasterSupport.IsLocked(RasterSupportType.Forms)) return false;
            if (RasterSupport.IsLocked(RasterSupportType.OcrLEAD)) return false;
            return true;
        }
        private bool SetEngineSettings()
        {
            try
            {
                this.TheOcrEngine.SettingManager.SetEnumValue("Recognition.Fonts.DetectFontStyles", 0);
                this.TheOcrEngine.SettingManager.SetBooleanValue("Recognition.Fonts.RecognizeFontAttributes", false);
                if (this.TheOcrEngine.SettingManager.IsSettingNameSupported("Recognition.RecognitionModuleTradeoff"))
                    this.TheOcrEngine.SettingManager.SetEnumValue("Recognition.RecognitionModuleTradeoff", "Accurate");
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }
        private bool StartOcrEngine()
        {
            try
            {
                OcrEngineType engineType;
                if (!Enum.TryParse(this.TheEngineType, true, out engineType)) return false;

                if (engineType == OcrEngineType.LEAD)
                {
                    this.TheOcrEngine = OcrEngineManager.CreateEngine(engineType, true);
                    this.TheOcrEngine.Startup(null, null, null, null);
                    this.SetEngineSettings();
                }
                else
                {
                    this.TheOcrEngine = OcrEngineManager.CreateEngine(engineType, true);
                    this.TheOcrEngine.Startup(null, null, null, null);
                }
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }
        private bool CreateRasterCodecs(int Resolution)
        {
            try
            {
                this.TheRasterCodecs = new RasterCodecs();
                RasterDefaults.DitheringMethod = RasterDitheringMethod.None;
                this.TheRasterCodecs.Options.Load.Resolution = Resolution;
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CreateRepository(string OCRMasterFormPath)
        {
            try
            {
                this.TheDiskMasterFormsRepository = new DiskMasterFormsRepository(this.TheRasterCodecs, Path.Combine(LibUtilities.TheEnvironmentVariablePath, OCRMasterFormPath));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool SetupAutoFormsEngine()
        {
            try
            {
                var managers = AutoFormsRecognitionManager.Ocr | AutoFormsRecognitionManager.Default;
                this.TheAutoFormsEngine = new AutoFormsEngine(this.TheDiskMasterFormsRepository, this.TheOcrEngine, null, managers, 30, 80, false)
                {
                    UseThreadPool = this.TheOcrEngine != null && this.TheOcrEngine.EngineType == OcrEngineType.LEAD
                };
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CleanupImage(RasterImage imageToClean, int startIndex, int count)
        {
            try
            {
                if (this.IsStartedOcrEngine())
                {
                    using (IOcrDocument document = this.TheOcrEngine.DocumentManager.CreateDocument())
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
                            deskewCommand.Flags = DeskewCommandFlags.DocumentAndPictures | DeskewCommandFlags.DoNotPerformPreProcessing | DeskewCommandFlags.UseNormalDetection | DeskewCommandFlags.DoNotFillExposedArea;
                        else
                            deskewCommand.Flags = DeskewCommandFlags.DeskewImage | DeskewCommandFlags.DoNotFillExposedArea;
                        deskewCommand.Run(imageToClean);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void Dispose(bool Disposed)
        {
            if (Disposed)
            {
                if (this.TheOcrEngine != null || this.IsStartedOcrEngine())
                {
                    this.TheOcrEngine.Shutdown();
                    this.TheOcrEngine = null;
                    this.TheOcrEngine.Dispose();
                }
            }
        }
        
        public bool IsStartedOcrEngine()
        {
            if (this.TheOcrEngine.IsStarted) return true;
            return false;
        }
        public List<VMFieldResult> GetFieldResult(AutoFormsRecognizeFormResult result)
        {
            if (result == null) return null;
            try
            {
                List<VMFieldResult> _VMFieldResult = new List<VMFieldResult>();
                foreach (var formPage in result.FormPages)
                {
                    if (formPage.Count > 0)
                    {
                        foreach (var pageResultItem in formPage)
                        {
                            var textField = pageResultItem as TextFormField;
                            var omrField = pageResultItem as OmrFormField;
                            var tablefield = pageResultItem as TableFormField;
                            if (textField != null)
                            {
                                ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                                ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                                _VMFieldResult.Add(new VMFieldResult(textField.Name, ((TextFormFieldResult)(textField.Result)).Text?.Trim()));
                            }
                            else if (omrField != null)
                            {
                                if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                                    ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                                else
                                    ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                                _VMFieldResult.Add(new VMFieldResult(omrField.Name, ((OmrFormFieldResult)(omrField.Result)).Text));
                            }
                        }
                    }
                }
                return _VMFieldResult;
            }
            catch (Exception err)
            {
                return null;
            }
        }
        public AutoFormsRecognizeFormResult ProcessForm(string file)
        {
            if (file == null) return null;
            try
            {
                var imageInfo = this.TheRasterCodecs.GetInformation(file, true);
                var targetImage = this.TheRasterCodecs.Load(file, 0, CodecsLoadByteOrder.Bgr, 1, imageInfo.TotalPages);
                targetImage.ChangeViewPerspective(RasterViewPerspective.TopLeft);
                if (!this.CleanupImage(targetImage, 1, targetImage.PageCount)) return null;

                var result = this.TheAutoFormsEngine.Run(targetImage, null, targetImage, null);
                if (result == null)
                {
                    return null;
                }
                if (result.RecognitionResult.Properties.Pages - targetImage.PageCount > 0)
                {
                    RasterImage remainingPages = this.TheRasterCodecs.Load(file, 0, CodecsLoadByteOrder.Bgr, imageInfo.TotalPages + 1, result.RecognitionResult.Properties.Pages);
                    if (!this.CleanupImage(remainingPages, 1, remainingPages.PageCount)) return null;
                    targetImage.AddPages(remainingPages, 1, -1);
                }
                return result.RecognitionResult;
            }
            catch
            {
                return null;
            }
        }
        public VMProcessResult ProcessFiles(string FileOrDir)
        {
            try
            {
                string[] _files = null;
                if (string.IsNullOrEmpty(FileOrDir))
                    _files = Directory.GetFiles(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRInput), "*.*", SearchOption.AllDirectories)
                        .Where(f => (f.ToLower().EndsWith("pdf") || f.ToLower().EndsWith("tiff") || f.ToLower().EndsWith("tif")))
                        .ToArray();
                else
                {
                    if (Path.HasExtension(FileOrDir))
                        _files = Directory.GetFiles(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRInput), "*.*", SearchOption.AllDirectories)
                        .Where(f => Path.GetFileName(f.ToLower()).Equals(FileOrDir.ToLower()))
                        .ToArray();
                    else
                        _files = Directory.GetFiles(Path.Combine(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRInput), FileOrDir), "*.*", SearchOption.AllDirectories)
                        .Where(f => (f.ToLower().EndsWith("pdf") || f.ToLower().EndsWith("tiff") || f.ToLower().EndsWith("tif")))
                        .ToArray();
                }
                if (_files == null) return null;
               
                List<VMFileResult> _VMFileResult = new List<VMFileResult>();
                foreach (var _file in _files)
                {
                    VMFileResult vMFileResult = new VMFileResult(Path.GetFileName(_file));
                    var _NewFileName = Path.GetFileNameWithoutExtension(_file) + "_" + DateTime.Now.Ticks + Path.GetExtension(_file);

                    AutoFormsRecognizeFormResult _result = this.ProcessForm(_file);
                    if (_result == null)
                    {
                        vMFileResult.FieldResult = null;
                        vMFileResult.Status = "Not Recognized";
                        File.Copy(_file, Path.Combine(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRNotRecognized), _NewFileName), true);
                    }
                    else
                    {
                        vMFileResult.FieldResult = this.GetFieldResult(_result);
                        if (vMFileResult.FieldResult == null || vMFileResult.FieldResult.Count == 0)
                        {
                            vMFileResult.Status = "Not Recognized";
                            File.Copy(_file, Path.Combine(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCRNotRecognized), _NewFileName), true);
                        }
                        else
                        {
                            vMFileResult.Status = "Successfully Recognized";
                            File.Copy(_file, Path.Combine(Path.Combine(LibUtilities.TheEnvironmentVariablePath, TheEnvVarSubDir.OCROutput), _NewFileName));
                        }
                    }
                    _VMFileResult.Add(vMFileResult);
                }
                return new VMProcessResult(_VMFileResult);
            }
            catch(Exception err)
            {
                return null;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        ~LeadToolsUtilities()
        {
            this.Dispose();
        }
    }
}
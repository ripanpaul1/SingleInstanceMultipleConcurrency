using System;
using System.IO;
using System.Linq;
using Leadtools.Forms.Auto;
using Leadtools.Ocr;
using Leadtools.Forms.Processing;
using NLog;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using OCR_DLL_Invoker;

namespace FormOcrWcf
{
    public class Program
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        static string EngineType = "LEAD";

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // const int SW_HIDE = 0;
        //const int SW_SHOW = 5;

        private static string GetEnvironmentVariable(string latFormHome)
        {
            if (latFormHome == null)
            {
                return null;
            }

            var value = Environment.GetEnvironmentVariable(latFormHome);

            return value;
        }

        private static Dictionary<string, int> dictionary = new Dictionary<string, int>();

        public static AutoFormsRecognizeFormResult ProcessForms(String DirPath)
        {
           // string result = string.Empty; // Commented by Prasanta,
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //DirPath = GetEnvironmentVariable("LAT_FORM_HOME");

            if (DirPath == null)
            {
                //log.Warn("Set the LAT_FORM_HOME environment variable");
                //Console.WriteLine("Not Recognized");
            }

            //log.Debug("App started.");

            try
            {
                //var targetDocDropbox = Path.Combine(DirPath, "Input\\Dropbox");
                var targetDocInput = Path.Combine(DirPath, "OCRInput\\1");
                string[] formNames = Directory.GetFiles(targetDocInput).Select(Path.GetFileName).ToArray();
                //ProcessFiles(targetDocInput, formNames);
                result = ProcessFlow(formNames);

                var targetDocOutput = Path.Combine(DirPath, "Output\\FormNameandCount.csv");

                using (var outputStream = new FileStream(targetDocOutput, FileMode.Create))
                {
                    using (var writer = new StreamWriter(outputStream))
                    {
                        // writer.WriteLine("page number, field, type, result, confidence, x, y, width, heigth");
                        writer.WriteLine("Formname,Count");
                        {
                            foreach (var item in dictionary)
                            {
                                var value = (item.Key);
                                int count = (item.Value);

                                {
                                    // writer.WriteLine($"{formPage.PageNumber}, {textField.Name}, Text, {((TextFormFieldResult)textField.Result).Text?.Trim()}, {((TextFormFieldResult)textField.Result).AverageConfidence}");
                                    writer.WriteLine(string.Format("{0}, {1}", value, count));
                                    continue;
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                //log.Error(ex, "Fatal error");
                //Console.WriteLine("Not Recognized");
            }

            //log.Debug("App closing..");   
            //Console.WriteLine(result);
            return result;
            //Console.Read();
        }

        public static AutoFormsRecognizeFormResult ProcessForms(String DirPath,long RunID)
        {
            // string result = string.Empty; // Commented by Prasanta,
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //DirPath = GetEnvironmentVariable("LAT_FORM_HOME");

            if (DirPath == null)
            {
                //log.Warn("Set the LAT_FORM_HOME environment variable");
                //Console.WriteLine("Not Recognized");
            }

            //log.Debug("App started.");

            try
            {
                //var targetDocDropbox = Path.Combine(DirPath, "Input\\Dropbox");
                var targetDocInput = Path.Combine(DirPath, "OCRInput\\1");
                string[] formNames = Directory.GetFiles(targetDocInput).Select(Path.GetFileName).ToArray();
                //ProcessFiles(targetDocInput, formNames);
                //result = ProcessFlow(formNames);
                result = ProcessFlow(formNames, RunID);

                var targetDocOutput = Path.Combine(DirPath, "Output\\FormNameandCount.csv");

                using (var outputStream = new FileStream(targetDocOutput, FileMode.Create))
                {
                    using (var writer = new StreamWriter(outputStream))
                    {
                        // writer.WriteLine("page number, field, type, result, confidence, x, y, width, heigth");
                        writer.WriteLine("Formname,Count");
                        {
                            foreach (var item in dictionary)
                            {
                                var value = (item.Key);
                                int count = (item.Value);

                                {
                                    // writer.WriteLine($"{formPage.PageNumber}, {textField.Name}, Text, {((TextFormFieldResult)textField.Result).Text?.Trim()}, {((TextFormFieldResult)textField.Result).AverageConfidence}");
                                    writer.WriteLine(string.Format("{0}, {1}", value, count));
                                    continue;
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                //log.Error(ex, "Fatal error");
                //Console.WriteLine("Not Recognized");
            }

            //log.Debug("App closing..");   
            //Console.WriteLine(result);
            return result;
            //Console.Read();
        }


        // commented because we are now building this as a dll - Murali
        /*
        public static void Main(string [] args )
        {
            string result = string.Empty;
            
            //log.Warn("entered the main funtion");
            var handle = GetConsoleWindow();

            // Hide
           // ShowWindow(handle, SW_HIDE);

            string DirPath = GetEnvironmentVariable("LAT_FORM_HOME");

            if (DirPath == null)
            {
                //log.Warn("Set the LAT_FORM_HOME environment variable");
                Console.WriteLine("Not Recognized");
            }

            //log.Debug("App started.");

            try
            {
                //var targetDocDropbox = Path.Combine(DirPath, "Input\\Dropbox");
                var targetDocInput = Path.Combine(DirPath, "OCRInput");
                string[] formNames = Directory.GetFiles(targetDocInput).Select(Path.GetFileName).ToArray();
                //ProcessFiles(targetDocInput, formNames);
                result =ProcessFlow(formNames);

                var targetDocOutput = Path.Combine(DirPath, "Output\\FormNameandCount.csv");

                using (var outputStream = new FileStream(targetDocOutput, FileMode.Create))
                {
                    using (var writer = new StreamWriter(outputStream))
                    {
                        // writer.WriteLine("page number, field, type, result, confidence, x, y, width, heigth");
                        writer.WriteLine("Formname,Count");
                        {
                            foreach (var item in dictionary)
                            {
                                var value = (item.Key);
                                int count = (item.Value);

                                {
                                    // writer.WriteLine($"{formPage.PageNumber}, {textField.Name}, Text, {((TextFormFieldResult)textField.Result).Text?.Trim()}, {((TextFormFieldResult)textField.Result).AverageConfidence}");
                                    writer.WriteLine(string.Format("{0}, {1}", value, count));
                                    continue;
                                }
                            }
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                //log.Error(ex, "Fatal error");
                Console.WriteLine("Not Recognized");
            }

            //log.Debug("App closing..");   
            Console.WriteLine(result);
            //Console.Read();
        }
        */

        public static AutoFormsRecognizeFormResult ProcessFlow(string[] formNames)
        {
            //string result = string.Empty;//Commented by Prasanta
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //log.Warn("entered the main funtion");
            var handle = GetConsoleWindow();

            // Hide
            // ShowWindow(handle, SW_HIDE);

            string DirPath = GetEnvironmentVariable("LAT_FORM_HOME");

            if (DirPath == null)
            {
                //log.Warn("Set the LAT_FORM_HOME environment variable");
                Console.WriteLine("Not Recognized");

            }

            //log.Debug("App started.");

            try
            {
                //var targetDocDropbox = Path.Combine(DirPath, "Input\\Dropbox");
                var targetDocInput = Path.Combine(DirPath, "OCRInput");

                result = ProcessFiles(targetDocInput, formNames);

                var targetDocOutput = Path.Combine(DirPath, "OCROutput\\FormNameandCount.csv");
            }
            catch (Exception ex)
            {
                //log.Error(ex, "Fatal error");
                Console.WriteLine("Not Recognized");
            }

            //log.Debug("App closing..");

            return result;
        }


        public static AutoFormsRecognizeFormResult ProcessFlow(string[] formNames, long RunID)
        {
            //string result = string.Empty;//Commented by Prasanta
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //log.Warn("entered the main funtion");
            var handle = GetConsoleWindow();

            // Hide
            // ShowWindow(handle, SW_HIDE);

            string DirPath = GetEnvironmentVariable("LAT_FORM_HOME");

            if (DirPath == null)
            {
                //log.Warn("Set the LAT_FORM_HOME environment variable");
                Console.WriteLine("Not Recognized");

            }

            //log.Debug("App started.");

            try
            {
                //var targetDocDropbox = Path.Combine(DirPath, "Input\\Dropbox");
                var targetDocInput = Path.Combine(DirPath, "OCRInput");

                //result = ProcessFiles(targetDocInput, formNames);
                result = ProcessFiles(targetDocInput, formNames,RunID);

                var targetDocOutput = Path.Combine(DirPath, "OCROutput\\FormNameandCount.csv");
            }
            catch (Exception ex)
            {
                //log.Error(ex, "Fatal error");
                Console.WriteLine("Not Recognized");
            }

            //log.Debug("App closing..");

            return result;
        }
        //New setlicence function - MM

        public static bool SetLicense(bool silent)
        {
            string licenseFileRelativePath = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic");
            string keyFileRelativePath = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic.key");
            string developerKey = System.IO.File.ReadAllText(keyFileRelativePath);

            try
            {
                // TODO: Change this to use your license file and developer key */
                //string licenseFilePath = "Replace this with the path to the LEADTOOLS license file";
                //string developerKey = "Replace this with your developer key";
                //Replace the path - MM
                //string licenseFileRelativePath = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic");
                //string keyFileRelativePath = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic.key");
                //string developerKey = System.IO.File.ReadAllText(keyFileRelativePath);
                Leadtools.RasterSupport.SetLicense(licenseFileRelativePath, developerKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }

            if (Leadtools.RasterSupport.KernelExpired)
            {
                string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                /* Try the common LIC directory */
                //string licenseFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC");
                //string keyFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\Common\\License\\LEADTOOLS.LIC.key");


                //var defaultLicenseFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic");
                //var defaultLicenseKeyFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic.key");

                if (!System.IO.File.Exists(licenseFileRelativePath))
                {
                    licenseFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\Common\\License\\LEADTOOLS.LIC");
                    keyFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\Common\\License\\LEADTOOLS.LIC.key");
                }

                if (!System.IO.File.Exists(licenseFileRelativePath))
                {
                    licenseFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\..\\..\\..\\Common\\License\\LEADTOOLS.LIC");
                    keyFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\..\\..\\..\\Common\\License\\LEADTOOLS.LIC.key");
                }

                if (!System.IO.File.Exists(licenseFileRelativePath))
                {
                    licenseFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\..\\..\\..\\..\\Common\\License\\LEADTOOLS.LIC");
                    keyFileRelativePath = System.IO.Path.Combine(dir, "..\\..\\..\\..\\..\\..\\..\\Common\\License\\LEADTOOLS.LIC.key");
                }

                if (System.IO.File.Exists(licenseFileRelativePath) && System.IO.File.Exists(keyFileRelativePath))
                {
                    //string developerKey = System.IO.File.ReadAllText(keyFileRelativePath);
                    try
                    {
                        Leadtools.RasterSupport.SetLicense(licenseFileRelativePath, developerKey);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex.Message);
                    }
                }
            }

            if (Leadtools.RasterSupport.KernelExpired)
            {
                if (silent == false)
                {
                    string msg = "Your license file is missing, invalid or expired. LEADTOOLS will not function. Please contact LEAD Sales for information on obtaining a valid license.";
                    string logmsg = string.Format("*** NOTE: {0} ***{1}", msg, Environment.NewLine);
                    System.Diagnostics.Debugger.Log(0, null, "*******************************************************************************" + Environment.NewLine);
                    System.Diagnostics.Debugger.Log(0, null, logmsg);
                    System.Diagnostics.Debugger.Log(0, null, "*******************************************************************************" + Environment.NewLine);

                    //MessageBox.Show(null, msg, "No LEADTOOLS License", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    System.Diagnostics.Process.Start("https://www.leadtools.com/downloads/evaluation-form.asp?evallicenseonly=true");
                }

                return false;
            }
            return true;
        }

        public static AutoFormsRecognizeFormResult ProcessFiles(string documentsPath, string[] formNames)
        {
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();// Added By Prasanta, string will be handled in srevice
            string ocrOutput = String.Empty;
            var dirPath = GetEnvironmentVariable("LAT_FORM_HOME");  // Home directory path
                                                                    // var defaultLicenseFile = Path.Combine(dirPath, "License\\Lateetud_License.lic");
                                                                    // var defaultLicenseKeyFile = Path.Combine(dirPath, "License\\Lateetud_License.lic.key");
            var defaultLicenseFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic");
            var defaultLicenseKeyFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic.key");
            SetLicense(true);
            var value = 0;
            var tifFiles = Directory.GetFiles(documentsPath, "*.*", SearchOption.AllDirectories).Where(file => (file.ToLower().EndsWith("pdf") || file.ToLower().EndsWith("tiff") || file.ToLower().EndsWith("tif"))).ToArray();

            //filter forms with sent formNames
            if (formNames.Length > 0)
            {
                tifFiles = tifFiles.Where(file => formNames.Any(tf => String.Equals(tf, Path.GetFileName(file), StringComparison.InvariantCultureIgnoreCase))).ToArray();
            }

            //var DirPath = @"C:\Users\Public\Lateetud Form Recognizer";
            var masterPath = Path.Combine(dirPath, "OCRMasterFormSets");
            var notFoundPath = Path.Combine(dirPath, "OCRNotRecognized");

            OcrEngineType engineType;
            if (!Enum.TryParse(EngineType, true, out engineType))
            {
                // log.Error(string.Format("Unknown engine type [{0}]", EngineType));
                Console.WriteLine("Not Recognized");

            }

            foreach (var fileToProcess in tifFiles)
            {
                //  var documentResult = new OcrDocumentResult();
                // documentResult.DocumentName = Path.GetFileName(fileToProcess);
                //   output.DocumentsOcrReport.Add(documentResult);

                using (var rApi = new FormAutoRecognitionApi(masterPath, engineType, defaultLicenseFile, File.ReadAllText(defaultLicenseKeyFile)))
                {
                    var targetDocPath = Path.Combine(documentsPath, fileToProcess);
                    var filename = Path.GetFileName(targetDocPath);
                    var newFilename = Path.GetFileNameWithoutExtension(targetDocPath)+"_"+DateTime.Now.Ticks + Path.GetExtension(targetDocPath); //Added By Prasanta
                    //AutoFormsRecognizeFormResult result = rApi.ProcessForm(targetDocPath);// Commented By Prasanta, string will be handled in srevice
                    result = rApi.ProcessForm(targetDocPath);
                    if (result != null)
                    {
                        string outputPath = Path.Combine(dirPath, "OCROutput");

                        var formName = result.Properties.Name; // This is the Master Form Name
                        var csvFileName = Path.GetFileNameWithoutExtension(filename);
                        var extension = Path.GetExtension(filename);
                        //var targetDirPath = Path.Combine(dirPath, "Output", outputFolderName, formName);
                        //var csvtargetPath = Path.Combine(targetDirPath, csvFileName);

                        // See whether Dictionary contains this string.
                        if (dictionary.ContainsKey(formName))
                        {
                            dictionary[formName]++;
                        }
                        else
                        {
                            dictionary.Add(formName, 1);
                        }

                        if (!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }
                        //var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        //var reNameFile = Path.Combine(csvtargetPath + "_" + timestamp + extension);

                        //File.Copy(targetDocPath, Path.Combine(outputPath, filename));// Commented By Prasanta, To rename the old file with timestamp
                        File.Copy(targetDocPath, Path.Combine(outputPath, newFilename));// Added By Prasanta

                        // PrintToCSV(csvtargetPath + "_" + timestamp + ".csv", result, formName);
                        //ocrOutput = PrintToString(result);// Commented By Prasanta, string will be handled in srevice

                        //log.Info("Done!");

                    }
                    else
                    {
                        if (!Directory.Exists(notFoundPath))
                        {
                            Directory.CreateDirectory(notFoundPath);
                        }

                        if (dictionary.ContainsKey("Not Recognized"))
                        {
                            dictionary["Not Recognized"]++;
                        }

                        else
                        {
                            dictionary.Add("Not Recognized", 1);
                        }

                        var filenotfound = Path.Combine(notFoundPath, filename);

                        // File.Copy(targetDocPath, filenotfound, true);
                        // log.Warn("No result found");
                        Console.WriteLine("Not Recognized");
                    }
                }

            }
            //return ocrOutput;// Commented By Prasanta, string will be handled in srevice
            return result;
        }

        public static AutoFormsRecognizeFormResult ProcessFiles(string documentsPath, string[] formNames, long RunID)
        {
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();// Added By Prasanta, string will be handled in srevice
            string ocrOutput = String.Empty;
            var dirPath = GetEnvironmentVariable("LAT_FORM_HOME");  // Home directory path
                                                                    // var defaultLicenseFile = Path.Combine(dirPath, "License\\Lateetud_License.lic");
                                                                    // var defaultLicenseKeyFile = Path.Combine(dirPath, "License\\Lateetud_License.lic.key");
            var defaultLicenseFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic");
            var defaultLicenseKeyFile = Path.Combine("C:\\LEADTOOLS 20\\Common", "License\\LEADTOOLS.lic.key");
            SetLicense(true);
            var value = 0;
            var tifFiles = Directory.GetFiles(documentsPath, "*.*", SearchOption.AllDirectories).Where(file => (file.ToLower().EndsWith("pdf") || file.ToLower().EndsWith("tiff") || file.ToLower().EndsWith("tif"))).ToArray();

            //filter forms with sent formNames
            if (formNames.Length > 0)
            {
                tifFiles = tifFiles.Where(file => formNames.Any(tf => String.Equals(tf, Path.GetFileName(file), StringComparison.InvariantCultureIgnoreCase))).ToArray();
            }

            //var DirPath = @"C:\Users\Public\Lateetud Form Recognizer";
            var masterPath = Path.Combine(dirPath, "OCRMasterFormSets");
            var notFoundPath = Path.Combine(dirPath, "OCRNotRecognized");

            OcrEngineType engineType;
            if (!Enum.TryParse(EngineType, true, out engineType))
            {
                // log.Error(string.Format("Unknown engine type [{0}]", EngineType));
                Console.WriteLine("Not Recognized");

            }

            foreach (var fileToProcess in tifFiles)
            {
                //  var documentResult = new OcrDocumentResult();
                // documentResult.DocumentName = Path.GetFileName(fileToProcess);
                //   output.DocumentsOcrReport.Add(documentResult);

                using (var rApi = new FormAutoRecognitionApi(masterPath, engineType, defaultLicenseFile, File.ReadAllText(defaultLicenseKeyFile)))
                {
                    var targetDocPath = Path.Combine(documentsPath, fileToProcess);
                    var filename = Path.GetFileName(targetDocPath);
                    var newFilename = Path.GetFileNameWithoutExtension(targetDocPath) + "_" + DateTime.Now.Ticks + Path.GetExtension(targetDocPath); //Added By Prasanta
                    //AutoFormsRecognizeFormResult result = rApi.ProcessForm(targetDocPath);// Commented By Prasanta, string will be handled in srevice
                    result = rApi.ProcessForm(targetDocPath);
                    if (result != null)
                    {
                        string outputPath = Path.Combine(dirPath, "OCROutput");

                        var formName = result.Properties.Name; // This is the Master Form Name
                        var csvFileName = Path.GetFileNameWithoutExtension(filename);
                        var extension = Path.GetExtension(filename);
                        //var targetDirPath = Path.Combine(dirPath, "Output", outputFolderName, formName);
                        //var csvtargetPath = Path.Combine(targetDirPath, csvFileName);

                        // See whether Dictionary contains this string.
                        if (dictionary.ContainsKey(formName))
                        {
                            dictionary[formName]++;
                        }
                        else
                        {
                            dictionary.Add(formName, 1);
                        }

                        if (!Directory.Exists(outputPath))
                        {
                            Directory.CreateDirectory(outputPath);
                        }
                        //var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        //var reNameFile = Path.Combine(csvtargetPath + "_" + timestamp + extension);

                        //File.Copy(targetDocPath, Path.Combine(outputPath, filename));// Commented By Prasanta, To rename the old file with timestamp
                        File.Copy(targetDocPath, Path.Combine(outputPath, newFilename));// Added By Prasanta

                        #region Save Into DB
                        MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();
                        foreach (var formPage in result.FormPages)
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
                                    //fieldname = textField.Name;
                                    //fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                                    applicationSummary.RunID = Convert.ToInt64(RunID);
                                    applicationSummary.EntryDate = DateTime.Now;
                                    applicationSummary.FileName = filename;
                                    applicationSummary.FieldKey = textField.Name;
                                    applicationSummary.FieldValue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();
                                    using (DBEntities db = new DBEntities())
                                    {
                                        db.MasterFormApplicationSummaries.Add(applicationSummary);
                                        db.SaveChanges();
                                    }


                                    //ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;

                                }
                                else if (omrField != null)
                                {
                                    if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                                    {
                                        ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                                    }
                                    else

                                    {
                                        ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                                    }

                                    //fieldname = omrField.Name;
                                    //fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;

                                    applicationSummary.RunID = Convert.ToInt64(RunID);
                                    applicationSummary.EntryDate = DateTime.Now;
                                    applicationSummary.FileName = fileToProcess;
                                    applicationSummary.FieldKey = omrField.Name;
                                    applicationSummary.FieldValue = ((OmrFormFieldResult)(omrField.Result)).Text;
                                    using (DBEntities db = new DBEntities())
                                    {
                                        db.MasterFormApplicationSummaries.Add(applicationSummary);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        #endregion

                        // PrintToCSV(csvtargetPath + "_" + timestamp + ".csv", result, formName);
                        //ocrOutput = PrintToString(result);// Commented By Prasanta, string will be handled in srevice

                        //log.Info("Done!");

                    }
                    else
                    {
                        if (!Directory.Exists(notFoundPath))
                        {
                            Directory.CreateDirectory(notFoundPath);
                        }

                        if (dictionary.ContainsKey("Not Recognized"))
                        {
                            dictionary["Not Recognized"]++;
                        }

                        else
                        {
                            dictionary.Add("Not Recognized", 1);
                        }

                        var filenotfound = Path.Combine(notFoundPath, filename);

                        // File.Copy(targetDocPath, filenotfound, true);
                        // log.Warn("No result found");
                        Console.WriteLine("Not Recognized");
                    }
                }

            }
            //return ocrOutput;// Commented By Prasanta, string will be handled in srevice
            return result;
        }




        private static int MinConfidence(AutoFormsRecognizeFormResult result)
        {
            var confidence = new List<int>();

            foreach (var formPage in result.FormPages)
            {
                foreach (var pageResultItem in formPage)
                {
                    var textField = pageResultItem as TextFormField;
                    var omrField = pageResultItem as OmrFormField;

                    if (textField != null)
                    {
                        if (((TextFormFieldResult)textField.Result).Text != null)
                        {
                            confidence.Add(((TextFormFieldResult)textField.Result).AverageConfidence);
                        }
                    }

                    else if (omrField != null)
                    {
                        if (((OmrFormFieldResult)omrField.Result).Text != null)
                        {
                            confidence.Add(((OmrFormFieldResult)(omrField.Result)).AverageConfidence);
                        }
                    }
                }
            }

            var minconfidence = confidence.Min();

            return minconfidence;
        }

        public static void PrintToCSV(string resultPath, AutoFormsRecognizeFormResult result, string formName)
        {
            string fieldname = "field,";
            string fieldvalue = "value,";
            string pagenumber = "pagenumber,";
            string confidencevalue = "confidence,";
            string boundx = "x,";
            string boundy = "y,";
            string boundwidth = "width,";
            string boundheight = "height,";
            string tableinfo = "";

            using (var outputStream = new FileStream(resultPath, FileMode.Create))
            {
                using (var writer = new StreamWriter(outputStream))
                {
                    foreach (var formPage in result.FormPages)
                    {
                        foreach (var pageResultItem in formPage)
                        {
                            var textField = pageResultItem as TextFormField;
                            var omrField = pageResultItem as OmrFormField;
                            var tablefield = pageResultItem as TableFormField;

                            // confidence.Add(((TextFormFieldResult)textField.Result).AverageConfidence);
                            if (textField != null)
                            {
                                //  if (((TextFormFieldResult)textField.Result).AverageConfidence < 60)

                                ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                                ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                                //MM
                                //writer.WriteLine($"{textField.Name}, {((TextFormFieldResult)textField.Result).Text?.Trim()}, {((TextFormFieldResult)textField.Result).AverageConfidence}, {textField.Bounds.X}, {textField.Bounds.Y}, {textField.Bounds.Width}, {textField.Bounds.Height}");
                                fieldname += textField.Name + ",";
                                fieldvalue += ((TextFormFieldResult)(textField.Result)).Text?.Trim() + ",";
                                pagenumber += (formPage.PageNumber) + ",";
                                confidencevalue += ((TextFormFieldResult)textField.Result).AverageConfidence + ",";
                                boundx += textField.Bounds.X + ",";
                                boundy += textField.Bounds.Y + ",";
                                boundwidth += textField.Bounds.Width + ",";
                                boundheight += textField.Bounds.Height + ",";
                            }
                            else if (omrField != null)
                            {
                                if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                                {
                                    ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                                }
                                else

                                {
                                    ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                                }

                                //MM
                                //writer.WriteLine($"{omrField.Name},{((OmrFormFieldResult)omrField.Result).Text}, {((OmrFormFieldResult)omrField.Result).AverageConfidence}, {omrField.Bounds.X}, {omrField.Bounds.Y}, {omrField.Bounds.Width}, {omrField.Bounds.Height}");
                                fieldname += omrField.Name + ",";
                                fieldvalue += ((OmrFormFieldResult)(omrField.Result)).Text + ",";
                                pagenumber += (formPage.PageNumber) + ",";
                                confidencevalue += ((OmrFormFieldResult)(omrField.Result)).AverageConfidence + ",";
                                boundx += omrField.Bounds.X + ",";
                                boundy += omrField.Bounds.Y + ",";
                                boundwidth += omrField.Bounds.Width + ",";
                                boundheight += omrField.Bounds.Height + ",";
                            }

                            else if (tablefield != null)
                            {
                                TableFormFieldResult results = tablefield.Result as TableFormFieldResult;

                                if (results != null)
                                {
                                    for (int i = 0; i < results.Rows.Count; i++)
                                    {
                                        TableFormRow row = results.Rows[i];
                                        // tableinfo += tableinfo + row;

                                        for (int j = 0; j < row.Fields.Count; j++)
                                        {
                                            OcrFormField ocrField = row.Fields[j];
                                            if (ocrField is TextFormField)
                                            {
                                                TextFormFieldResult txtResults = ocrField.Result as TextFormFieldResult;
                                                tableinfo += txtResults.Text;



                                            }
                                            else if (ocrField is OmrFormField)
                                            {
                                                OmrFormFieldResult omrResults = ocrField.Result as OmrFormFieldResult;
                                                tableinfo += omrResults.Text;
                                                //tableinfo += ocrField;
                                            }
                                        }
                                    }
                                }
                                File.WriteAllText(@"C: \Users\Public\Lateetud\string.txt", tableinfo);

                            }
                        }
                    }

                    writer.WriteLine(fieldname);
                    writer.WriteLine(fieldvalue);
                    writer.WriteLine(pagenumber);
                    writer.WriteLine(confidencevalue);
                    writer.WriteLine(boundx);
                    writer.WriteLine(boundy);
                    writer.WriteLine(boundwidth);
                    writer.WriteLine(boundheight);
                }
            }
        }

        //Table region is not added
        public static string PrintToString(AutoFormsRecognizeFormResult result)
        {
            string fieldname = string.Empty;
            string fieldvalue = "value,";
            string pagenumber = "pagenumber,";
            string confidencevalue = "confidence,";
            string boundx = "x,";
            string boundy = "y,";
            string boundwidth = "width,";
            string boundheight = "height,";
            string tableinfo = "";

            string ocrResult = String.Empty;
            string openBrac = "[[[";
            string closeBrac = "]]]";
            string resDenoter = ":::";

            foreach (var formPage in result.FormPages)
            {
                foreach (var pageResultItem in formPage)
                {
                    var textField = pageResultItem as TextFormField;
                    var omrField = pageResultItem as OmrFormField;
                    var tablefield = pageResultItem as TableFormField;

                    // confidence.Add(((TextFormFieldResult)textField.Result).AverageConfidence);
                    if (textField != null)
                    {
                        //  if (((TextFormFieldResult)textField.Result).AverageConfidence < 60)

                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                        //MM
                        //writer.WriteLine($"{textField.Name}, {((TextFormFieldResult)textField.Result).Text?.Trim()}, {((TextFormFieldResult)textField.Result).AverageConfidence}, {textField.Bounds.X}, {textField.Bounds.Y}, {textField.Bounds.Width}, {textField.Bounds.Height}");
                        fieldname = textField.Name;
                        fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                        /*
                        pagenumber += (formPage.PageNumber) + ",";
                        confidencevalue += ((TextFormFieldResult)textField.Result).AverageConfidence + ",";
                        boundx += textField.Bounds.X + ",";
                        boundy += textField.Bounds.Y + ",";
                        boundwidth += textField.Bounds.Width + ",";
                        boundheight += textField.Bounds.Height + ",";
                        */
                    }
                    else if (omrField != null)
                    {
                        if (((OmrFormFieldResult)(omrField.Result)).Text == "0")
                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("0", "False");
                        }
                        else

                        {
                            ((OmrFormFieldResult)(omrField.Result)).Text = ((OmrFormFieldResult)(omrField.Result)).Text?.Replace("1", "True");
                        }

                        //MM
                        //writer.WriteLine($"{omrField.Name},{((OmrFormFieldResult)omrField.Result).Text}, {((OmrFormFieldResult)omrField.Result).AverageConfidence}, {omrField.Bounds.X}, {omrField.Bounds.Y}, {omrField.Bounds.Width}, {omrField.Bounds.Height}");
                        fieldname = omrField.Name;
                        fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;
                        /*
                        pagenumber += (formPage.PageNumber) + ",";
                        confidencevalue += ((OmrFormFieldResult)(omrField.Result)).AverageConfidence + ",";
                        boundx += omrField.Bounds.X + ",";
                        boundy += omrField.Bounds.Y + ",";
                        boundwidth += omrField.Bounds.Width + ",";
                        boundheight += omrField.Bounds.Height + ",";
                        */
                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                    }
                }
            }
            return ocrResult;
        }

    }
}
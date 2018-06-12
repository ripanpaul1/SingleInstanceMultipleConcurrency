using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;
using System.Configuration;
using System.Web.Services;
using FormOcrWcf;
using Leadtools.Forms.Processing;
using Leadtools.Forms.Auto;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Web.Script.Services;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Collections;
using System.Reflection;

namespace OCR_DLL_Invoker
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        Process proc = null;
        private static string GetEnvironmentVariable(string latFormHome)
        {
            if (latFormHome == null)
            {
                return null;
            }

            var value = Environment.GetEnvironmentVariable(latFormHome);

            return value;
        }

        [WebMethod]
        public string OCRInvoker_BPMSERVER()
        {

            string output = "";
            string DirPath = GetEnvironmentVariable("SMART");
            AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();

            //output = FormOcrWcf.Program.ProcessForms(DirPath);
            result = FormOcrWcf.Program.ProcessForms(DirPath);
            output = PrintToString(result);
            return output;
        }
        [WebMethod]
        public string GetResultInXML(string FileName)
        {

            string strStatus = "START";
            long runID;
            string str = string.Empty;
            
            using (DBEntities context = new DBEntities())
            {
                IEnumerable<CurrentRun> details = context.Database.SqlQuery
                                                                              <CurrentRun>("exec proc_APICallHistory_GetRunID").ToList();
                runID = Convert.ToInt64(details.FirstOrDefault().RunID);
            }
            try
            {
                #region Entry into APICallHistory


                APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.StartTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For XML";
                apiCallHstStart.Status = strStatus;//"START";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion


                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                if (string.IsNullOrEmpty(FileName))//If File Name doesn`t exists
                {
                    // result = FormOcrWcf.Program.ProcessForms(DirPath);//, runID);
                    result = FormOcrWcf.Program.ProcessForms(DirPath, runID);
                    dtOutput = ResultToDataTable(result);
                }
                else//If File Name provided
                {
                    var targetDocInput = Path.Combine(DirPath, "OCRInput");
                    result = FormOcrWcf.Program.ProcessFiles(targetDocInput, new[] { FileName }, runID);
                    //result=FormOcrWcf.Program.ProcessFiles(targetDocInput, new[] { FileName });
                    //dtOutput = ResultToDataTable(result);
                }

                using (DBEntities db = new DBEntities())
                {
                    var runIdParam = new SqlParameter("@RunID", runID);
                    IEnumerable<Result> details = db.Database.SqlQuery
                                                                              <Result>("exec proc_ApplicationSummary_GetResultByRunID @RunID", runIdParam).ToList();

                    dtOutput = CreateDataTable(details);
                    //dtOutput =detail
                }

                dtOutput.TableName = "LateetudRuleApplication";
                str = ConvertDatatableToXML(dtOutput);
                #region End Entry into APICallHistory
                //APICallHistory apiCallHstStart = new APICallHistory();
                if (dtOutput.Rows.Count > 0)
                {
                    strStatus = dtOutput.Rows.Count + " Rows Successfully returned";
                }
                else
                {
                    strStatus = "No Records returned";
                }
                //apiCallHstStart.RunID = Convert.ToInt64(runID);
                //apiCallHstStart.EndTime = DateTime.Now;
                //apiCallHstStart.Comment = "Call For XML";

                apiCallHstStart.EndTime = DateTime.Now;
                apiCallHstStart.Status = strStatus;
                using (DBEntities db = new DBEntities())
                {
                    db.Entry(apiCallHstStart).State = EntityState.Modified;

                    db.SaveChanges();
                }
                #endregion

                #region Entry into Summary
                //MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                //foreach (DataRow row in dtOutput.Rows)
                //{
                //    applicationSummary.RunID = Convert.ToInt64(runID);
                //    applicationSummary.EntryDate = DateTime.Now;
                //    applicationSummary.FieldKey = Convert.ToString(row["FieldName"]);
                //    applicationSummary.FieldValue = Convert.ToString(row["FieldValue"]);
                //    using (DBEntities db = new DBEntities())
                //    {
                //        db.MasterFormApplicationSummaries.Add(applicationSummary);
                //        db.SaveChanges();
                //    }

                //}

                #endregion


            }
            catch (Exception ex)
            {
                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For XML Method for Run ID: " + runID + "";

                using (DBEntities db = new DBEntities())
                {
                    db.ExceptionLogs.Add(log);
                    db.SaveChanges();
                }

                #endregion

                #region Update History table
                APICallHistory apiCallHstStart = new APICallHistory();
                apiCallHstStart.RunID = runID;
                using (DBEntities db = new DBEntities())
                {
                    apiCallHstStart = db.APICallHistories.Where(x => x.RunID == apiCallHstStart.RunID).FirstOrDefault();
                    apiCallHstStart.EndTime = DateTime.Now;
                    apiCallHstStart.Status = "Exception";
                    db.Entry(apiCallHstStart).State = EntityState.Modified;

                    db.SaveChanges();
                }
                #endregion

                WriteToFile("RuleEngine Error on: {0} " + ex.Message + ex.StackTrace);
            }
            return str;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetResultInJSON(string FileName)
        {
            string strStatus = "START";
            long runID;
            string jsonResult = string.Empty;

            using (DBEntities context = new DBEntities())
            {
                IEnumerable<CurrentRun> details = context.Database.SqlQuery
                                                                              <CurrentRun>("exec proc_APICallHistory_GetRunID").ToList();
                runID = Convert.ToInt64(details.FirstOrDefault().RunID);
            }
            try
            {
                #region Entry into APICallHistory


                APICallHistory apiCallHstStart = new APICallHistory();

                apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.StartTime = DateTime.Now;
                apiCallHstStart.Comment = "Call For JSON";
                apiCallHstStart.Status = strStatus;//"START";
                using (DBEntities db = new DBEntities())
                {
                    db.APICallHistories.Add(apiCallHstStart);
                    db.SaveChanges();
                }
                #endregion

                string DirPath = GetEnvironmentVariable("SMART");
                AutoFormsRecognizeFormResult result = new AutoFormsRecognizeFormResult();
                DataTable dtOutput = new DataTable();
                if (string.IsNullOrEmpty(FileName))//If File Name doesn`t exists
                {
                    // result = FormOcrWcf.Program.ProcessForms(DirPath);//, runID);
                    result = FormOcrWcf.Program.ProcessForms(DirPath, runID);
                    dtOutput = ResultToDataTable(result);
                }
                else//If File Name provided
                {
                    var targetDocInput = Path.Combine(DirPath, "OCRInput");
                    result = FormOcrWcf.Program.ProcessFiles(targetDocInput, new[] { FileName }, runID);
                    //result=FormOcrWcf.Program.ProcessFiles(targetDocInput, new[] { FileName });
                    //dtOutput = ResultToDataTable(result);
                }

                using (DBEntities db = new DBEntities())
                {
                    var runIdParam = new SqlParameter("@RunID", runID);
                    IEnumerable<Result> details = db.Database.SqlQuery
                                                                              <Result>("exec proc_ApplicationSummary_GetResultByRunID @RunID", runIdParam).ToList();

                    dtOutput = CreateDataTable(details);
                    //dtOutput =detail
                }

                dtOutput.TableName = "LateetudRuleApplication";
                JsonSerializerSettings jss = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                jsonResult = JsonConvert.SerializeObject(dtOutput, Newtonsoft.Json.Formatting.Indented, jss);

                #region End Entry into APICallHistory
                if (dtOutput.Rows.Count > 0)
                {
                    strStatus = dtOutput.Rows.Count + " Rows Successfully returned";
                }
                else
                {
                    strStatus = "No Records returned";
                }
                //APICallHistory apiCallHstStart = new APICallHistory();

                //apiCallHstStart.RunID = Convert.ToInt64(runID);
                apiCallHstStart.EndTime = DateTime.Now;
                //apiCallHstStart.Comment = "Call For JSON";
                apiCallHstStart.Status = strStatus;//"END";
                using (DBEntities db = new DBEntities())
                {
                    db.Entry(apiCallHstStart).State = EntityState.Modified;
                    db.SaveChanges();
                }
                #endregion

                #region Entry into Summary
                MasterFormApplicationSummary applicationSummary = new MasterFormApplicationSummary();

                foreach (DataRow row in dtOutput.Rows)
                {
                    applicationSummary.RunID = Convert.ToInt64(runID);
                    applicationSummary.EntryDate = DateTime.Now;
                    applicationSummary.FieldKey = Convert.ToString(row["FieldName"]);
                    applicationSummary.FieldValue = Convert.ToString(row["FieldValue"]);
                    using (DBEntities db = new DBEntities())
                    {
                        db.MasterFormApplicationSummaries.Add(applicationSummary);
                        db.SaveChanges();
                    }

                }

                #endregion
            }
            catch (Exception ex)
            {
                #region Update History table
                APICallHistory apiCallHstStart = new APICallHistory();
                apiCallHstStart.RunID = runID;
                using (DBEntities db = new DBEntities())
                {
                    apiCallHstStart = db.APICallHistories.Where(x => x.RunID == apiCallHstStart.RunID).FirstOrDefault();
                    apiCallHstStart.EndTime = DateTime.Now;
                    apiCallHstStart.Status = "Exception";
                    db.Entry(apiCallHstStart).State = EntityState.Modified;

                    db.SaveChanges();
                }
                #endregion

                #region Error Log
                ExceptionLog log = new ExceptionLog();
                log.ErrorTime = DateTime.Now;
                log.ErrorMessage = Convert.ToString(ex.Message);
                log.Comments = "Error at Call For JSON Method for Run ID: " + runID + "";

                using (DBEntities db = new DBEntities())
                {
                    db.ExceptionLogs.Add(log);
                    db.SaveChanges();
                }

                #endregion

                WriteToFile("RuleEngine Error on: {0} " + ex.Message + ex.StackTrace);
            }

            return jsonResult;

        }
        private static string PrintToString(AutoFormsRecognizeFormResult result)
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

                    if (textField != null)
                    {


                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(",", " ");
                        ((TextFormFieldResult)textField.Result).Text = ((TextFormFieldResult)textField.Result).Text?.Replace(System.Environment.NewLine, " ");
                        fieldname = textField.Name;
                        fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;

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

                        fieldname = omrField.Name;
                        fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;

                        ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                    }
                }
            }
            return ocrResult;
        }


        private static DataTable ResultToDataTable(AutoFormsRecognizeFormResult result)
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

            //string ocrResult = String.Empty;
            //string openBrac = "[[[";
            //string closeBrac = "]]]";
            //string resDenoter = ":::";

            DataTable dtResult = new DataTable("Result");
            dtResult.Columns.Add("FieldName", typeof(string));
            dtResult.Columns.Add("FieldValue", typeof(string));

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
                        fieldname = textField.Name;
                        fieldvalue = ((TextFormFieldResult)(textField.Result)).Text?.Trim();

                        DataRow dr = dtResult.NewRow();
                        dr["FieldName"] = fieldname;
                        dr["FieldValue"] = fieldvalue;
                        dtResult.Rows.Add(dr);
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

                        fieldname = omrField.Name;
                        fieldvalue = ((OmrFormFieldResult)(omrField.Result)).Text;

                        //ocrResult += openBrac + fieldname + resDenoter + fieldvalue + closeBrac;
                        DataRow dr = dtResult.NewRow();
                        dr["FieldName"] = fieldname;
                        dr["FieldValue"] = fieldvalue;
                        dtResult.Rows.Add(dr);
                    }
                }
            }
            return dtResult;
        }

        private string ConvertDatatableToXML(DataTable dt)
        {
            MemoryStream str = new MemoryStream();
            dt.WriteXml(str, true);
            str.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(str);
            string xmlstr;
            xmlstr = sr.ReadToEnd();
            return (xmlstr);
        }

        private void WriteToFile(string text)
        {
            string filePath = ConfigurationManager.AppSettings["LogFile"].ToString();
            //string path = "E:\\ServiceLog.txt";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            }
        }
        public static DataTable CreateDataTable(IEnumerable source)
        {
            var table = new DataTable();
            int index = 0;
            var properties = new List<PropertyInfo>();
            foreach (var obj in source)
            {
                if (index == 0)
                {
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            continue;
                        }
                        properties.Add(property);
                        table.Columns.Add(new DataColumn(property.Name, property.PropertyType));
                    }
                }
                object[] values = new object[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    values[i] = properties[i].GetValue(obj);
                }
                table.Rows.Add(values);
                index++;
            }
            return table;
        }

        public class CurrentRun
        {
            public long RunID { get; set; }

        }
        public class Result
        {
            public string FileName { get; set; }
            public string FieldKey { get; set; }
            public string FieldValue { get; set; }

        }
    }
}

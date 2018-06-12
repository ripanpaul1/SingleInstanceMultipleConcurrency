using Newtonsoft.Json;
using System;
using System.Web.Script.Services;
using System.Web.Services;

namespace OcrServiceClient
{
    /// <summary>
    /// Summary description for OcrService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class OcrService : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetResult(string FileOrDir)
        {
            try
            {
                LeadToolsEngineService.VMProcessResult vMProcessResult = new LeadToolsEngineService.EngineServiceClient("BasicHttpBinding_IEngineService").ProcessFiles(FileOrDir);
                if (vMProcessResult == null) return "Failed the process";
                return JsonConvert.SerializeObject(vMProcessResult);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}

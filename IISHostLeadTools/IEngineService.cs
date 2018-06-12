using System.ServiceModel;

namespace IISHostLeadTools
{
    [ServiceContract]
    public interface IEngineService
    {
        [OperationContract]
        VMProcessResult ProcessFiles(string FileOrDir);
    }
    
}

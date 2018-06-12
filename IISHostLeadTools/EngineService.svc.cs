using System.ServiceModel;

namespace IISHostLeadTools
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true)]
    public class EngineService : IEngineService
    {
        public LeadToolsUtilities leadToolsUtilities = null;
        public EngineService()
        {
            leadToolsUtilities = new LeadToolsUtilities();
        }
        public VMProcessResult ProcessFiles(string FileOrDir)
        {
            return leadToolsUtilities.ProcessFiles(FileOrDir);
        }
    }
}

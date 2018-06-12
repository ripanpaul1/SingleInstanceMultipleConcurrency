using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IISHostLeadTools
{
    [DataContract]
    public class VMProcessResult
    {
        public VMProcessResult()
        {
            this.FileResult = null;
        }
        public VMProcessResult(List<VMFileResult> vMFileResult)
        {
            this.FileResult = vMFileResult;
        }

        [DataMember(Order = 1)]
        public List<VMFileResult> FileResult { get; set; }
    }
}
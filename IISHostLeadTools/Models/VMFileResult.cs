using System.Collections.Generic;
using System.Runtime.Serialization;

namespace IISHostLeadTools
{
    [DataContract]
    public class VMFileResult
    {
        public VMFileResult()
        {
            this.FileName = null;
            this.FieldResult = null;
            this.Status = "Nothing";
        }
        public VMFileResult(string FileName)
        {
            this.FileName = FileName;
            this.FieldResult = null;
            this.Status = "Nothing";
        }
        public VMFileResult(string FileName, List<VMFieldResult> FieldResult)
        {
            this.FileName = FileName;
            this.FieldResult = FieldResult;
            this.Status = "Nothing";
        }
        public VMFileResult(string FileName, string Status)
        {
            this.FileName = FileName;
            this.FieldResult = null;
            this.Status = Status;
        }
        public VMFileResult(string FileName, List<VMFieldResult> FieldResult, string Status)
        {
            this.FileName = FileName;
            this.FieldResult = FieldResult;
            this.Status = Status;
        }

        [DataMember(Order = 1)]
        public string FileName { get; set; }

        [DataMember(Order = 2)]
        public List<VMFieldResult> FieldResult { get; set; }

        [DataMember(Order = 3)]
        public string Status { get; set; }
    }
}
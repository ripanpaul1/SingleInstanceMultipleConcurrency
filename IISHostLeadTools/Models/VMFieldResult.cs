using System.Runtime.Serialization;

namespace IISHostLeadTools
{
    [DataContract]
    public class VMFieldResult
    {
        public VMFieldResult()
        {
            this.FieldName = null;
            this.FieldValue = null;
        }
        public VMFieldResult(string FieldName, string FieldValue)
        {
            this.FieldName = FieldName;
            this.FieldValue = FieldValue;
        }

        [DataMember(Order = 1)]
        public string FieldName { get; set; }

        [DataMember(Order = 2)]
        public string FieldValue { get; set; }
    }
}
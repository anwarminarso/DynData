using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysJsonSerial = System.Text.Json.Serialization;

#nullable disable
namespace a2n.DynData
{
    public class PagingRequest
    {
        public int pageIndex { get; set; } = 0;
        public int pageSize { get; set; } = 20;

        [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExpressionRule[] rules { get; set; } = null;

        [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? orderBy { get; set; } = null;

        [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ascending { get; set; } = null;
    }
    //public class SimpleExpressionRule
    //{
    //    [JsonConverter(typeof(StringEnumConverter))]
    //    [SysJsonSerial.JsonConverter(typeof(SysJsonSerial.JsonStringEnumConverter))]
    //    public ExpressionLogicalOperator LogicalOperator { get; set; }

    //    [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingDefault)]
    //    public bool IsBracket { get; set; } = false;

    //    public string ReferenceFieldName { get; set; }

    //    [JsonConverter(typeof(StringEnumConverter))]
    //    [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //    public ExpressionOperator Operator { get; set; }

    //    [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //    public object CompareFieldObject { get; set; }

    //    [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //    public string CompareFieldValue { get; set; }

    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //    public SimpleExpressionRule[] Children { get; set; }


    //}
}

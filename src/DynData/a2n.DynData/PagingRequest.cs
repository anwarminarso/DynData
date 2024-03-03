using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysJsonSerial = System.Text.Json.Serialization;


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
        public string orderBy { get; set; } = null;

        [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? ascending { get; set; } = null;

        [SysJsonSerial.JsonIgnore(Condition = SysJsonSerial.JsonIgnoreCondition.WhenWritingNull)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object userContext { get; set; } = null;
    }


}

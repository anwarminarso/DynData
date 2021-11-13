using Newtonsoft.Json;
#nullable disable

namespace a2n.DynData
{
    public class PagingResult<T>
    {
        public T[] items { get; set; } = new T[0];

        public int pageSize { get; set; } = 20;
        public int pageIndex { get; set; } = 0;
        public int totalPages { get; set; } = 0;
        public int totalRows { get; set; } = 0;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object context { get; set; }
    }
    public class PagingResult
    {
        public object[] items { get; set; } = new object[0];

        public int pageSize { get; set; } = 20;
        public int pageIndex { get; set; } = 0;
        public int totalPages { get; set; } = 0;
        public int totalRows { get; set; } = 0;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object context { get; set; }
    }
}

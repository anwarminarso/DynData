using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace a2n.DynData
{
    public class DatabaseServer
    {
        public string ConnectionString { get; set; }
        
        public string DefaultSchema { get; set; } = "";

        [JsonConverter(typeof(StringEnumConverter))]
        public DatabaseProvider Provider { get; set; }
    }
    public enum DatabaseProvider
    {
        SqlServer,
        Postgres,
        MySql,
        Sqlite
    }
}

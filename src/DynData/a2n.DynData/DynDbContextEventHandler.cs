using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a2n.DynData
{
    public abstract class DynDbContextEventHandler
    {
        public virtual string[] AvailableExportFormats => new string[] { "csv", "xlsx" };
        public virtual void OnMetaGenerated(Metadata meta)
        {
        }
        public virtual bool OnBeforeCreate(DynDbContext db, Type valueType, object value)
        {
            return true;
        }
        public virtual bool OnBeforeUpdate(DynDbContext db, Type valueType, object originalValue, JObject valueToModified)
        {
            return true;
        }
        public virtual void OnAfterUpdate(DynDbContext db, Type valueType, object modifiedValue)
        {
        }
        public virtual bool OnBeforeDelete(DynDbContext db, Type valueType, object value)
        {
            return true;
        }
        public virtual void OnExport(string format, string viewName,
           Type valueType, Metadata[] metadataArr,
           IQueryable<dynamic> qry,
           out byte[] buffer, out string mimeType, out string fileName)
        {
            DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
        }
    }
}

using a2n.DynData;
using Newtonsoft.Json.Linq;
using AdvWorks.DataAccess;

namespace AdvWorks.WebUI.EventHandlers
{
    public class AdvWorksDbContextHandler : DynDbContextEventHandler
    {
        public override void OnMetaGenerated(Metadata meta)
        {


        }
        public override bool OnBeforeCreate(DynDbContext db, Type valueType, object value)
        {
            var ctxt = db as AdvWorksDbContext;
            return true;
        }
        public override bool OnBeforeDelete(DynDbContext db, Type valueType, object value)
        {
            var ctxt = db as AdvWorksDbContext;

            return true;
        }
        public override bool OnBeforeUpdate(DynDbContext db, Type valueType, object originalValue, JObject valueToModified)
        {
            var ctxt = db as AdvWorksDbContext;

            return true;
        }
        public override void OnAfterUpdate(DynDbContext db, Type valueType, object modifiedValue)
        {
            var ctxt = db as AdvWorksDbContext;

        }
        public override void OnExport(string format, string viewName, Type valueType, Metadata[] metadataArr, IQueryable<dynamic> qry, out byte[] buffer, out string mimeType, out string fileName)
        {
            base.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
        }
    }
}

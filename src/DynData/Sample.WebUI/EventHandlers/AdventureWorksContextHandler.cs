using a2n.DynData;
using Newtonsoft.Json.Linq;
using Sample.DataAccess;

namespace Sample.WebUI.EventHandlers
{
    public class AdventureWorksContextHandler : DynDbContextEventHandler
    {
        public override bool OnBeforeCreate(DynDbContext db, Type valueType, object value)
        {
            var ctxt = db as AdventureWorksContext;
            return true;
        }
        public override bool OnBeforeDelete(DynDbContext db, Type valueType, object value)
        {
            var ctxt = db as AdventureWorksContext;

            return true;
        }
        public override bool OnBeforeUpdate(DynDbContext db, Type valueType, object originalValue, JObject valueToModified)
        {
            var ctxt = db as AdventureWorksContext;

            return true;
        }
        public override void OnAfterUpdate(DynDbContext db, Type valueType, object modifiedValue)
        {
            var ctxt = db as AdventureWorksContext;

        }
    }
}

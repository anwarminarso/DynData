using a2n.DynData;
using Microsoft.AspNetCore.Http;

namespace Sample.WebUI.Security
{

    public class APIAuth : IDynDataAPIAuth
    {
        public void ApplyRequest(HttpContext context, DynDbContext db, string ViewName, DataTableJSRequest req)
        {
        }

        public void ApplyRequest(HttpContext context, DynDbContext db, string ViewName, DataTableJSExportRequest req)
        {
        }

        public bool IsAllowed(HttpContext context, string controllerName, DynDbContext db, DynDataAPIMethod method, string viewName)
        {
            string userName = String.Empty;
            if (context != null && context.User != null && context.User.Identity.IsAuthenticated)
                userName = context.User.Identity.Name;

            switch (method)
            {
                case DynDataAPIMethod.ViewNames:
                    break;
                case DynDataAPIMethod.DataTable:
                    break;
                case DynDataAPIMethod.DataTableExport:
                    break;
                case DynDataAPIMethod.List:
                    break;
                case DynDataAPIMethod.Export:
                    break;
                case DynDataAPIMethod.DropDown:
                    break;
                case DynDataAPIMethod.Read:
                    break;
                case DynDataAPIMethod.Create:
                    break;
                case DynDataAPIMethod.Update:
                    break;
                case DynDataAPIMethod.Delete:
                    break;
                case DynDataAPIMethod.Metadata:
                    break;
                case DynDataAPIMethod.MetadataQB:
                    break;
                default:
                    break;
            }
            // return false if user not authorize access

            return true;
        }

    }
}

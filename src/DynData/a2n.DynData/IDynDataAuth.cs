using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a2n.DynData
{
    public interface IDynDataAPIAuth
    {
        public bool IsAllowed(HttpContext context, string controllerName, DynDbContext db, DynDataAPIMethod method, string viewName);
        public void ApplyRequest(HttpContext context, DynDbContext db, string ViewName, DataTableJSRequest req);
        public void ApplyRequest(HttpContext context, DynDbContext db, string ViewName, DataTableJSExportRequest req);
    }
    public enum DynDataAPIMethod
    {
        ViewNames,
        DataTable,
        DataTableExport,
        List,
        Export,
        DropDown,
        Read,
        Create,
        Update,
        Delete,
        Metadata,
        MetadataQB
    }
}

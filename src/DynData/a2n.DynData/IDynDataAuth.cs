using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a2n.DynData
{
    public interface IDynDataAPIAuth
    {
        public bool IsAllowed(HttpContext context, DynDbContext db, DynDataAPIMethod method, string viewName);
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

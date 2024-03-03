using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Northwind.DataAccess;
using Northwind.WebUI.Configuration;

namespace Northwind.WebUI.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly NorthwindDbContext db;
        public readonly string[] AllTables;

        public IndexModel(ILogger<IndexModel> logger, NorthwindDbContext db, NorthwindQueryTemplate qryTpl)
        {
            this.logger = logger;
            this.db = db;
            AllTables = db.GetAllTableViewNames();
        }

        public void OnGet()
        {

        }
    }
}

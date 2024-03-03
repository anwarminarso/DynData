using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using AdvWorks.DataAccess;
using AdvWorks.WebUI.Configuration;

namespace AdvWorks.WebUI.Pages
{
    [Authorize()]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly AdvWorksDbContext db;
        public readonly string[] AllTables;

        public IndexModel(ILogger<IndexModel> logger, AdvWorksDbContext db, AdvWorkQueryTemplate qryTpl)
        {
            this.logger = logger;
            this.db = db;
            List<string> qryTblVwNameLst = new List<string>();
            qryTblVwNameLst.AddRange(db.GetAllTableViewNames());
            qryTblVwNameLst.AddRange(qryTpl.GetQueryTemplateNames());
            AllTables = qryTblVwNameLst.ToArray();
        }

        public void OnGet()
        {
            
        }
    }
}
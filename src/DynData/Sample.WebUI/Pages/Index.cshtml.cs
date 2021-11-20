using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Sample.DataAccess;
using Sample.WebUI.Configuration;

namespace Sample.WebUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly AdventureWorksContext db;
        public readonly string[] AllTables;

        public IndexModel(ILogger<IndexModel> logger, AdventureWorksContext db, AdvWorkQueryTemplate qryTpl)
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
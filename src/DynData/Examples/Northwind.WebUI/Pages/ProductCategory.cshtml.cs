using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Northwind.WebUI.Pages
{
    [Authorize]
    public class ProductCategoryModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

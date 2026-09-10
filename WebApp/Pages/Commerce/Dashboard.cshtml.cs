using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Commerce
{
    public class DashboardModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CommerceId { get; set; }

        public void OnGet()
        {
            // El CommerceId se obtendrá automáticamente del query string
        }
    }
}
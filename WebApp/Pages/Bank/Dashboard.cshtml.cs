using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Bank
{
    public class DashboardModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int BankId { get; set; }

        public void OnGet()
        {
            // El BankId se obtendrá automáticamente del query string
        }
    }
}
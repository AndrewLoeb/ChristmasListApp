using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class UserPageModel : PageModel
    {
        [FromQuery]
        public string Token { get; set; }

        public void OnGet()
        {
            // Token is automatically bound from query string
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AP.Platform.Pages;

public class IndexModel : PageModel
{
    public string? AccessToken { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }

    public string? Message { get; set; }
    public string? Error { get; set; }

    public IActionResult OnGet(string? accessToken, string? fullName, string? email)
    {
        if (!HttpContext.User.Identity!.IsAuthenticated)
        {
            return RedirectToPage("/Login");
        }
        else
        {
            this.Message = "Welcome to RE !!!";

            this.AccessToken = accessToken ?? string.Empty;
            this.FullName = fullName ?? string.Empty;
            this.Email = email ?? string.Empty;
        }

        return Page();
    }
}
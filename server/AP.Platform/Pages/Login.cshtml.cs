using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AP.Platform.Pages;

public class LoginModel() : PageModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool? IsVerified { get; set; }

    public string? Message { get; set; }
    public string? Error { get; set; }

    public IActionResult OnGet(string? email, bool? isVerified, string? error)
    {
        this.Message = "Welcome to RE !!!";

        this.Email = email ?? string.Empty;
        this.Password ??= string.Empty;
        this.IsVerified = isVerified ?? true;

        this.Error = error ?? string.Empty;

        return Page();
    }
}
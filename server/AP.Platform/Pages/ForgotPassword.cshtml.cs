using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AP.Platform.Pages;

public class ForgotPasswordModel : PageModel
{
    public string? Email { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? email, string? error)
    {
        this.Message = "Welcome to RE !!!";

        this.Email = email ?? string.Empty;

        this.Error = error ?? string.Empty;
    }
}
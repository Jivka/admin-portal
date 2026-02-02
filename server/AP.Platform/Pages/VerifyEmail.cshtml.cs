using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AP.Platform.Pages;

public class VerifyEmailModel : PageModel
{
    public string? VerificationCode { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? code, string? email, string? password, string? confirmPassword, string? error)
    {
        this.Message = "Welcome to RE !!!";

        this.VerificationCode = code ?? string.Empty;
        this.Email = email ?? string.Empty;
        this.Password = password ?? string.Empty;
        this.ConfirmPassword = confirmPassword ?? string.Empty; 

        this.Error = error ?? string.Empty;
    }
}
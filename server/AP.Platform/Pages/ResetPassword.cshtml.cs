using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AP.Platform.Pages;

public class ResetPasswordModel : PageModel
{
    public string? Email { get; set; }
    public string? ResetCode { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? email, string? resetCode, string? newPassword, string? confirmPassword, string? error)
    {
        this.Message = "Welcome to RE !!!";

        this.Email = email ?? string.Empty;
        this.ResetCode = resetCode ?? string.Empty;
        this.NewPassword = newPassword ?? string.Empty;
        this.ConfirmPassword = confirmPassword ?? string.Empty;

        this.Error = error ?? string.Empty;
    }
}
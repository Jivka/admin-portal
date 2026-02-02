using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AP.Platform.Pages;

public class SignupModel : PageModel
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    ////public string? Password { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? email, string? fname, string lname, string? error)
    {
        this.Message = "Welcome to RE !!!";

        this.Email = email ?? string.Empty;
        this.FirstName = fname ?? string.Empty;
        this.LastName = lname ?? string.Empty;
        ////this.Password = password ?? string.Empty;

        this.Error = error ?? string.Empty;
    }
}
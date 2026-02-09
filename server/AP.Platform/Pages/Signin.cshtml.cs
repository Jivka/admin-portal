using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Platform.Pages;

public class SigninModel(IIdentityService identityService) : PageModel
{
    public string? ErrorMessage { get; set; }

    public async Task OnGet(string email, string password, bool? external, string? idp)
    {
        await SigninUserInternal(email, password);
    }

    private async Task SigninUserInternal(string email, string password)
    {
        // Cookies are now set by the IdentityService
        await identityService.SignIn(new SigninRequest(email, password)
        {
            Email = email,
            Password = password
        }, Request.Headers.Origin);
    }
}
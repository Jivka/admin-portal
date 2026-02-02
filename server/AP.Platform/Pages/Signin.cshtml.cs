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
        var tokens = await identityService.SignIn(new SigninRequest(email, password)
        {
            Email = email,
            Password = password
        }, Request.Headers.Origin);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(tokens.Data.JwtToken);
        var identity = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));
    }
}
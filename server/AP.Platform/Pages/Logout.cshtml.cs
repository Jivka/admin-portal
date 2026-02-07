using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static AP.Common.Constants.Constants;

namespace AP.Platform.Pages;

public class LogoutModel() : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Clear authentication cookies
        Response.Cookies.Delete(AuthenticationCookieName);
        Response.Cookies.Delete(RefreshTokenCookieName);

        return RedirectToPage("/Login");

        ////accountService.AddUserLog(currentUser.UserId, currentUser.Email, "Logged Out", true, currentUser.IpAddress);
    }
}
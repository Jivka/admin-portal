using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AP.Common.Models;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.ApiErrorMessages;

namespace AP.Identity.Internal.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("auth")]
public class LoginController(IIdentityService identityService/*, IHttpContextAccessor httpContextAccessor*/) : Controller
{
    private const string LoginPage = "/login";
    private const string SignInPage = "/signin";
    private const string SignUpPage = "/signup";
    private const string VerifyEmailPage = "/verifyemail";
    private const string ForgotPasswordPage = "/forgotpassword";
    private const string ResetPasswordPage = "/resetpassword";

    [HttpGet("login")]
    [HttpPost("login")]
    public IActionResult LoginUser(string? email)
    {
        return RedirectToPage(
            LoginPage,
            new
            {
                email,
            });
    }

    [HttpPost("internal")]
    public async Task<IActionResult> LoginUserInternal(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return RedirectToPage(LoginPage, new { email, error = "Email and password are required for authentication" });
        }

        var result = await identityService.SignIn(new SigninRequest(email, password)
        {
            Email = email,
            Password = password
        }, Origin());

        if (!result.Succeeded)
        {
            var isVerified = !result.Error?.Code.Equals(NotVerifiedEmail.Code);
            return RedirectToPage(LoginPage, new { email, isVerified, error = GetErrorMessage(result) });
        }

        // Authentication cookies are already set by IdentityService
        return RedirectToPage(
            "/index",
            new
            {
                fullName = result.Data.FullName,
                email = result.Data.Email
            });
    }

    [HttpPost("social")]
    public IActionResult LoginUserSocial(string? idp)
    {
        return RedirectToPage(SignInPage, new { idp });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage(ForgotPasswordPage, new { email, error = "Email is required to send the reset token" });
        }

        var result = await identityService.ForgotPassword(new ForgotPasswordRequest(email) { Email = email}, Origin());
        if (!result.Succeeded)
        {
            return RedirectToPage(ForgotPasswordPage, new { email, error = GetErrorMessage(result) });
        }

        return RedirectToPage(ResetPasswordPage, new { email, resetCode = string.Empty });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(string? email, string? code, string? newPassword, string? confirmationPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage(ResetPasswordPage, new { email, resetCode = code, error = "Email is required" });
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToPage(ResetPasswordPage, new { email, resetCode = code, error = "Reset code is required" });
        }
        if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmationPassword))
        {
            return RedirectToPage(ResetPasswordPage, new { email, resetCode = code, error = "New password and old password are required" });
        }
        if (newPassword != confirmationPassword)
        {
            return RedirectToPage(ResetPasswordPage, new { email, resetCode = code, error = "New password and confirmation password do not match" });
        }

        var result = await identityService.ResetPassword(new ResetPasswordRequest(email, code, newPassword, confirmationPassword)
        {
            Email = email,
            ResetToken = code,
            Password = newPassword,
            ConfirmPassword = confirmationPassword
        });
        if (!result.Succeeded)
        {
            return RedirectToPage(ResetPasswordPage, new { email, resetCode = code, error = GetErrorMessage(result) });
        }

        return RedirectToPage(LoginPage, new { email });
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUpUser(string fname, string lname, string? email/*, string? password*/)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage(SignUpPage, new { email, error = "Email is required for sign up" });
        }

        try
        {
            await System.Net.Dns.GetHostEntryAsync(email.Split('@')[1]);
        }
        catch (Exception ex)
        {
            return RedirectToPage(SignUpPage, new { email, error = ex.Message });
        }

        var result = await identityService.SignUpUser(new SignupRequest(fname, lname, email)
        { 
            FirstName = fname, 
            LastName = lname,
            Email = email,
        }, Origin());
        if (!result.Succeeded)
        {
            return RedirectToPage(SignUpPage, new { email, error = GetErrorMessage(result) });
        }

        return RedirectToPage(VerifyEmailPage, new { email });
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyEmail(string? code, string? email, string? password, string? confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToPage(VerifyEmailPage, new { code, email, error = "Verification code is required" });
        }
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage(VerifyEmailPage, new { code, email, error = "Email is required" });
        }
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return RedirectToPage(VerifyEmailPage, new { code, email, error = "Password and confirmation password are required" });
        }
        if (password != confirmPassword)
        {
            return RedirectToPage(VerifyEmailPage, new { code, email, error = "New password and confirmation password do not match" });
        }

        var result = await identityService.VerifyEmail(new VerifyEmailRequest(code, email, password, confirmPassword)
        {
            Email = email,
            VerificationToken = code,
            Password = password,
            ConfirmPassword = confirmPassword
        });

        if (!result.Succeeded)
        {
            return RedirectToPage(VerifyEmailPage, new { code, email, error = GetErrorMessage(result) });
        }

        return RedirectToPage(LoginPage, new { email });
    }

    [HttpPost("resend-verification-code")]
    public async Task<ActionResult> ResendVerificationCode(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage(VerifyEmailPage, new { email, error = "Email is required to send a new code" });
        }

        var result = await identityService.ResendVerificationCode(email, Origin());
        if (!result.Succeeded)
        {
            return RedirectToPage(VerifyEmailPage, new { email, error = GetErrorMessage(result) });
        }

        return RedirectToPage(VerifyEmailPage, new { email, intarnal = true });
    }

    private string? Origin()
    {
        var origin = HttpContext.Request.Headers.Origin;

        return !string.IsNullOrEmpty(origin)
            ? origin
            : default!;
    }

    private static string? GetErrorMessage(ApiResult result)
    {
        return result.Error?.Message ?? result.Errors?[0].Message;
    }
}
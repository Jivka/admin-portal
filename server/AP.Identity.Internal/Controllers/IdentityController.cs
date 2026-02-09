using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;

namespace AP.Identity.Internal.Controllers;

[ApiController]
[Route("identity")]
[AllowAnonymous]
public class IdentityController(IIdentityService identity) : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task<ActionResult<UserOutput>> SignUp(SignupRequest model)
    {
        var result = await identity.SignUpUser(model, Origin());

        return result;
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<UserOutput>> VerifyEmail(VerifyEmailRequest model)
    {
        var result = await identity.VerifyEmail(model);

        return result;
    }

    [HttpPost("resend-verification-code")]
    public async Task<ActionResult<string>> ResendVerificationCode(string email)
    {
        var result = await identity.ResendVerificationCode(email, Origin());

        return result;
    }

    [HttpPost("sign-in")]
    public async Task<ActionResult<SigninResponse>> SignIn(SigninRequest model)
    {
        var result = await identity.SignIn(model, IpAddress());

        return result;
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<SigninResponse>> RefreshToken()
    {
        // Read refresh token from cookie
        var refreshToken = HttpContext.Request.Cookies["RefreshToken"];
        var email = User.Identity?.Name;

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var model = new RefreshTokenRequest(email, refreshToken)
        {
            Email = email,
            RefreshToken = refreshToken
        };

        var result = await identity.RefreshToken(model, IpAddress());

        return result;
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<string>> ForgotPassword(ForgotPasswordRequest model)
    {
        var result = await identity.ForgotPassword(model, Origin());

        return result;
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<string>> ResetPassword(ResetPasswordRequest model)
    {
        var result = await identity.ResetPassword(model);

        return result;
    }

    private string? IpAddress()
    {
        var forwardedHeader = HttpContext.Request.Headers[ForwardedHeadersDefaults.XForwardedForHeaderName];
        if (!StringValues.IsNullOrEmpty(forwardedHeader))
        {
            return forwardedHeader.FirstOrDefault();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? Origin()
    {
        var origin = HttpContext.Request.Headers.Origin;

        return !string.IsNullOrEmpty(origin)
            ? origin
            : default!;
    }
}
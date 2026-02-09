using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AP.Common.Models;
using AP.Identity.Internal.Controllers;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.NoticeMessages;

namespace AP.Identity.Internal.Tests.Controllers;

public class IdentityControllerTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly IdentityController _controller;

    public IdentityControllerTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _controller = new IdentityController(_identityServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task SignUp_ReturnsExpectedResult()
    {
        // Arrange
        var model = new SignupRequest("Test", "User", "test@example.com")
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };
        var expectedResult = ApiResult<UserOutput>.SuccessWith(new UserOutput());
        _identityServiceMock.Setup(s => s.SignUpUser(model, It.IsAny<string>())).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SignUp(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult.Data, okResult.Value);
    }

    [Fact]
    public async Task VerifyEmail_ReturnsExpectedResult()
    {
        // Arrange
        var model = new VerifyEmailRequest("123456", "test@example.com", "Password*123", "Password*123")
        {
            Email = "test@example.com",
            VerificationToken = "123456",
            Password = "Password*123",
            ConfirmPassword = "Password*123"
        };
        var expectedResult = ApiResult<UserOutput>.SuccessWith(new UserOutput());
        _identityServiceMock.Setup(s => s.VerifyEmail(model)).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.VerifyEmail(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult.Data, okResult.Value);
    }

    [Fact]
    public async Task SignIn_ReturnsExpectedResult()
    {
        // Arrange
        var model = new SigninRequest("test@example.com", "Password123")
        { 
            Email = "test@example.com",
            Password = "Password123",
        };
        var expectedResult = ApiResult<SigninResponse>.SuccessWith(new SigninResponse { Email = "test@example.com" });
        _identityServiceMock.Setup(s => s.SignIn(model, It.IsAny<string>())).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.SignIn(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<SigninResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult.Data, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = ApiResult<SigninResponse>.SuccessWith(new SigninResponse { Email = "test@example.com" });
        var model = new RefreshTokenRequest("test@example.com", "123456")
        {
            Email = "test@example.com",
            RefreshToken = "123456",
        };
        _identityServiceMock.Setup(s => s.RefreshToken(It.IsAny<RefreshTokenRequest>(), It.IsAny<string>())).ReturnsAsync(expectedResult);
        
        // Mock cookies and user identity
        _controller.ControllerContext.HttpContext.Request.Headers.Cookie = "RefreshToken=123456";
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "test@example.com") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        _controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        var actionResult = Assert.IsType<ActionResult<SigninResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult.Data, okResult.Value);
    }

    [Fact]
    public async Task ForgotPassword_ReturnsExpectedResult()
    {
        // Arrange
        var model = new ForgotPasswordRequest("test@example.com") { Email = "test@example.com" };
        var expectedResult = ApiResult<string>.SuccessWith(EmailWithPasswordResetInstructions);
        _identityServiceMock.Setup(s => s.ForgotPassword(model, It.IsAny<string>())).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ForgotPassword(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result, exactMatch: false);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(EmailWithPasswordResetInstructions, okResult.Value);
    }

    [Fact]
    public async Task ResetPassword_ReturnsExpectedResult()
    {
        // Arrange
        var model = new ResetPasswordRequest("test@example.com", "123456", "NewPassword*123", "NewPassword*123")
        {
            Email = "test@example.com",
            ResetToken = "123456",
            Password = "NewPassword*123",
            ConfirmPassword = "NewPassword*123"
        };
        var expectedResult = ApiResult<string>.SuccessWith(PasswordResetSuccessfully);
        _identityServiceMock.Setup(s => s.ResetPassword(model)).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.ResetPassword(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result, exactMatch: false);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(PasswordResetSuccessfully, okResult.Value);
    }
}
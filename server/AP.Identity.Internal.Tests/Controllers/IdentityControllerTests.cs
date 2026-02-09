using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AP.Common.Data.Identity.Entities;
using AP.Common.Models;
using AP.Common.Services.Contracts;
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
        var sessionServiceMock = new Mock<ISessionService>();
        var expectedResult = ApiResult<SigninResponse>.SuccessWith(new SigninResponse { Email = "test@example.com" });
        var model = new RefreshTokenRequest("test@example.com", "123456")
        {
            Email = "test@example.com",
            RefreshToken = "123456",
        };
        
        var userSession = new UserSession
        {
            SessionId = 1,
            UserId = 1,
            AccessToken = "access-token",
            RefreshToken = "123456",
            CreatedOn = DateTime.UtcNow,
            CreatedFomIp = "127.0.0.1",
            User = new User
            {
                UserId = 1,
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = "hash",
                Active = true,
                Enabled = true,
                CreatedOn = DateTime.UtcNow
            }
        };
        
        sessionServiceMock.Setup(s => s.GetSessionById(It.IsAny<long>())).ReturnsAsync(userSession);
        _identityServiceMock.Setup(s => s.RefreshToken(It.IsAny<RefreshTokenRequest>(), It.IsAny<string>())).ReturnsAsync(expectedResult);
        
        // Mock session ID cookie
        _controller.ControllerContext.HttpContext.Request.Headers.Cookie = "SessionId=1";

        // Act
        var result = await _controller.RefreshToken(sessionServiceMock.Object);

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
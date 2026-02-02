using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using AP.Common.Models;
using AP.Common.Services.Contracts;
using AP.Identity.Internal.Controllers;
using AP.Identity.Internal.Models;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.NoticeMessages;

namespace AP.Identity.Internal.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<ISystemService> _systemServiceMock;
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _systemServiceMock = new Mock<ISystemService>();
        _usersServiceMock = new Mock<IUsersService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _controller = new UsersController(_systemServiceMock.Object, _usersServiceMock.Object, _currentUserMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task GetAllRoles_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new List<RoleOutput> { new() { RoleId = 1, RoleName = "Admin" } };
        _systemServiceMock.Setup(s => s.GetAllRoles()).ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetAllRoles();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task GetUsers_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new UsersResponse { Count = 1, Users = [new() { UserId = 1, FirstName = "Test" }] };
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.GetUsers(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(ApiResult<UsersResponse>.SuccessWith(expectedResult));

        // Act
        var result = await _controller.GetUsers(null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task GetUser_ReturnsExpectedResult()
    {
        // Arrange
        var userId = 1;
        var expectedResult = new UserOutput { UserId = userId };
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.GetUser(userId)).ReturnsAsync(ApiResult<UserOutput>.SuccessWith(expectedResult));

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task CreateUser_ReturnsExpectedResult()
    {
        // Arrange
        var model = new CreateUserRequest("Test", "User", "test@example.com", null, 1, 1)
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            RoleId = 1,
            TenantId = 1
        };
        var expectedResult = new UserOutput { UserId = 1 };
        _currentUserMock.Setup(c => c.UserId).Returns(1);
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.CreateUser(model, 1, It.IsAny<string>())).ReturnsAsync(ApiResult<UserOutput>.SuccessWith(expectedResult));

        // Act
        var result = await _controller.CreateUser(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task EditUser_ReturnsExpectedResult()
    {
        // Arrange
        var model = new EditUserRequest(1, "Test", "User", "test@example.com", null, 1)
        {
            UserId = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            RoleId = 1
        };
        var expectedResult = new UserOutput { UserId = 1 };
        _currentUserMock.Setup(c => c.UserId).Returns(1);
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.EditUser(model, 1, It.IsAny<string>())).ReturnsAsync(ApiResult<UserOutput>.SuccessWith(expectedResult));

        // Act
        var result = await _controller.EditUser(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task ActivateDeactivateUser_ReturnsExpectedResult()
    {
        // Arrange
        var userId = 1;
        var active = true;
        var expectedResult = new UserOutput { UserId = userId };
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.ActivateOrDeactivateUser(userId, active)).ReturnsAsync(ApiResult<UserOutput>.SuccessWith(expectedResult));

        // Act
        var result = await _controller.ActivateDeactivateUser(userId, active);

        // Assert
        var actionResult = Assert.IsType<ActionResult<UserOutput>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task RevokeToken_ReturnsExpectedResult()
    {
        // Arrange
        var model = new RevokeTokenRequest("test@example.com", "token")
        {
            Email = "test@example.com",
            RefreshToken = "token"
        };
        _currentUserMock.Setup(c => c.UserId).Returns(1);
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.RevokeToken(model, It.IsAny<string>(), 1)).ReturnsAsync(ApiResult<string>.SuccessWith(RefreshTokenRevoked));

        // Act
        var result = await _controller.RevokeToken(model);

        // Assert
        var actionResult = Assert.IsType<ActionResult<string>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(RefreshTokenRevoked, okResult.Value);
    }

    [Fact]
    public async Task DeleteUser_ReturnsExpectedResult()
    {
        // Arrange
        var userId = 1;
        _systemServiceMock.Setup(s => s.IsCurrentUserSystemAdmin(_currentUserMock.Object)).ReturnsAsync(ApiResult.Success);
        _usersServiceMock.Setup(s => s.DeleteUser(userId)).ReturnsAsync(ApiResult<bool>.SuccessWith(true));

        // Act
        var result = await _controller.DeleteUser(userId);

        // Assert
        _ = Assert.IsType<ActionResult<bool>>(result, exactMatch: false);
    }
}
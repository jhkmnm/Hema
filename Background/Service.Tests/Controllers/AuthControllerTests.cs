using Microsoft.AspNetCore.Mvc;
using Moq;
using Service.Controllers;
using Service.Services;
using Service.Models;
using Xunit;
using MongoDB.Driver;

namespace Service.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<User>> _mockCollection;
    private readonly UserService _userService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // 设置MongoDB模拟
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<User>>();

        _mockDb.Setup(db => db.GetCollection<User>("Users", null))
            .Returns(_mockCollection.Object);

        _userService = new UserService(_mockDb.Object);
        _controller = new AuthController(_userService);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequest { Username = "admin", Password = "sk2025" };
        
        // 创建一个包含预期用户的列表
        var users = new List<User>
        {
            new User { Username = "admin", Password = "sk2025", Id = "1" }
        };
        
        var mockCursor = new Mock<IAsyncCursor<User>>();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)   // 第一次调用返回true，表示有数据
            .ReturnsAsync(false); // 第二次调用返回false，表示结束
        
        // 模拟 Current 属性返回预期的用户数据
        mockCursor.SetupGet(c => c.Current).Returns(users.AsEnumerable());

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<FindOptions<User>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        var response = okResult.Value as dynamic;
        Assert.Equal("登录成功", (string)response.message);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "admin", Password = "wrongpassword" };
        
        var mockCursor = new Mock<IAsyncCursor<User>>();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // 直接返回false，表示没有找到匹配的用户

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<FindOptions<User>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.NotNull(unauthorizedResult);
        var response = unauthorizedResult.Value as dynamic;
        Assert.Equal("用户名或密码错误", (string)response.message);
    }
} 

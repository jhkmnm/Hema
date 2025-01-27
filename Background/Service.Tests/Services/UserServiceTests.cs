using MongoDB.Driver;
using Service.Models;
using Service.Services;
using Moq;
using Xunit;

namespace Service.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<User>> _mockCollection;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<User>>();

        _mockDb.Setup(db => db.GetCollection<User>("Users", null))
            .Returns(_mockCollection.Object);

        _userService = new UserService(_mockDb.Object);
    }

    [Fact]
    public async Task InitializeSeedUser_WhenUserDoesNotExist_ShouldCreateAdminUser()
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<User>>();
        mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);  // 表示没有找到用户

        _mockCollection.Setup(c => c.FindAsync(
            It.IsAny<FilterDefinition<User>>(),
            It.IsAny<FindOptions<User>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        await _userService.InitializeSeedUserAsync();

        // Assert
        _mockCollection.Verify(c => c.InsertOneAsync(
            It.Is<User>(u => u.Username == "admin"),
            It.IsAny<InsertOneOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("admin", "sk2025", true)]
    [InlineData("admin", "wrongpassword", false)]
    [InlineData("wronguser", "sk2025", false)]
    public async Task ValidateUser_ShouldReturnExpectedResult(string username, string password, bool expected)
    {
        // Arrange
        var mockCursor = new Mock<IAsyncCursor<User>>();
        
        // 设置游标的行为
        if (expected)
        {
            var users = new List<User>
            {
                new User { Username = "admin", Password = "sk2025", Id = "1" }
            };
            
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)   // 第一次调用返回true，表示有数据
                .ReturnsAsync(false); // 第二次调用返回false，表示结束
            
            // 模拟 Current 属性返回预期的用户数据
            mockCursor.SetupGet(c => c.Current).Returns(users.AsEnumerable());
        }
        else
        {
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 直接返回false，表示没有数据
        }

        // 设置查询条件的验证
        _mockCollection.Setup(c => c.FindAsync(
            It.Is<FilterDefinition<User>>(filter => true), // 这里可以添加更具体的filter验证
            It.IsAny<FindOptions<User>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCursor.Object);

        // Act
        var result = await _userService.ValidateUserAsync(username, password);

        // Assert
        Assert.Equal(expected, result);
    }
} 

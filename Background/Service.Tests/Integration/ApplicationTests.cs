using MongoDB.Driver;
using Service.Models;
using Service.Services;
using Xunit;

namespace Service.Tests.Integration;

public class ApplicationTests : IAsyncLifetime
{
    private readonly IMongoDatabase _database;
    private readonly ApplicationService _applicationService;
    private readonly IMongoCollection<Application> _applications;

    public ApplicationTests()
    {
        // 使用测试数据库
        var client = new MongoClient("mongodb://localhost:3000");
        _database = client.GetDatabase("UserAuth_Test");
        _applicationService = new ApplicationService(_database);
        _applications = _database.GetCollection<Application>("Applications");
    }

    public async Task InitializeAsync()
    {
        // 测试开始前清理数据
        await _database.DropCollectionAsync("Applications");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateApplication_ShouldSucceed()
    {
        // Arrange
        var application = new Application
        {
            Name = "测试应用",
            Version = "1.0.0",
            Description = "这是一个测试应用",
            OfficialUrl = "https://test.com",
            IconBase64 = "data:image/png;base64,..."
        };

        // Act
        var result = await _applicationService.CreateAsync(application);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(application.Name, result.Name);
        Assert.Equal(application.Version, result.Version);
        Assert.Equal(application.Description, result.Description);
        Assert.Equal(application.OfficialUrl, result.OfficialUrl);
        Assert.Equal(application.IconBase64, result.IconBase64);
        Assert.NotEqual(default, result.CreateTime);
        Assert.NotEqual(default, result.UpdateTime);
    }

    [Fact]
    public async Task GetAllApplications_ShouldReturnAllApplications()
    {
        // Arrange
        var applications = new[]
        {
            new Application { Name = "应用1", Version = "1.0", Description = "描述1" },
            new Application { Name = "应用2", Version = "2.0", Description = "描述2" }
        };
        await _applications.InsertManyAsync(applications);

        // Act
        var result = await _applicationService.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetApplicationById_ShouldReturnCorrectApplication()
    {
        // Arrange
        var application = new Application
        {
            Name = "测试应用",
            Version = "1.0",
            Description = "测试描述"
        };
        await _applications.InsertOneAsync(application);

        // Act
        var result = await _applicationService.GetByIdAsync(application.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(application.Id, result.Id);
        Assert.Equal(application.Name, result.Name);
    }

    [Fact]
    public async Task SearchApplications_ShouldReturnMatchingApplications()
    {
        // Arrange
        var applications = new[]
        {
            new Application { Name = "测试应用1", Version = "1.0", Description = "这是测试描述" },
            new Application { Name = "应用2", Version = "2.0", Description = "这是另一个测试" },
            new Application { Name = "其他应用", Version = "3.0", Description = "普通描述" }
        };
        await _applications.InsertManyAsync(applications);

        // Act
        var result = await _applicationService.SearchAsync("测试");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, a => a.Name == "测试应用1");
        Assert.Contains(result, a => a.Description.Contains("另一个测试"));
    }

    [Fact]
    public async Task UpdateApplication_ShouldSucceed()
    {
        // Arrange
        var application = new Application
        {
            Name = "原始应用",
            Version = "1.0",
            Description = "原始描述"
        };
        await _applications.InsertOneAsync(application);

        var updateData = new Application
        {
            Name = "更新后的应用",
            Version = "2.0",
            Description = "更新后的描述",
            OfficialUrl = "https://updated.com",
            IconBase64 = "updated-base64"
        };

        // Act
        var success = await _applicationService.UpdateAsync(application.Id, updateData);
        var updated = await _applicationService.GetByIdAsync(application.Id);

        // Assert
        Assert.True(success);
        Assert.NotNull(updated);
        Assert.Equal(updateData.Name, updated.Name);
        Assert.Equal(updateData.Version, updated.Version);
        Assert.Equal(updateData.Description, updated.Description);
        Assert.Equal(updateData.OfficialUrl, updated.OfficialUrl);
        Assert.Equal(updateData.IconBase64, updated.IconBase64);
        Assert.NotEqual(updated.CreateTime, updated.UpdateTime);
    }

    [Fact]
    public async Task DeleteApplication_ShouldSucceed()
    {
        // Arrange
        var application = new Application
        {
            Name = "待删除应用",
            Version = "1.0",
            Description = "待删除描述"
        };
        await _applications.InsertOneAsync(application);

        // Act
        var success = await _applicationService.DeleteAsync(application.Id);
        var deleted = await _applicationService.GetByIdAsync(application.Id);

        // Assert
        Assert.True(success);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task UpdateApplication_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = "507f1f77bcf86cd799439011";
        var updateData = new Application
        {
            Name = "更新测试",
            Version = "1.0",
            Description = "更新测试描述"
        };

        // Act
        var result = await _applicationService.UpdateAsync(nonExistentId, updateData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteApplication_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = "507f1f77bcf86cd799439011";

        // Act
        var result = await _applicationService.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }
} 

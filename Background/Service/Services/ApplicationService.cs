using MongoDB.Driver;
using Service.Models;

namespace Service.Services;

/// <summary>
/// 应用程序服务
/// </summary>
public class ApplicationService
{
    private readonly IMongoCollection<Application> _applications;

    public ApplicationService(IMongoDatabase database)
    {
        _applications = database.GetCollection<Application>("Applications");
    }

    /// <summary>
    /// 获取所有应用程序
    /// </summary>
    public async Task<List<Application>> GetAllAsync()
    {
        return await _applications.Find(_ => true).ToListAsync();
    }

    /// <summary>
    /// 根据ID获取应用程序
    /// </summary>
    public async Task<Application?> GetByIdAsync(string id)
    {
        return await _applications.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// 搜索应用程序
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    public async Task<List<Application>> SearchAsync(string keyword)
    {
        var filter = Builders<Application>.Filter.Or(
            Builders<Application>.Filter.Regex(x => x.Name, new MongoDB.Bson.BsonRegularExpression(keyword, "i")),
            Builders<Application>.Filter.Regex(x => x.Description, new MongoDB.Bson.BsonRegularExpression(keyword, "i"))
        );
        return await _applications.Find(filter).ToListAsync();
    }

    /// <summary>
    /// 创建新应用程序
    /// </summary>
    public async Task<Application> CreateAsync(Application application)
    {
        application.CreateTime = DateTime.UtcNow;
        application.UpdateTime = DateTime.UtcNow;
        await _applications.InsertOneAsync(application);
        return application;
    }

    /// <summary>
    /// 更新应用程序信息
    /// </summary>
    public async Task<bool> UpdateAsync(string id, Application application)
    {
        application.UpdateTime = DateTime.UtcNow;
        var filter = Builders<Application>.Filter.Eq(x => x.Id, id);
        var update = Builders<Application>.Update
            .Set(x => x.Name, application.Name)
            .Set(x => x.Version, application.Version)
            .Set(x => x.Description, application.Description)
            .Set(x => x.OfficialUrl, application.OfficialUrl)
            .Set(x => x.IconBase64, application.IconBase64)
            .Set(x => x.UpdateTime, application.UpdateTime);

        var result = await _applications.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    /// <summary>
    /// 删除应用程序
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _applications.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }
} 

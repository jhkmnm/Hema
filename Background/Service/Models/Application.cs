using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Service.Models;

/// <summary>
/// 应用程序信息模型
/// </summary>
public class Application
{
    /// <summary>
    /// MongoDB文档ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// 应用程序名称
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 版本号
    /// </summary>
    [BsonElement("version")]
    public string Version { get; set; } = null!;

    /// <summary>
    /// 应用程序描述
    /// </summary>
    [BsonElement("description")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 官方网站地址
    /// </summary>
    [BsonElement("officialUrl")]
    public string? OfficialUrl { get; set; }

    /// <summary>
    /// 应用程序图标（Base64编码）
    /// </summary>
    [BsonElement("iconBase64")]
    public string? IconBase64 { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [BsonElement("createTime")]
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [BsonElement("updateTime")]
    public DateTime UpdateTime { get; set; }
} 

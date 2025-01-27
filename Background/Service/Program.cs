using MongoDB.Driver;
using Service.Services;

// 添加这个特性，允许测试项目访问内部类型
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Service.Tests")]

var builder = WebApplication.CreateBuilder(args);

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient("mongodb://localhost:3000");
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("UserAuth");
});

// 添加用户服务
builder.Services.AddScoped<UserService>();

// 添加应用程序服务
builder.Services.AddScoped<ApplicationService>();

// 添加CORS服务
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:8080")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 启用CORS
app.UseCors();

// 初始化种子数据
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    await userService.InitializeSeedUserAsync();
}

app.Run();

// 添加这个公共的Program类
public partial class Program { }

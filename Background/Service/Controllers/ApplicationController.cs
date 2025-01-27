using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Services;

namespace Service.Controllers;

/// <summary>
/// 应用程序管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly ApplicationService _applicationService;

    public ApplicationController(ApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    /// <summary>
    /// 获取所有应用程序
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Application>>> GetAll()
    {
        var applications = await _applicationService.GetAllAsync();
        return Ok(applications);
    }

    /// <summary>
    /// 根据ID获取应用程序
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Application>> GetById(string id)
    {
        var application = await _applicationService.GetByIdAsync(id);
        if (application == null)
        {
            return NotFound(new { message = "未找到指定的应用程序" });
        }
        return Ok(application);
    }

    /// <summary>
    /// 搜索应用程序
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<Application>>> Search([FromQuery] string keyword)
    {
        var applications = await _applicationService.SearchAsync(keyword);
        return Ok(applications);
    }

    /// <summary>
    /// 创建新应用程序
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Application>> Create([FromBody] Application application)
    {
        var created = await _applicationService.CreateAsync(application);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// 更新应用程序信息
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Application application)
    {
        var success = await _applicationService.UpdateAsync(id, application);
        if (!success)
        {
            return NotFound(new { message = "未找到指定的应用程序" });
        }
        return NoContent();
    }

    /// <summary>
    /// 删除应用程序
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _applicationService.DeleteAsync(id);
        if (!success)
        {
            return NotFound(new { message = "未找到指定的应用程序" });
        }
        return NoContent();
    }

    /// <summary>
    /// 分页获取应用程序列表
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<Application>>> GetPaginated([FromQuery] PaginationRequest request)
    {
        var result = await _applicationService.GetPaginatedAsync(request);
        return Ok(result);
    }
} 

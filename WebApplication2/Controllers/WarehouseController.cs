using Microsoft.AspNetCore.Mvc;
using WebApplication2.DTO;
using WebApplication2.Services.Abstractions;

namespace WebApplication2.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IProductWarehouseService _productWarehouseService;

    public WarehouseController(IProductWarehouseService productWarehouseService)
    {
        _productWarehouseService = productWarehouseService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterProduct([FromBody] ProductWarehouseRequest request)
    {
        try
        {
            int newId = await _productWarehouseService.RegisterProductAsync(request);
            return Ok(newId);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    [HttpPost("with-procedure")]
    public async Task<IActionResult> RegisterUsingProcedure([FromBody] ProductWarehouseRequest request)
    {
        try
        {
            int newId = await _productWarehouseService.CallStoredProcedureAsync(request);
            return Ok(newId);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
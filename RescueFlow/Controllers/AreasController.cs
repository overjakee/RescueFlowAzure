using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Models;

namespace RescueFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
    {

        private readonly RescueFlowDbContext _context;

        public AreasController(RescueFlowDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddArea([FromBody] Area area)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (area.RequiredResources == null || !area.RequiredResources.Any())
            {
                return BadRequest("กรุณากรอกรายการทรัพยากรที่ต้องการอย่างน้อย 1 รายการ");
            }

            var existingArea = await _context.Areas.FirstOrDefaultAsync(a => a.AreaId == area.AreaId);

            if (existingArea != null)
            {
                return Conflict(new { message = "พื้นที่นี้มีอยู่ในระบบแล้ว" });
            }

            try
            {
                _context.Areas.Add(area);
                await _context.SaveChangesAsync();
                return Ok(area);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAreasAsync()
        {
            try
            {
                var areas = await _context.Areas.ToListAsync();
                return Ok(areas); 
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}

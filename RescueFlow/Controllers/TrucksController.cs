using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using RescueFlow.Data;
using RescueFlow.Models;

namespace RescueFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrucksController : ControllerBase
    {
        private readonly RescueFlowDbContext _context;

        public TrucksController(RescueFlowDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddTruck([FromBody] Truck truck)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
                
            var existingTruck = await _context.Trucks.FirstOrDefaultAsync(a => a.TruckId == truck.TruckId);

            if (existingTruck != null)
            {
                return Conflict(new { message = "รถคันนี้มีอยู่ในระบบแล้ว" });
            }

            try
            {
                _context.Trucks.Add(truck);
                await _context.SaveChangesAsync();

                return Ok(truck);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrucksAsync()
        {
            try
            {
                var trucks= await _context.Trucks.ToListAsync();
                return Ok(trucks);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}

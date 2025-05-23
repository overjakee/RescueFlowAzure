using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RescueFlow.Data;
using RescueFlow.Interfaces;
using RescueFlow.Models;
using RescueFlow.Services;
using StackExchange.Redis;

namespace RescueFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly RescueFlowDbContext _context;
        private readonly IRedisCacheService _redis;
        private readonly IAssignmentService _assignmentService;

        private const string CACHE_KEY = "latest_assignments";

        public AssignmentsController(
            RescueFlowDbContext context,
            IRedisCacheService redis,
            IAssignmentService assignmentService)
        {
            _context = context;
            _redis = redis;
            _assignmentService = assignmentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessAssignments()
        {
            try
            {
                // แสดงผลการคำนวณว่าที่ Area ไหนสามารถจัดการได้, จัดการไม่ได้ เพราะอะไร
                var assignments = _assignmentService.ProcessAssignments();

                // ตรวจสอบกรณีไม่มีการมอบหมาย
                if (assignments == null || !assignments.Any())
                {
                    return NotFound(new { message = "ไม่สามารถมอบหมายทรัพยากรได้" });
                }

                // ลบข้อมูลการมอบหมายเก่าและเพิ่มใหม่
                _context.Assignments.RemoveRange(_context.Assignments);
                _context.Assignments.AddRange(assignments);
                await _context.SaveChangesAsync();

                // แคชผลลัพธ์ใน Redis
                var json = JsonSerializer.Serialize(assignments);
                await _redis.SetCacheAsync(CACHE_KEY, json, TimeSpan.FromMinutes(30));

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments()
        {
            try
            {
                var cached = await _redis.GetCacheAsync(CACHE_KEY);
                if (!string.IsNullOrEmpty(cached))
                {
                    var result = JsonSerializer.Deserialize<List<Assignment>>(cached);
                    return Ok(result);
                }


                var assignmentsFromDb = _context.Assignments.ToList();
                return Ok(assignmentsFromDb);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> ClearAssignments()
        {
            try
            {
                await _redis.DeleteCacheAsync(CACHE_KEY);

                //_context.Assignments.RemoveRange(_context.Assignments);
                //await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}

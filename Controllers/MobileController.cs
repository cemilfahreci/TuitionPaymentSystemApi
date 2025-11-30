using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuitionApi.Data;

namespace TuitionApi.Controllers;

[Route("api/v1/mobile")]
[ApiController]
public class MobileController : ControllerBase
{
    private readonly TuitionDbContext _context;

    public MobileController(TuitionDbContext context)
    {
        _context = context;
    }

    [HttpGet("tuition/{studentNo}")]
    public async Task<IActionResult> QueryTuition(string studentNo, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == studentNo);
        if (student == null) return NotFound(new { error = "Student not found" });

        var query = _context.Tuitions.Where(t => t.StudentId == student.Id);
        var totalRecords = await query.CountAsync();

        var tuitions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new 
            {
                term = t.Term,
                tuition_total = t.TotalAmount,
                paid_amount = t.PaidAmount,
                balance = t.TotalAmount - t.PaidAmount,
                status = t.Status
            })
            .ToListAsync();

        return Ok(new
        {
            student_no = student.StudentNo,
            name = student.Name,
            total_records = totalRecords,
            page = page,
            page_size = pageSize,
            tuitions = tuitions
        });
    }
}

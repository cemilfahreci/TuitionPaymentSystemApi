using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TuitionApi.Data;
using TuitionApi.Models;

namespace TuitionApi.Controllers;

[Route("api/v1/admin")]
[ApiController]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly TuitionDbContext _context;

    public AdminController(TuitionDbContext context)
    {
        _context = context;
    }

    [HttpPost("tuition")]
    public async Task<IActionResult> AddTuition([FromBody] AddTuitionRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);
        if (student == null)
        {
            student = new Student { StudentNo = request.StudentNo, Name = request.StudentName ?? "Unknown" };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        var tuition = await _context.Tuitions.FirstOrDefaultAsync(t => t.StudentId == student.Id && t.Term == request.Term);
        if (tuition != null) return BadRequest(new { transaction_status = "Error", message = "Tuition already exists" });

        tuition = new Tuition
        {
            StudentId = student.Id,
            Term = request.Term,
            TotalAmount = request.Amount
        };
        _context.Tuitions.Add(tuition);
        await _context.SaveChangesAsync();

        return Ok(new { transaction_status = "Success" });
    }

    [HttpPost("tuition/batch")]
    public async Task<IActionResult> AddTuitionBatch(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { transaction_status = "Error", message = "No file uploaded" });

        try
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<BatchTuitionRecord>().ToList();
                foreach (var record in records)
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == record.student_no);
                    if (student == null)
                    {
                        student = new Student { StudentNo = record.student_no, Name = record.student_name ?? "Unknown" };
                        _context.Students.Add(student);
                        await _context.SaveChangesAsync();
                    }

                    var tuition = await _context.Tuitions.FirstOrDefaultAsync(t => t.StudentId == student.Id && t.Term == record.term);
                    if (tuition == null)
                    {
                        tuition = new Tuition
                        {
                            StudentId = student.Id,
                            Term = record.term,
                            TotalAmount = record.amount
                        };
                        _context.Tuitions.Add(tuition);
                    }
                }
                await _context.SaveChangesAsync();
            }
            return Ok(new { transaction_status = "Success" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { transaction_status = "Error", message = ex.Message });
        }
    }

    [HttpGet("tuition/unpaid")]
    public async Task<IActionResult> UnpaidTuitionStatus([FromQuery] string term, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Tuitions
            .Include(t => t.Student)
            .Where(t => t.Term == term && t.Status != "Paid");

        var totalRecords = await query.CountAsync();

        var tuitions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                student_no = t.Student!.StudentNo,
                name = t.Student.Name,
                tuition_total = t.TotalAmount,
                paid_amount = t.PaidAmount,
                balance = t.TotalAmount - t.PaidAmount,
                status = t.Status
            })
            .ToListAsync();

        return Ok(new 
        {
            total_records = totalRecords,
            page = page,
            page_size = pageSize,
            students = tuitions
        });
    }


    [HttpDelete("tuition/{studentNo}")]
    public async Task<IActionResult> DeleteTuition([FromRoute] string studentNo, [FromQuery] string term)
    {
        if (string.IsNullOrEmpty(term)) return BadRequest(new { transaction_status = "Error", message = "Term is required" });

        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == studentNo);
        if (student == null) return NotFound(new { transaction_status = "Error", message = "Student not found" });

        var tuition = await _context.Tuitions.FirstOrDefaultAsync(t => t.StudentId == student.Id && t.Term == term);
        if (tuition == null) return NotFound(new { transaction_status = "Error", message = "Tuition record for this term not found" });

        _context.Tuitions.Remove(tuition);
        await _context.SaveChangesAsync();

        return Ok(new { transaction_status = "Success", message = "Tuition deleted" });
    }

    [HttpPut("tuition/{studentNo}")]
    public async Task<IActionResult> UpdateTuition(string studentNo, [FromBody] UpdateTuitionRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == studentNo);
        if (student == null) return NotFound(new { transaction_status = "Error", message = "Student not found" });

        if (string.IsNullOrEmpty(request.Term)) return BadRequest(new { transaction_status = "Error", message = "Term is required to identify the tuition" });

        var tuition = await _context.Tuitions.FirstOrDefaultAsync(t => t.StudentId == student.Id && t.Term == request.Term);
        if (tuition == null) return NotFound(new { transaction_status = "Error", message = "Tuition record for this term not found" });

        if (request.Amount.HasValue) tuition.TotalAmount = request.Amount.Value;
        
        // Recalculate status
        if (tuition.PaidAmount >= tuition.TotalAmount) tuition.Status = "Paid";
        else if (tuition.PaidAmount > 0) tuition.Status = "Partial";
        else tuition.Status = "Unpaid";

        await _context.SaveChangesAsync();

        return Ok(new { transaction_status = "Success", message = "Tuition updated" });
    }
}

public class UpdateTuitionRequest
{
    public string Term { get; set; } = string.Empty;
    public double? Amount { get; set; }
}

public class AddTuitionRequest
{
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string? StudentName { get; set; }
}

public class BatchTuitionRecord
{
    public string student_no { get; set; } = string.Empty;
    public string term { get; set; } = string.Empty;
    public double amount { get; set; }
    public string? student_name { get; set; }
}

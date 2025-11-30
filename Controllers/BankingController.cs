using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuitionApi.Data;
using TuitionApi.Models;

namespace TuitionApi.Controllers;

[Route("api/v1/banking")]
[ApiController]
public class BankingController : ControllerBase
{
    private readonly TuitionDbContext _context;

    public BankingController(TuitionDbContext context)
    {
        _context = context;
    }

    [HttpGet("tuition/{studentNo}")]
    [Authorize]
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

    [HttpPost("payment")]
    public async Task<IActionResult> PayTuition([FromBody] PaymentRequest request)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentNo == request.StudentNo);
        if (student == null) return NotFound(new { payment_status = "Error", message = "Student not found" });

        var tuition = await _context.Tuitions.FirstOrDefaultAsync(t => t.StudentId == student.Id && t.Term == request.Term);
        if (tuition == null) return NotFound(new { payment_status = "Error", message = "Tuition record not found" });

        try
        {
            var payment = new Payment
            {
                TuitionId = tuition.Id,
                Amount = request.Amount,
                Status = "Successful"
            };
            _context.Payments.Add(payment);

            tuition.PaidAmount += request.Amount;
            if (tuition.PaidAmount >= tuition.TotalAmount)
                tuition.Status = "Paid";
            else
                tuition.Status = "Partial";

            await _context.SaveChangesAsync();

            return Ok(new { payment_status = "Successful", new_balance = tuition.TotalAmount - tuition.PaidAmount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { payment_status = "Error", message = ex.Message });
        }
    }
}

public class PaymentRequest
{
    public string StudentNo { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public double Amount { get; set; }
}

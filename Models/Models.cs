using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuitionApi.Models;

public class Student
{
    public int Id { get; set; }
    [Required]
    public string StudentNo { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;
    public List<Tuition> Tuitions { get; set; } = new();
}

public class Tuition
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    [ForeignKey("StudentId")]
    public Student? Student { get; set; }
    [Required]
    public string Term { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = "Unpaid"; // Unpaid, Partial, Paid
    public List<Payment> Payments { get; set; } = new();
}

public class Payment
{
    public int Id { get; set; }
    public int TuitionId { get; set; }
    [ForeignKey("TuitionId")]
    public Tuition? Tuition { get; set; }
    public double Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Successful";
}

public class User
{
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}

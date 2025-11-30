using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityPaymentApi.Dtos;
using UniversityPaymentApi.Models;


namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StudentController : ControllerBase
{
    private readonly UniversityContext _context;

    public StudentController(UniversityContext context)
    {
        _context = context;
    }

    [HttpGet("mobile/tuition/{studentNumber}")]
    [AllowAnonymous]
    public async Task<ActionResult<TuitionSummaryDto>> GetTuitionForMobile(string studentNumber)
    {
        var student = await _context.Students
            .Include(s => s.TuitionRecords)
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        if (student == null)
            return NotFound(new { message = "Student not found" });

        var dto = MapStudentToTuitionSummary(student);
        return Ok(dto);
    }

    [HttpGet("bank/tuition/{studentNumber}")]
    [Authorize]
    public async Task<ActionResult<TuitionSummaryDto>> GetTuitionForBank(string studentNumber)
    {
        var student = await _context.Students
            .Include(s => s.TuitionRecords)
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

        if (student == null)
            return NotFound(new { message = "Student not found" });

        var dto = MapStudentToTuitionSummary(student);
        return Ok(dto);
    }

    private static TuitionSummaryDto MapStudentToTuitionSummary(Student student)
    {
        var recordDtos = student.TuitionRecords
            .Select(r => new TuitionRecordDto
            {
                Term = r.Term,
                TotalAmount = r.TotalAmount,
                PaidAmount = r.PaidAmount,
                Balance = r.Balance,
                Status = r.Status
            })
            .ToList();

        var totalAmount = recordDtos.Sum(r => r.TotalAmount);
        var paidAmount = recordDtos.Sum(r => r.PaidAmount);
        var balance = recordDtos.Sum(r => r.Balance);

        return new TuitionSummaryDto
        {
            StudentNumber = student.StudentNumber,
            FullName = $"{student.FirstName} {student.LastName}".Trim(),
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            Balance = balance,
            Records = recordDtos
        };
    }
}

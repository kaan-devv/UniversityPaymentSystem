using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityPaymentApi.Dtos;
using UniversityPaymentApi.Models;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly UniversityContext _context;

    public PaymentController(UniversityContext context)
    {
        _context = context;
    }

    [HttpPost("pay")]
    [AllowAnonymous]
    public async Task<ActionResult<PayTuitionResponseDto>> PayTuition(
        [FromBody] PayTuitionRequestDto request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        var tuitionRecord = await _context.TuitionRecords
            .Include(r => r.Student)
            .Include(r => r.Transactions)
            .FirstOrDefaultAsync(r =>
                r.Student.StudentNumber == request.StudentNumber &&
                r.Term == request.Term);

        if (tuitionRecord == null)
        {
            return NotFound(new { message = "Tuition record not found for this student and term." });
        }

        if (tuitionRecord.Balance <= 0)
        {
            return BadRequest(new { message = "This tuition is already fully paid." });
        }

        if (request.Amount > tuitionRecord.Balance)
        {
            return BadRequest(new { message = "Amount cannot be greater than remaining balance." });
        }

        var transaction = new PaymentTransaction
        {
            RecordId = tuitionRecord.RecordId,
            TuitionRecord = tuitionRecord,
            PaymentAmount = request.Amount,
            PaymentDate = DateTime.UtcNow,
            Status = "SUCCESS"
        };

        tuitionRecord.PaidAmount += request.Amount;

        if (tuitionRecord.Balance == 0)
        {
            tuitionRecord.Status = "PAID";
        }
        else
        {
            tuitionRecord.Status = "PARTIAL";
        }

        tuitionRecord.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        var response = new PayTuitionResponseDto
        {
            Success = true,
            Message = "Payment processed successfully.",
            StudentNumber = request.StudentNumber,
            Term = request.Term,
            PaidAmount = request.Amount,
            TotalPaid = tuitionRecord.PaidAmount,
            TotalAmount = tuitionRecord.TotalAmount,
            Balance = tuitionRecord.Balance,
            TuitionStatus = tuitionRecord.Status,
            TransactionReference = transaction.ReferenceCode
        };

        return Ok(response);
    }
}

using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityPaymentApi.Dtos;
using UniversityPaymentApi.Models;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] 
public class AdminController : ControllerBase
{
    private readonly UniversityContext _context;

    public AdminController(UniversityContext context)
    {
        _context = context;
    }

    [HttpPost("tuition")]
    public async Task<ActionResult<AddTuitionResponseDto>> AddTuition(
        [FromBody] AddTuitionRequestDto request)
    {
        if (request.TotalAmount <= 0)
        {
            return BadRequest(new { message = "TotalAmount must be greater than zero." });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == request.StudentNumber);

        if (student == null)
        {
            return NotFound(new { message = "Student not found." });
        }

        var existingRecord = await _context.TuitionRecords
            .FirstOrDefaultAsync(r =>
                r.StudentId == student.StudentId &&
                r.Term == request.Term);

        if (existingRecord != null)
        {
            return BadRequest(new { message = "Tuition record already exists for this student and term." });
        }

        var record = new TuitionRecord
        {
            StudentId = student.StudentId,
            Student = student,
            Term = request.Term,
            TotalAmount = request.TotalAmount,
            PaidAmount = 0,
            Status = "UNPAID"
        };

        _context.TuitionRecords.Add(record);
        await _context.SaveChangesAsync();

        var response = new AddTuitionResponseDto
        {
            Success = true,
            Message = "Tuition record created.",
            StudentNumber = request.StudentNumber,
            Term = request.Term,
            TotalAmount = request.TotalAmount
        };

        return Ok(response);
    }


    [HttpPost("tuition/batch")]
    public async Task<ActionResult<AddTuitionBatchResponseDto>> AddTuitionBatch(
        IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "CSV file is required." });
        }

        var response = new AddTuitionBatchResponseDto();

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        string? line;
        int lineNumber = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            response.TotalLines++;

            var resultItem = new AddTuitionBatchResultItemDto
            {
                LineNumber = lineNumber,
                RawLine = line
            };

            try
            {
                var parts = line.Split(',');

                if (parts.Length < 3)
                {
                    resultItem.Success = false;
                    resultItem.Message = "Invalid format. Expected: StudentNumber,Term,TotalAmount";
                }
                else
                {
                    var studentNumber = parts[0].Trim();
                    var term = parts[1].Trim();

                    if (!decimal.TryParse(
                            parts[2].Trim(),
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out var totalAmount))
                    {
                        resultItem.Success = false;
                        resultItem.Message = "Invalid TotalAmount.";
                    }
                    else
                    {
                        var student = await _context.Students
                            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

                        if (student == null)
                        {
                            resultItem.Success = false;
                            resultItem.Message = "Student not found.";
                        }
                        else
                        {
                            var existingRecord = await _context.TuitionRecords
                                .FirstOrDefaultAsync(r =>
                                    r.StudentId == student.StudentId &&
                                    r.Term == term);

                            if (existingRecord != null)
                            {
                                resultItem.Success = false;
                                resultItem.Message = "Tuition already exists for this student and term.";
                            }
                            else
                            {
                                var record = new TuitionRecord
                                {
                                    StudentId = student.StudentId,
                                    Student = student,
                                    Term = term,
                                    TotalAmount = totalAmount,
                                    PaidAmount = 0,
                                    Status = "UNPAID"
                                };

                                _context.TuitionRecords.Add(record);

                                resultItem.Success = true;
                                resultItem.Message = "Created.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultItem.Success = false;
                resultItem.Message = "Error: " + ex.Message;
            }

            if (resultItem.Success)
                response.SuccessCount++;
            else
                response.FailedCount++;

            response.Results.Add(resultItem);
        }

        await _context.SaveChangesAsync();

        return Ok(response);
    }

    [HttpGet("unpaid")]
    public async Task<ActionResult<UnpaidTuitionPageDto>> GetUnpaidTuition(
        [FromQuery] string term,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { message = "Term is required." });
        }

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _context.TuitionRecords
            .Include(r => r.Student)
            .Where(r => r.Term == term && r.Balance > 0)
            .OrderBy(r => r.Student.StudentNumber);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var records = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dto = new UnpaidTuitionPageDto
        {
            Term = term,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = records.Select(r => new UnpaidTuitionItemDto
            {
                StudentNumber = r.Student.StudentNumber,
                FullName = $"{r.Student.FirstName} {r.Student.LastName}".Trim(),
                Term = r.Term,
                TotalAmount = r.TotalAmount,
                PaidAmount = r.PaidAmount,
                Balance = r.Balance
            }).ToList()
        };

        return Ok(dto);
    }
}

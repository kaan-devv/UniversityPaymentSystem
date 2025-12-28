using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using UniversityPaymentApi.Dtos;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StudentController : ControllerBase
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "Students"; // Collection name in Firestore

    public StudentController()
    {
        // The environment variable for credentials is set in Program.cs.
        // We initialize Firestore with the specific Project ID here.
        string projectId = "aichatproject-f22d6"; 
        _firestoreDb = FirestoreDb.Create(projectId);
    }

    [HttpGet("mobile/tuition/{studentNumber}")]
    [AllowAnonymous]
    public async Task<ActionResult<TuitionSummaryDto>> GetTuitionForMobile(string studentNumber)
    {
        return await GetStudentData(studentNumber);
    }

    [HttpGet("bank/tuition/{studentNumber}")]
    [Authorize] // Authorization check for Bank (currently placeholder)
    public async Task<ActionResult<TuitionSummaryDto>> GetTuitionForBank(string studentNumber)
    {
        return await GetStudentData(studentNumber);
    }

    // Shared method for retrieving student data from Firestore
    private async Task<ActionResult<TuitionSummaryDto>> GetStudentData(string studentNumber)
    {
        try 
        {
            // 1. Fetch student document from Firestore (assuming Document ID = StudentNumber)
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(studentNumber);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return NotFound(new { message = "Student not found in Firestore" });

            // 2. Convert Firestore data to Dictionary
            Dictionary<string, object> data = snapshot.ToDictionary();

            // 3. Manual Mapping (Mapping Dictionary to DTO)
            var dto = new TuitionSummaryDto
            {
                StudentNumber = studentNumber,
                // Safely retrieve string values
                FullName = data.ContainsKey("firstName") && data.ContainsKey("lastName") 
                           ? $"{data["firstName"]} {data["lastName"]}" 
                           : "Unknown Student",
                
                Records = new List<TuitionRecordDto>() 
            };

            // Handle nested 'tuitionRecords' array
            if (data.ContainsKey("tuitionRecords") && data["tuitionRecords"] is List<object> recordsList)
            {
                foreach (var item in recordsList)
                {
                    if (item is Dictionary<string, object> recordData)
                    {
                        var tRecord = new TuitionRecordDto
                        {
                            Term = recordData.ContainsKey("term") ? recordData["term"].ToString() : "",
                            Status = recordData.ContainsKey("status") ? recordData["status"].ToString() : "",
                            // Firestore numbers might come as Int64, safe conversion to Decimal:
                            TotalAmount = recordData.ContainsKey("totalAmount") ? Convert.ToDecimal(recordData["totalAmount"]) : 0,
                            PaidAmount = recordData.ContainsKey("paidAmount") ? Convert.ToDecimal(recordData["paidAmount"]) : 0,
                        };
                        
                        // Calculate balance
                        tRecord.Balance = tRecord.TotalAmount - tRecord.PaidAmount;
                        
                        dto.Records.Add(tRecord);
                    }
                }
            }

            // Calculate summary totals
            dto.TotalAmount = dto.Records.Sum(r => r.TotalAmount);
            dto.PaidAmount = dto.Records.Sum(r => r.PaidAmount);
            dto.Balance = dto.Records.Sum(r => r.Balance);

            return Ok(dto);
        }
        catch (Exception ex)
        {
            // Return 500 Internal Server Error with exception message
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
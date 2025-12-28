using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using UniversityPaymentApi.Dtos;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdminController : ControllerBase
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "Students"; // Firestore Collection Name

    public AdminController()
    {
        // Initialize Firestore with your specific Project ID
        string projectId = "aichatproject-f22d6";
        _firestoreDb = FirestoreDb.Create(projectId);
    }

    // Endpoint to create a new student or add tuition to an existing student
    // Route: POST api/v1/Admin/tuition
    [HttpPost("tuition")]
    public async Task<IActionResult> AddTuition([FromBody] TuitionCreateDto request)
    {
        // 1. Validate the request
        if (request == null || string.IsNullOrEmpty(request.StudentNumber))
            return BadRequest("Invalid data. Student Number is required.");

        try
        {
            // Reference to the student document in Firestore
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(request.StudentNumber);
            
            // 2. Check if the student already exists
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            
            Dictionary<string, object> studentData;

            if (snapshot.Exists)
            {
                // If student exists, retrieve current data to update it
                studentData = snapshot.ToDictionary();
            }
            else
            {
                // If student does not exist, create a new student structure
                studentData = new Dictionary<string, object>
                {
                    { "studentNumber", request.StudentNumber },
                    { "firstName", request.FirstName },
                    { "lastName", request.LastName },
                    { "tuitionRecords", new List<object>() } // Initialize empty list
                };
            }

            // 3. Create the new tuition record object
            var newRecord = new Dictionary<string, object>
            {
            { "term", request.Term },
            { "totalAmount", Convert.ToDouble(request.Amount) }, 
            { "paidAmount", 0 },
            { "status", "Unpaid" }
            };


            // 4. Add the new record to the existing list
            List<object> records = new List<object>();
            
            // Check if 'tuitionRecords' field exists and uses the correct list type
            if (studentData.ContainsKey("tuitionRecords") && studentData["tuitionRecords"] is List<object> existingList)
            {
                records = existingList;
            }
            
            records.Add(newRecord);
            studentData["tuitionRecords"] = records;

            // 5. Save data to Firestore (SetAsync overwrites or creates the document)
            await docRef.SetAsync(studentData);

            return Ok(new { message = $"Student {request.StudentNumber} updated/created successfully." });
        }
        catch (Exception ex)
        {
            // Return 500 error with the exception message for debugging
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// DTO Class for the request body
public class TuitionCreateDto
{
    public string StudentNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Term { get; set; }
    public decimal Amount { get; set; }
}
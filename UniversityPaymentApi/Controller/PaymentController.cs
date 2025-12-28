using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using UniversityPaymentApi.Dtos;

namespace UniversityPaymentApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly FirestoreDb _firestoreDb;
    private const string CollectionName = "Students";

    public PaymentController()
    {
        // SQL Context yerine Firestore projesini bağlıyoruz
        string projectId = "aichatproject-f22d6";
        _firestoreDb = FirestoreDb.Create(projectId);
    }

    [HttpPost("pay")]
    [AllowAnonymous]
    public async Task<ActionResult<PayTuitionResponseDto>> PayTuition([FromBody] PayTuitionRequestDto request)
    {
        // 1. Validasyonlar
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        try
        {
            // 2. Firestore'dan Öğrenciyi Çek
            DocumentReference docRef = _firestoreDb.Collection(CollectionName).Document(request.StudentNumber);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                return NotFound(new { message = "Student not found." });
            }

            // Veriyi al
            Dictionary<string, object> studentData = snapshot.ToDictionary();
            bool recordFound = false;
            
            // Yanıt için değişkenler
            decimal totalAmount = 0;
            decimal newTotalPaid = 0;
            decimal newBalance = 0;
            string newStatus = "Unknown";

            // 3. Tuition Kayıtlarını Tara
            if (studentData.ContainsKey("tuitionRecords") && studentData["tuitionRecords"] is List<object> records)
            {
                var updatedRecords = new List<object>();

                foreach (var item in records)
                {
                    if (item is Dictionary<string, object> recordData)
                    {
                        string term = recordData.ContainsKey("term") ? recordData["term"].ToString() : "";

                        // İstenen dönemi bulduk mu?
                        if (term == request.Term)
                        {
                            // Verileri güvenli şekilde sayıya çevir
                            totalAmount = recordData.ContainsKey("totalAmount") ? Convert.ToDecimal(recordData["totalAmount"]) : 0;
                            decimal paidAmount = recordData.ContainsKey("paidAmount") ? Convert.ToDecimal(recordData["paidAmount"]) : 0;
                            
                            decimal currentBalance = totalAmount - paidAmount;

                            // Bakiye Kontrolü
                            if (currentBalance <= 0)
                                return BadRequest(new { message = "Tuition is already fully paid." });

                            if (request.Amount > currentBalance)
                                return BadRequest(new { message = $"Amount cannot be greater than remaining balance ({currentBalance})." });

                            // Ödemeyi İşle
                            newTotalPaid = paidAmount + request.Amount;
                            
                            // Durumu Güncelle
                            newStatus = (newTotalPaid >= totalAmount) ? "Paid" : "Partial";

                            recordData["paidAmount"] = Convert.ToDouble(newTotalPaid);
                            recordData["status"] = newStatus;
                            
                            newBalance = totalAmount - newTotalPaid;
                            recordFound = true;
                        }
                        updatedRecords.Add(recordData);
                    }
                }

                if (!recordFound)
                {
                    return NotFound(new { message = $"Tuition record for term '{request.Term}' not found." });
                }

                // 4. Güncel listeyi Firestore'a geri yaz
                studentData["tuitionRecords"] = updatedRecords;
                await docRef.SetAsync(studentData);
            }
            else
            {
                 return NotFound(new { message = "No tuition records found for this student." });
            }

            // 5. Başarılı Yanıt Dön
            var response = new PayTuitionResponseDto
            {
                Success = true,
                Message = "Payment processed successfully.",
                StudentNumber = request.StudentNumber,
                Term = request.Term,
                PaidAmount = request.Amount,
                TotalPaid = newTotalPaid,
                TotalAmount = totalAmount,
                Balance = newBalance,
                TuitionStatus = newStatus,
                TransactionReference = Guid.NewGuid().ToString()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}


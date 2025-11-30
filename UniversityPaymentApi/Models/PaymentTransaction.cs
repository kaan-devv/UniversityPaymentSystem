using System.ComponentModel.DataAnnotations;

namespace UniversityPaymentApi.Models;

public class PaymentTransaction
{
    [Key]
    public int TransactionId { get; set; }

    public int RecordId { get; set; }
    public TuitionRecord TuitionRecord { get; set; } = null!;

    public decimal PaymentAmount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public string ReferenceCode { get; set; } = Guid.NewGuid().ToString();

    public string Status { get; set; } = "SUCCESS";
}

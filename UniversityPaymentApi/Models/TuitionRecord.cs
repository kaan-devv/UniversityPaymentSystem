using System.ComponentModel.DataAnnotations;
namespace UniversityPaymentApi.Models;

public class TuitionRecord
{
    [Key]
    public int RecordId { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public string Term { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    
    public decimal PaidAmount { get; set; } = 0;

    public decimal Balance => TotalAmount - PaidAmount;

    public string Status { get; set; } = "UNPAID";

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
}

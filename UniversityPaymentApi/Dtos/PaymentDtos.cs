namespace UniversityPaymentApi.Dtos;

public class PayTuitionRequestDto
{
    public string StudentNumber { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class PayTuitionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string StudentNumber { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;

    public decimal PaidAmount { get; set; }     
    public decimal TotalPaid { get; set; }    
    public decimal TotalAmount { get; set; }  
    public decimal Balance { get; set; }      

    public string TuitionStatus { get; set; } = string.Empty;
    public string TransactionReference { get; set; } = string.Empty;
}

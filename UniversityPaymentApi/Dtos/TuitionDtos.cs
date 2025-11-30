namespace UniversityPaymentApi.Dtos;

public class TuitionRecordDto
{
    public string Term { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TuitionSummaryDto
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }

    public List<TuitionRecordDto> Records { get; set; } = new();
}

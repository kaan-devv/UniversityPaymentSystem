namespace UniversityPaymentApi.Dtos;

public class AddTuitionRequestDto
{
    public string StudentNumber { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class AddTuitionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string StudentNumber { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class AddTuitionBatchResultItemDto
{
    public int LineNumber { get; set; }
    public string RawLine { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class AddTuitionBatchResponseDto
{
    public int TotalLines { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<AddTuitionBatchResultItemDto> Results { get; set; } = new();
}

public class UnpaidTuitionItemDto
{
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
}

public class UnpaidTuitionPageDto
{
    public string Term { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public List<UnpaidTuitionItemDto> Items { get; set; } = new();
}

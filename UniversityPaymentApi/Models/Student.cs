namespace UniversityPaymentApi.Models;

public class Student
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public ICollection<TuitionRecord> TuitionRecords { get; set; } = new List<TuitionRecord>();
}

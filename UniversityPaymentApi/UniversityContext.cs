using Microsoft.EntityFrameworkCore;
using UniversityPaymentApi.Models; 

namespace UniversityPaymentApi 
{
    public class UniversityContext : DbContext
    {
        public UniversityContext(DbContextOptions<UniversityContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<TuitionRecord> TuitionRecords { get; set; } = null!;
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
    }
}
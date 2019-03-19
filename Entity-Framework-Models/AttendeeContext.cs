using Microsoft.EntityFrameworkCore;

namespace Pivotal.Workshop.Models
{
    public class AttendeeContext : DbContext
    {
        public AttendeeContext(DbContextOptions<AttendeeContext> options)
            : base(options)
        {
        }

        public DbSet<Pivotal.Workshop.Models.AttendeeModel> AttendeeModel { get; set; }
    }
}
using Microsoft.EntityFrameworkCore;

namespace Circle_API.Models
{
    public class CircleDbContext : DbContext
    {
        public CircleDbContext(DbContextOptions<CircleDbContext> options) : base(options) { }
        

        public DbSet<User> Users { get; set; }
        
    }
}

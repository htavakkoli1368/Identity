using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using security.Models;

namespace security.Data
{
    public class SecurityDbContext: IdentityDbContext
    {
        public SecurityDbContext(DbContextOptions options ):base(options)
        {
                
        }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}

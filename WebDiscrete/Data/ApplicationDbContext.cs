using Microsoft.EntityFrameworkCore;
using WebDiscrete.Data.Entity;

namespace WebDiscrete.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<SystemObject> SystemObjects { get; set; }
        public DbSet<ObjectAccessRights> ObjectAccessRights { get; set; }
    }
}
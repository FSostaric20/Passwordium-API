using Microsoft.EntityFrameworkCore;
using Passwordium_api.Model.Entities;

namespace Passwordium_api.Data {
    public class DatabaseContext : DbContext {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
    }
}

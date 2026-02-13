using Microsoft.EntityFrameworkCore;

namespace Zed.EntityFrameworkCore.Tests.Model {
    public class TestDbContext : DbContext {

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new TagMapping());
        }
    }
}

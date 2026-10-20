using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Data
{
    public class CmsContext : DbContext
    {
        public CmsContext(DbContextOptions<CmsContext> options) : base(options)
        {
        }

        public DbSet<Member> Members { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ItemImage>()
                .HasKey(img => new { img.ItemId, img.ImageUrl });
        }
    }
}

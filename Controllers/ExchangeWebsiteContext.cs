using ExchangeWebsite.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExchangeWebsite.Controllers;

public class ExchangeWebsiteContext : IdentityDbContext<IdentityUser>
{
    public ExchangeWebsiteContext(DbContextOptions<ExchangeWebsiteContext> options)
        : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<PostImage> PostImages { get; set; } = null!;
    public DbSet<ExchangeWebsite.Models.Comment> Comments { get; set; }
    public DbSet<Bill> Bills { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<CommentReport> CommentReports { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ShippingRequest> ShippingRequests { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());

        builder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Post>()
            .HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<PostImage>()
            .HasOne(pi => pi.Post)
            .WithMany(p => p.PostImages)
            .HasForeignKey(pi => pi.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);
    }
}
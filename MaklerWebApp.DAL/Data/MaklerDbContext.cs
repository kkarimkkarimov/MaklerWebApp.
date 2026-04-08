using MaklerWebApp.DAL.Entities;
using MaklerWebApp.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace MaklerWebApp.DAL.Data;

public class MaklerDbContext : DbContext
{
    public MaklerDbContext(DbContextOptions<MaklerDbContext> options) : base(options)
    {
    }

    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<ListingImage> ListingImages => Set<ListingImage>();
    public DbSet<ListingTranslation> ListingTranslations => Set<ListingTranslation>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<ListingView> ListingViews => Set<ListingView>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.ToTable("Listings");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.City).IsRequired().HasMaxLength(80);
            entity.Property(x => x.District).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Address).IsRequired().HasMaxLength(250);
            entity.Property(x => x.ContactName).IsRequired().HasMaxLength(120);
            entity.Property(x => x.ContactPhone).IsRequired().HasMaxLength(30);
            entity.Property(x => x.ModerationNote).HasMaxLength(500);
            entity.Property(x => x.RepairStatus).HasDefaultValue(RepairStatus.NoRepair);
            entity.Property(x => x.DocumentStatus).HasDefaultValue(DocumentStatus.NoDocument);
            entity.Property(x => x.IsMortgageEligible).HasDefaultValue(false);

            entity.HasIndex(x => new { x.City, x.District });
            entity.HasIndex(x => new { x.ListingType, x.PropertyType });
            entity.HasIndex(x => new { x.RepairStatus, x.DocumentStatus, x.IsMortgageEligible });
            entity.HasIndex(x => x.Price);
            entity.HasIndex(x => x.PublishedAt);
            entity.HasIndex(x => new { x.IsDeleted, x.Status, x.IsFeatured });
            entity.HasIndex(x => new { x.OwnerUserId, x.IsDeleted });

            entity.HasMany(x => x.Images)
                .WithOne(x => x.Listing)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Translations)
                .WithOne(x => x.Listing)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Views)
                .WithOne(x => x.Listing)
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListingImage>(entity =>
        {
            entity.ToTable("ListingImages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);
            entity.HasIndex(x => new { x.ListingId, x.SortOrder });
        });

        modelBuilder.Entity<ListingTranslation>(entity =>
        {
            entity.ToTable("ListingTranslations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(150);
            entity.Property(x => x.Description).IsRequired().HasMaxLength(4000);
            entity.HasIndex(x => new { x.ListingId, x.LanguageCode }).IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FullName).IsRequired().HasMaxLength(120);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(200);
            entity.Property(x => x.PhoneNumber).HasMaxLength(30);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            entity.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(256);
            entity.Property(x => x.ProfileImageUrl).HasMaxLength(500);
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).IsRequired().HasMaxLength(300);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("Favorites");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ListingView>(entity =>
        {
            entity.ToTable("ListingViews");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ListingId, x.ViewedAt });
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.ToTable("PaymentTransactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Reference).IsRequired().HasMaxLength(64);
            entity.HasIndex(x => x.Reference).IsUnique();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Listing)
                .WithMany()
                .HasForeignKey(x => x.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

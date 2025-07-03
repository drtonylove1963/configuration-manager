using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Data;

public class ConfigurationDbContext : DbContext
{
    public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : base(options)
    {
    }

    public DbSet<Configuration> Configurations { get; set; } = null!;
    public DbSet<Domain.Entities.Environment> Environments { get; set; } = null!;
    public DbSet<ConfigurationGroup> ConfigurationGroups { get; set; } = null!;
    public DbSet<ConfigurationHistory> ConfigurationHistory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Configuration entity
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            // Configure ConfigurationKey value object
            entity.Property(e => e.Key)
                .HasConversion(
                    key => key.Value,
                    value => ConfigurationKey.Create(value))
                .HasMaxLength(200)
                .IsRequired();

            // Configure ConfigurationValue value object
            entity.OwnsOne(e => e.Value, valueBuilder =>
            {
                valueBuilder.Property(v => v.Value)
                    .HasColumnName("Value")
                    .HasMaxLength(4000)
                    .IsRequired();

                valueBuilder.Property(v => v.Type)
                    .HasColumnName("ValueType")
                    .HasConversion<string>()
                    .IsRequired();
            });

            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DefaultValue).HasMaxLength(4000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);

            // Configure relationships
            entity.HasOne(e => e.Environment)
                .WithMany(env => env.Configurations)
                .HasForeignKey(e => e.EnvironmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Group)
                .WithMany(g => g.Configurations)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes
            entity.HasIndex(e => new { e.Key, e.EnvironmentId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.EnvironmentId);
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);

            // Configure table name
            entity.ToTable("Configurations");
        });

        // Configure Environment entity
        modelBuilder.Entity<Domain.Entities.Environment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);

            // Configure indexes
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.SortOrder);

            // Configure table name
            entity.ToTable("Environments");
        });

        // Configure ConfigurationGroup entity
        modelBuilder.Entity<ConfigurationGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentGroupId);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);

            // Configure self-referencing relationship
            entity.HasMany(e => e.ChildGroups)
                .WithOne()
                .HasForeignKey(e => e.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            entity.HasIndex(e => e.ParentGroupId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.SortOrder);

            // Configure table name
            entity.ToTable("ConfigurationGroups");
        });

        // Configure ConfigurationHistory entity
        modelBuilder.Entity<ConfigurationHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.OldValue).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.NewValue).HasMaxLength(4000).IsRequired();
            entity.Property(e => e.OldValueType).HasConversion<string>().IsRequired();
            entity.Property(e => e.NewValueType).HasConversion<string>().IsRequired();
            entity.Property(e => e.ChangedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ChangeReason).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);

            // Configure relationship
            entity.HasOne(e => e.Configuration)
                .WithMany(c => c.History)
                .HasForeignKey(e => e.ConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(e => e.ConfigurationId);
            entity.HasIndex(e => e.ChangedAt);
            entity.HasIndex(e => e.ChangedBy);

            // Configure table name
            entity.ToTable("ConfigurationHistory");
        });

        // Configure global query filters for soft delete
        modelBuilder.Entity<Configuration>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Domain.Entities.Environment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConfigurationGroup>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                // CreatedAt and CreatedBy are set in the entity constructor
            }
            else if (entry.State == EntityState.Modified)
            {
                // UpdatedAt and UpdatedBy are set by calling MarkAsUpdated in the entity
                entry.Property(nameof(Domain.Common.BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(Domain.Common.BaseEntity.CreatedBy)).IsModified = false;
            }
        }
    }
}

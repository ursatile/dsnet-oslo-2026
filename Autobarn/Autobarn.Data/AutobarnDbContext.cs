using Autobarn.Data.Entities;
using Autobarn.Data.Sample;
using Microsoft.EntityFrameworkCore;

namespace Autobarn.Data;

public class AutobarnDbContext(
	DbContextOptions<AutobarnDbContext> options
) : DbContext(options) {

	public virtual DbSet<VehicleMake> Makes { get; set; }
	public virtual DbSet<VehicleModel> Models { get; set; }
	public virtual DbSet<Vehicle> Vehicles { get; set; }

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
		if (Database.IsSqlite()) {
			configurationBuilder.Properties<string>().UseCollation("NOCASE");
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder) {

		modelBuilder.Entity<VehicleMake>(entity => {
			entity.HasKey(e => e.Code);
			entity.Property(e => e.Code).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.Name).HasMaxLength(32).IsUnicode(false);
			entity.HasMany(e => e.Models).WithOne(m => m.VehicleMake).HasForeignKey(m => m.MakeCode);
		});

		modelBuilder.Entity<VehicleModel>(entity => {
			entity.HasKey(e => e.Code);
			entity.Property(e => e.Code).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.MakeCode).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.Name).HasMaxLength(32).IsUnicode(false);
			entity.HasMany(e => e.Vehicles).WithOne(v => v.Model).HasForeignKey(v => v.ModelCode);
		});

		modelBuilder.Entity<Vehicle>(entity => {
			entity.HasKey(e => e.Registration);
			entity.Property(e => e.Registration).HasMaxLength(16).IsUnicode(false);
			entity.Property(e => e.Color).HasMaxLength(32).IsUnicode(false);
			entity.Property(e => e.ModelCode).HasMaxLength(32).IsUnicode(false);
		});

		modelBuilder.Entity<VehicleMake>().HasData(SampleData.VehicleMakeData);
		modelBuilder.Entity<VehicleModel>().HasData(SampleData.VehicleModelData);
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
		optionsBuilder
			.UseSeeding((dbContext, _) => {
				if (dbContext.Set<Vehicle>().Any()) return;
				dbContext.AddRange(SampleData.Vehicles);
				dbContext.SaveChanges();
			})
			.UseAsyncSeeding(async (dbContext, _, cancellationToken) => {
				if (await dbContext.Set<Vehicle>().AnyAsync(cancellationToken)) return;
				await dbContext.AddRangeAsync(SampleData.Vehicles, cancellationToken);
				await dbContext.SaveChangesAsync(cancellationToken);
			});
	}
}

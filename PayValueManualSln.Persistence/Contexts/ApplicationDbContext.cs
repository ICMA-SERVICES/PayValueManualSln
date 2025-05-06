using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Domain.Entities;
using PayValueManualSln.Domain.Entities.Setting;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Persistence.Repositories;
using PayValueV2.Domain.Entities.PayValue;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace PayValueManualSln.Infrastructure.Persistence.Contexts
{
	public partial class ApplicationDbContext : DbContext
	{
		private readonly IDateTimeService _dateTime;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime) : base(options)
		{
			//ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			_dateTime = dateTime;

		}
		public virtual DbSet<Services> Services { get; set; }
		public virtual DbSet<Revenue> Revenue { get; set; }
		public virtual DbSet<BillDetails> BillDetails { get; set; }
		public virtual DbSet<BillInfo> BillInfo { get; set; }
		public virtual DbSet<Assessment> Assessment { get; set; }
        public virtual DbSet<Audit> Audit { get; set; }
        public virtual DbSet<PayerDetails> PayerDetails { get; set; }
        public virtual DbSet<Service> Service { get; set; }
        public virtual DbSet<Rate> Rate { get; set; }
        public virtual DbSet<Agency> Agency { get; set; }
        public virtual DbSet<AgencyLogo> AgencyLogo { get; set; }
        public virtual DbSet<ModuleApprovalConfig> ModuleApprovalConfig { get; set; }
        public virtual DbSet<MapUserApproval> MapUserApproval { get; set; }
        public virtual DbSet<AgencySignature> AgencySignature { get; set; }
        public virtual DbSet<AppModule> AppModule { get; set; }
        public virtual DbSet<DepositOnConsent> DepositOnConsents { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<InputDefinitionMapping> InputDefinitionMapping { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<MerchantConfig> MerchantConfig { get; set; }
        public virtual DbSet<RateRange> Range { get; set; }
        public virtual DbSet<RenewalFrequency> RenewalFrequency { get; set; }
        public virtual DbSet<ServiceDetails> ServiceDetails { get; set; }
        public virtual DbSet<ServiceMethod> ServiceMethod { get; set; }
        public virtual DbSet<ServiceRevenue> ServiceRevenue { get; set; }
        public virtual DbSet<Types> Type { get; set; }
        public virtual DbSet<UserDepartment> UserDepartment { get; set; }
        public virtual DbSet<Zone> Zone { get; set; }
        public virtual DbSet<Input> Input { get; set; }
        public virtual DbSet<DepositOnConsent> DepositOnConsent { get; set; }
        public DbSet<RenewalFrequency> RenewalFrequencies { get; set; }
        public virtual DbSet<ValueTemplateForLocation> ValueTemplateForLocation { get; set; }
        public virtual DbSet<BillAdditionalInfo> BillAdditionalInfo { get; set; }
        public virtual DbSet<ServiceMethodSetup> ServiceMethodSetups { get; set; }
        public virtual DbSet<UsersRolePermission> UsersRolePermission { get; set; }
        public virtual DbSet<MenuSetup> MenuSetup { get; set; }
        public virtual DbSet<AdditionalServiceDetail> AdditionalServiceDetail { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.EnableDetailedErrors(true);
			optionsBuilder.AddInterceptors(new MyCommandInterceptor());

			if (!optionsBuilder.IsConfigured)
			{
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
				optionsBuilder.UseSqlServer("Server=LAPTOP-7AOALMT8;Database=PayValueManual;Trusted_Connection=True;");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            //All Decimals will have 18,2 Range

            modelBuilder.Entity<Agency>()
   .HasOne(a => a.AgencySignature)
   .WithOne(s => s.Agency)
   .HasForeignKey<AgencySignature>(s => s.Id);
            modelBuilder.Entity<Agency>()
    .HasKey(a => new { a.Code, a.Id });

            modelBuilder.Entity<AgencySignature>()
                .HasOne(s => s.Agency)
                .WithOne(a => a.AgencySignature)
                .HasForeignKey<AgencySignature>(s => new { s.AgencyCode, s.Id });
            modelBuilder.Entity<Rate>()
    .HasOne(r => r.ServiceRevenue)
    .WithMany(sr => sr.Rate)
    .HasForeignKey(r => r.ServiceRevenueId)
    .OnDelete(DeleteBehavior.Restrict); // <- THIS breaks the cascade path

            modelBuilder.Entity<Revenue>()
            .HasOne(r => r.Service)  
            .WithMany(s => s.Revenues)  
            .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Agency>()
           .HasKey(a => new { a.Code, a.Id });

            modelBuilder.Entity<Revenue>(entity =>
			{
				entity.ToTable("Revenue", "dbo");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.CreatedBy)
					.IsRequired()
					.HasMaxLength(350);

				entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

				entity.Property(e => e.DeletedBy).HasMaxLength(350);

				entity.Property(e => e.FormulaeValue)
					.IsRequired()
					.HasMaxLength(100)
					.IsUnicode(false);

				entity.Property(e => e.IsAdditionalInfoRequired)
					.IsRequired()
					.HasDefaultValueSql("((1))");

				entity.Property(e => e.LastUpdated).HasColumnType("datetime");

				entity.Property(e => e.PaymentItemName)
					.IsRequired()
					.HasMaxLength(350)
					.IsUnicode(false);

				entity.Property(e => e.RevenueCode)
					.IsRequired()
					.HasMaxLength(350)
					.IsUnicode(false);

				entity.Property(e => e.RevenueName)
					.IsRequired()
					.HasMaxLength(350)
					.IsUnicode(false);

				entity.Property(e => e.UpdatedBy).HasMaxLength(350);

				entity.HasOne(d => d.Service)
					.WithMany(p => p.Revenues)
					.HasForeignKey(d => d.ServiceId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("FK_ServiceRevenue_Services");
			});

			modelBuilder.Entity<BillDetails>()
		.Property(e => e.BillBalance)
		.HasComputedColumnSql("[TotalBillAmount] - [BillAmountPaid]");

			modelBuilder.Entity<Services>(entity =>
			{
				entity.ToTable("Services", "dbo");
				entity.HasKey(e => e.Id);
				entity.Property(e => e.AgencyCode)
					.HasMaxLength(150)
					.IsUnicode(false);

				entity.Property(e => e.CreatedBy)
					.IsRequired()
					.HasMaxLength(350);

				entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");

				entity.Property(e => e.DeletedBy).HasMaxLength(350);

				entity.Property(e => e.Description)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.IsApproved)
					.IsRequired()
					.HasDefaultValueSql("((1))");

				entity.Property(e => e.LastUpdated).HasColumnType("datetime");

				entity.Property(e => e.Name)
					.HasMaxLength(350)
					.IsUnicode(false);

				entity.Property(e => e.PaymentPeriod)
					.HasMaxLength(50)
					.IsUnicode(false);

				entity.Property(e => e.ReminderRequired).HasDefaultValueSql("((0))");

				entity.Property(e => e.IsValueInputRequired)
				   .IsRequired()
				   .HasDefaultValueSql("((1))");


				entity.Property(e => e.ServiceHeader)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.ServiceSubHeader)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.SignatoryName)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.SignatoryPosition)
					.HasMaxLength(200)
					.IsUnicode(false);

				entity.Property(e => e.StandardLetterDescriptions).IsUnicode(false);

				entity.Property(e => e.UpdatedBy).HasMaxLength(350);
			});

			foreach (var property in modelBuilder.Model.GetEntityTypes()
					   .SelectMany(t => t.GetProperties())
					   .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
			{
				property.SetColumnType("decimal(18,6)");
			}
			base.OnModelCreating(modelBuilder);

			OnModelCreatingPartial(modelBuilder);

		}


		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


		//protected override void OnModelCreating(ModelBuilder builder)
		//{
		//    //All Decimals will have 18,6 Range
		//    foreach (var property in builder.Model.GetEntityTypes()
		//    .SelectMany(t => t.GetProperties())
		//    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
		//    {
		//        property.SetColumnType("decimal(18,6)");
		//    }
		//    base.OnModelCreating(builder);
		//}
	}
}

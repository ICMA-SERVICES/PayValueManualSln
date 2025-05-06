using Microsoft.EntityFrameworkCore;
using PayValueManualSln.Domain.Entities;

namespace PayValueManualSln.Infrastructure.Persistence.Contexts
{
    public partial class IcmaCollectionContext : DbContext
    {
        public IcmaCollectionContext()
        {
        }

        public IcmaCollectionContext(DbContextOptions<IcmaCollectionContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CollectionReport> CollectionReports { get; set; }

        public virtual DbSet<CollectionRevenue> CollectionRevenue { get; set; }

        public virtual DbSet<CollectionAgency> CollectionAgency { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlServer("Server=DELL;Database=IcmaCollection;Trusted_Connection=true;");
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=IcmaCollection;Trusted_Connection=true;Uid=sa;Password=1964;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionReport>(entity =>
            {
                entity.HasKey(e => e.PaymentRefNumber);

                entity.ToTable("CollectionReport", "Collection");

                entity.HasIndex(e => e.PaymentDate)
                    .HasName("idx_PaymentDate");

                entity.HasIndex(e => e.PaymentRefNumber)
                    .HasName("idx_PaymentRefNumber")
                    .IsUnique();

                entity.HasIndex(e => new { e.ColProviderCode, e.ChannelCode, e.DepositSlipNumber, e.PaymentMethodCode, e.MerchantCode, e.PaymentDate, e.Amount })
                    .HasName("idx_Clustered");

                entity.HasIndex(e => new { e.ColProviderCode, e.ChannelCode, e.DepositSlipNumber, e.PaymentMethodCode, e.AgencyId, e.PaymentDate, e.Amount, e.ReversalId })
                    .HasName("idx_NonClustered_dobyLastThreeYears");

                entity.Property(e => e.PaymentRefNumber).HasMaxLength(50);

                entity.Property(e => e.AgencyCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.AgencyName).IsRequired();

                entity.Property(e => e.Amount).HasColumnType("money");

                entity.Property(e => e.Balance).HasColumnType("money");

                entity.Property(e => e.BankCode).HasMaxLength(50);

                entity.Property(e => e.BranchCode).HasMaxLength(50);

                entity.Property(e => e.ChannelCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ChequeBankCode).HasMaxLength(50);

                entity.Property(e => e.ChequeBankName).HasMaxLength(500);

                entity.Property(e => e.ChequeNumber).HasMaxLength(50);

                entity.Property(e => e.ColProviderCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ControlNumber).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                //entity.Property(e => e.CurrencyCode)
                //    .IsRequired()
                //    .HasMaxLength(5)
                //    .IsUnicode(false)
                //    .HasDefaultValueSql("('NGN')");

                entity.Property(e => e.DepositSlipNumber).HasMaxLength(150);

                entity.Property(e => e.FinalUtin)
                    .HasColumnName("FinalUTIN")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.IcmaReceipt).HasMaxLength(100);

                entity.Property(e => e.IcmaReceiptDate).HasColumnType("datetime");

                entity.Property(e => e.IsModified).HasDefaultValueSql("((0))");

                entity.Property(e => e.IsRepositoryUpdate)
                    .HasColumnName("isRepositoryUpdate")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.MerchantCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.NormalisedBy)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.NormalisedDate).HasColumnType("datetime");

                entity.Property(e => e.PayerId).HasColumnName("PayerID");

                entity.Property(e => e.PayerUtin).HasMaxLength(100);

                entity.Property(e => e.PaymentDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentId).ValueGeneratedOnAdd();

                //  entity.Property(e => e.UsedByPlatform).ValueGeneratedOnAdd();

                entity.Property(e => e.PaymentMethodCode).HasMaxLength(40);

                entity.Property(e => e.PaymentValueStatusCode).HasMaxLength(40);

                entity.Property(e => e.PlatformCode).HasMaxLength(10);

                entity.Property(e => e.ReceiptDate).HasColumnType("datetime");

                entity.Property(e => e.ReceiptNo).HasMaxLength(100);

                entity.Property(e => e.RevenueCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RevenueName).IsRequired();

                entity.Property(e => e.ReversalId).HasMaxLength(100);

                entity.Property(e => e.ReversedDate).HasColumnType("datetime");

                entity.Property(e => e.TransactionReference).HasMaxLength(50);

                entity.Property(e => e.ValueDate).HasColumnType("datetime");

                entity.Property(e => e.ValueStatus).HasMaxLength(40);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
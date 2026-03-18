using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ventas.Models;

namespace Ventas.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<CompanySettings> Companies => Set<CompanySettings>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductService> ProductServices => Set<ProductService>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<CreditNoteItem> CreditNoteItems => Set<CreditNoteItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<SupplierInvoice> SupplierInvoices => Set<SupplierInvoice>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<CashBankAccount> CashBankAccounts => Set<CashBankAccount>();
    public DbSet<CashBankMovement> CashBankMovements => Set<CashBankMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Receipt>()
            .HasOne(x => x.Invoice)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CreditNote>()
            .HasOne(x => x.Invoice)
            .WithMany(x => x.CreditNotes)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CompanySettings>()
            .Property(x => x.LogoUrl)
            .HasMaxLength(300);

        modelBuilder.Entity<ProductService>()
            .Property(x => x.SalePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Quote>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Invoice>().Property(x => x.BalanceDue).HasPrecision(18, 2);
        modelBuilder.Entity<CreditNote>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Purchase>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierInvoice>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<CashBankAccount>().Property(x => x.Balance).HasPrecision(18, 2);

        ConfigureDocumentItem<QuoteItem>(modelBuilder);
        ConfigureDocumentItem<InvoiceItem>(modelBuilder);
        ConfigureDocumentItem<CreditNoteItem>(modelBuilder);
        ConfigurePurchaseItem(modelBuilder);

        modelBuilder.Entity<Receipt>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<SupplierPayment>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<CashBankMovement>().Property(x => x.Amount).HasPrecision(18, 2);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    private static void ConfigureDocumentItem<T>(ModelBuilder modelBuilder) where T : DocumentItem
    {
        modelBuilder.Entity<T>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<T>().Property(x => x.Quantity).HasPrecision(18, 2);
        modelBuilder.Entity<T>().Property(x => x.Total).HasPrecision(18, 2);
    }

    private static void ConfigurePurchaseItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseItem>().Property(x => x.UnitCost).HasPrecision(18, 2);
        modelBuilder.Entity<PurchaseItem>().Property(x => x.FixedProfitAmount).HasPrecision(18, 2);
        modelBuilder.Entity<PurchaseItem>().Property(x => x.Quantity).HasPrecision(18, 2);
        modelBuilder.Entity<PurchaseItem>().Property(x => x.Total).HasPrecision(18, 2);
    }

    private void ApplyAuditInfo()
    {
        var userName =
            httpContextAccessor.HttpContext?.User?.Identity?.Name
            ?? httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? "Sistema";

        var now = DateTime.Now;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userName;
                entry.Entity.UpdatedAt = null;
                entry.Entity.UpdatedBy = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Property(x => x.CreatedBy).IsModified = false;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userName;
            }
        }
    }
}

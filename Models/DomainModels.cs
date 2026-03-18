using System.ComponentModel.DataAnnotations;

namespace Ventas.Models;

public enum PaymentMethod
{
    Cash = 1,
    Transfer = 2
}

public enum DocumentStatus
{
    Draft = 1,
    Approved = 2,
    Rejected = 3,
    Issued = 4,
    Cancelled = 5,
    PartiallyPaid = 6,
    Paid = 7
}

public enum AccountType
{
    Cash = 1,
    Bank = 2
}

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [StringLength(150)]
    public string CreatedBy { get; set; } = "Sistema";
    public DateTime? UpdatedAt { get; set; }
    [StringLength(150)]
    public string? UpdatedBy { get; set; }
}

public class Role : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public ICollection<User> Users { get; set; } = new List<User>();
}

public class User : BaseEntity
{
    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int RoleId { get; set; }
    public Role? Role { get; set; }
}

public class CompanySettings : BaseEntity
{
    [Required, StringLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [StringLength(50)]
    public string TaxId { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [StringLength(300)]
    public string? LogoUrl { get; set; }

    [StringLength(10)]
    public string DefaultCurrency { get; set; } = "DOP";
}

public class Customer : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string TaxId { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }
}

public class Supplier : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string TaxId { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class ProductService : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string UnitOfMeasure { get; set; } = "Unidad";

    public decimal SalePrice { get; set; }
}

public abstract class DocumentBase : BaseEntity
{
    [Required, StringLength(20)]
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public PaymentMethod PaymentMethod { get; set; }
    public DocumentStatus Status { get; set; }
    public decimal Total { get; set; }
}

public abstract class DocumentItem : BaseEntity
{
    public int ProductServiceId { get; set; }
    public ProductService? ProductService { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class Quote : DocumentBase
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
}

public class QuoteItem : DocumentItem
{
    public int QuoteId { get; set; }
    public Quote? Quote { get; set; }
}

public class Invoice : DocumentBase
{
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal BalanceDue { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();
}

public class InvoiceItem : DocumentItem
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}

public class CreditNote : DocumentBase
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public ICollection<CreditNoteItem> Items { get; set; } = new List<CreditNoteItem>();
}

public class CreditNoteItem : DocumentItem
{
    public int CreditNoteId { get; set; }
    public CreditNote? CreditNote { get; set; }
}

public class Purchase : DocumentBase
{
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}

public class PurchaseItem : BaseEntity
{
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public int ProductServiceId { get; set; }
    public ProductService? ProductService { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal FixedProfitAmount { get; set; }
    public decimal Total { get; set; }
}

public class SupplierInvoice : DocumentBase
{
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
}

public class Receipt : BaseEntity
{
    public DateTime Date { get; set; } = DateTime.Today;
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    [StringLength(250)]
    public string Reference { get; set; } = string.Empty;
}

public class SupplierPayment : BaseEntity
{
    public DateTime Date { get; set; } = DateTime.Today;
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    [StringLength(250)]
    public string Reference { get; set; } = string.Empty;
}

public class CashBankAccount : BaseEntity
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public ICollection<CashBankMovement> Movements { get; set; } = new List<CashBankMovement>();
}

public class CashBankMovement : BaseEntity
{
    public int CashBankAccountId { get; set; }
    public CashBankAccount? CashBankAccount { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    [Required, StringLength(250)]
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

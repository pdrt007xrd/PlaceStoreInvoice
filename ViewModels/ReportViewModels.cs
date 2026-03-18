using Ventas.Models;

namespace Ventas.ViewModels;

public class CompanyHeaderViewModel
{
    public string BusinessName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ReportFilterViewModel
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Search { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public DocumentStatus? Status { get; set; }
}

public class ReportRowViewModel
{
    public string Date { get; set; } = string.Empty;
    public string Main { get; set; } = string.Empty;
    public string Secondary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string AmountText { get; set; } = string.Empty;
}

public class ReportResultViewModel
{
    public string Title { get; set; } = string.Empty;
    public CompanyHeaderViewModel Company { get; set; } = new();
    public ReportFilterViewModel Filters { get; set; } = new();
    public string DateHeader { get; set; } = "Fecha";
    public string MainHeader { get; set; } = "Principal";
    public string SecondaryHeader { get; set; } = "Detalle";
    public string StatusHeader { get; set; } = "Estado";
    public string AmountHeader { get; set; } = "Monto";
    public List<ReportRowViewModel> Rows { get; set; } = [];
}

public class InvoicePdfItemViewModel
{
    public string Product { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class InvoicePdfViewModel
{
    public CompanyHeaderViewModel Company { get; set; } = new();
    public string DocumentTitle { get; set; } = "Factura";
    public string Number { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string CustomerTaxId { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<InvoicePdfItemViewModel> Items { get; set; } = [];
}

public class InvoicePostPrintViewModel
{
    public int InvoiceId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal BalanceDue { get; set; }
}

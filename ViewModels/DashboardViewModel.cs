namespace Ventas.ViewModels;

public class DashboardViewModel
{
    public int Customers { get; set; }
    public int Suppliers { get; set; }
    public int Products { get; set; }
    public int QuotesToday { get; set; }
    public int InvoicesToday { get; set; }
    public decimal SalesToday { get; set; }
    public decimal PurchasesToday { get; set; }
    public decimal CashBalance { get; set; }
    public List<string> Alerts { get; set; } = [];
}

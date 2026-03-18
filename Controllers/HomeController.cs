using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;
using Ventas.ViewModels;

namespace Ventas.Controllers;

[Authorize]
public class HomeController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var dashboard = new DashboardViewModel
        {
            Customers = await context.Customers.CountAsync(),
            Suppliers = await context.Suppliers.CountAsync(),
            Products = await context.ProductServices.CountAsync(),
            QuotesToday = await context.Quotes.CountAsync(x => x.Date == today),
            InvoicesToday = await context.Invoices.CountAsync(x => x.Date == today),
            SalesToday = await context.Invoices.Where(x => x.Date == today).SumAsync(x => (decimal?)x.Total) ?? 0,
            PurchasesToday = await context.Purchases.Where(x => x.Date == today).SumAsync(x => (decimal?)x.Total) ?? 0,
            CashBalance = await context.CashBankAccounts.SumAsync(x => (decimal?)x.Balance) ?? 0
        };

        if (dashboard.QuotesToday == 0)
        {
            dashboard.Alerts.Add("No hay cotizaciones registradas hoy.");
        }

        if (dashboard.InvoicesToday == 0)
        {
            dashboard.Alerts.Add("No hay facturas emitidas hoy.");
        }

        if (dashboard.CashBalance <= 0)
        {
            dashboard.Alerts.Add("La caja y bancos no tienen balance positivo.");
        }

        return View(dashboard);
    }
}

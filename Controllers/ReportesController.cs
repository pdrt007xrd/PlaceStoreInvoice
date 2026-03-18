using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Services;
using Ventas.ViewModels;

namespace Ventas.Controllers;

[Authorize]
public class ReportesController(AppDbContext context, PdfReportService pdfReportService) : Controller
{
    public async Task<IActionResult> Ventas(DateTime? from, DateTime? to, string search = "")
        => View("Ventas/Ventas", await BuildVentasReportAsync("Reporte de ventas", from, to, search));

    public async Task<IActionResult> Compras(DateTime? from, DateTime? to, string search = "")
        => View("Compras/Compras", await BuildComprasReportAsync("Reporte de compras", from, to, search));

    public async Task<IActionResult> CuentasPorCobrar(DateTime? from, DateTime? to, string search = "")
        => View("CuentasPorCobrar/CuentasPorCobrar", await BuildCuentasPorCobrarAsync("Cuentas por cobrar", from, to, search));

    public async Task<IActionResult> CuentasPorPagar(DateTime? from, DateTime? to, string search = "")
        => View("CuentasPorPagar/CuentasPorPagar", await BuildCuentasPorPagarAsync("Cuentas por pagar", from, to, search));

    public async Task<IActionResult> NotasCredito(DateTime? from, DateTime? to, string search = "")
        => View("NotasCredito/NotasCredito", await BuildNotasCreditoAsync("Reporte de notas de credito", from, to, search));

    public async Task<IActionResult> LibroVentas(DateTime? from, DateTime? to, string search = "")
        => View("LibroVentas/LibroVentas", await BuildVentasReportAsync("Libro de ventas", from, to, search));

    public async Task<IActionResult> LibroCompras(DateTime? from, DateTime? to, string search = "")
        => View("LibroCompras/LibroCompras", await BuildComprasReportAsync("Libro de compras", from, to, search));

    public async Task<IActionResult> FlujoCaja(DateTime? from, DateTime? to, string search = "")
        => View("FlujoCaja/FlujoCaja", await BuildFlujoCajaAsync("Flujo de caja", from, to, search));

    public async Task<FileResult> ExportPdf(string type, DateTime? from, DateTime? to, string search = "")
    {
        var report = type switch
        {
            "ventas" => await BuildVentasReportAsync("Reporte de ventas", from, to, search),
            "compras" => await BuildComprasReportAsync("Reporte de compras", from, to, search),
            "cxc" => await BuildCuentasPorCobrarAsync("Cuentas por cobrar", from, to, search),
            "cxp" => await BuildCuentasPorPagarAsync("Cuentas por pagar", from, to, search),
            "notas-credito" => await BuildNotasCreditoAsync("Reporte de notas de credito", from, to, search),
            "libro-ventas" => await BuildVentasReportAsync("Libro de ventas", from, to, search),
            "libro-compras" => await BuildComprasReportAsync("Libro de compras", from, to, search),
            _ => await BuildFlujoCajaAsync("Flujo de caja", from, to, search)
        };

        var bytes = pdfReportService.Generate(report);
        Response.Headers.ContentDisposition = $"inline; filename={type}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(bytes, "application/pdf");
    }

    private async Task<ReportResultViewModel> BuildVentasReportAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var query = context.Invoices.Include(x => x.Customer).AsQueryable();
        query = ApplyDates(query, from, to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Number.Contains(search) || x.Customer!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = await query.OrderByDescending(x => x.Date).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Number,
                Secondary = x.Customer!.Name,
                Status = x.Status.ToString(),
                Amount = x.Total
            }).ToListAsync()
        };
    }

    private async Task<ReportResultViewModel> BuildComprasReportAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var query = context.Purchases.Include(x => x.Supplier).Include(x => x.Items).ThenInclude(x => x.ProductService).AsQueryable();
        query = ApplyDates(query, from, to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Number.Contains(search) || x.Supplier!.Name.Contains(search));
        }

        var rows = await query.OrderByDescending(x => x.Date).SelectMany(x => x.Items.Select(item => new ReportRowViewModel
        {
            Date = x.Date.ToString("dd/MM/yyyy"),
            Main = x.Supplier!.Name,
            Secondary = $"{item.ProductService!.Name} / Cant. {item.Quantity:N2}",
            Status = x.Status.ToString(),
            Amount = item.Total
        })).ToListAsync();

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = rows
        };
    }

    private async Task<ReportResultViewModel> BuildCuentasPorCobrarAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var invoices = context.Invoices.Include(x => x.Customer).Where(x => x.Status != Models.DocumentStatus.Paid).AsQueryable();
        invoices = ApplyDates(invoices, from, to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            invoices = invoices.Where(x => x.Customer!.Name.Contains(search) || x.Number.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = await invoices.OrderByDescending(x => x.Date).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Customer!.Name,
                Secondary = $"Factura {x.Number} / {GetAgeRange(x.Date)}",
                Status = x.Status.ToString(),
                Amount = x.Total
            }).ToListAsync()
        };
    }

    private async Task<ReportResultViewModel> BuildCuentasPorPagarAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var invoices = context.SupplierInvoices.Include(x => x.Purchase).ThenInclude(x => x!.Supplier).Where(x => x.Status != Models.DocumentStatus.Paid).AsQueryable();
        invoices = ApplyDates(invoices, from, to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            invoices = invoices.Where(x => x.Number.Contains(search) || x.Purchase!.Supplier!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = await invoices.OrderByDescending(x => x.Date).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Purchase!.Supplier!.Name,
                Secondary = $"Factura proveedor {x.Number} / {GetAgeRange(x.Date)}",
                Status = x.Status.ToString(),
                Amount = x.Total
            }).ToListAsync()
        };
    }

    private async Task<ReportResultViewModel> BuildNotasCreditoAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var query = context.CreditNotes
            .Include(x => x.Invoice)
            .ThenInclude(x => x!.Customer)
            .AsQueryable();

        query = ApplyDates(query, from, to);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Number.Contains(search) ||
                x.Invoice!.Number.Contains(search) ||
                x.Invoice.Customer!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = await query.OrderByDescending(x => x.Date).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Invoice!.Customer!.Name,
                Secondary = $"Nota {x.Number} / Factura {x.Invoice.Number}",
                Status = x.Status.ToString(),
                Amount = x.Total
            }).ToListAsync()
        };
    }

    private async Task<ReportResultViewModel> BuildFlujoCajaAsync(string title, DateTime? from, DateTime? to, string search)
    {
        var query = context.CashBankMovements.Include(x => x.CashBankAccount).AsQueryable();
        if (from.HasValue) query = query.Where(x => x.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(x => x.Date <= to.Value.Date);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Description.Contains(search) || x.CashBankAccount!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search },
            Rows = await query.OrderByDescending(x => x.Date).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.CashBankAccount!.Name,
                Secondary = x.Description,
                Status = x.Amount >= 0 ? "Ingreso" : "Egreso",
                Amount = x.Amount
            }).ToListAsync()
        };
    }

    private static IQueryable<T> ApplyDates<T>(IQueryable<T> query, DateTime? from, DateTime? to) where T : Models.DocumentBase
    {
        if (from.HasValue) query = query.Where(x => x.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(x => x.Date <= to.Value.Date);
        return query;
    }

    private static string GetAgeRange(DateTime date)
    {
        var days = (DateTime.Today - date.Date).Days;
        if (days <= 30) return "0-30 dias";
        if (days <= 60) return "31-60 dias";
        return "61+ dias";
    }

    private async Task<CompanyHeaderViewModel> GetCompanyAsync()
    {
        var company = await context.Companies.OrderBy(x => x.Id).FirstOrDefaultAsync();
        if (company is null)
        {
            return new CompanyHeaderViewModel();
        }

        return new CompanyHeaderViewModel
        {
            BusinessName = company.BusinessName,
            TaxId = company.TaxId,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email
        };
    }
}

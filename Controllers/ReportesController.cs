using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Extensions;
using Ventas.Models;
using Ventas.Services;
using Ventas.ViewModels;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOperadorConsulta")]
public class ReportesController(AppDbContext context, PdfReportService pdfReportService) : Controller
{
    public async Task<IActionResult> Ventas(DateTime? from, DateTime? to, string search = "", int? customerId = null, DocumentStatus? status = null)
    {
        await LoadReportLookupsAsync(customers: true);
        ViewBag.EnableStatusFilter = true;
        return View("Ventas/Ventas", await BuildVentasReportAsync("Reporte de ventas", from, to, search, customerId, status));
    }

    public async Task<IActionResult> Compras(DateTime? from, DateTime? to, string search = "", int? supplierId = null, DocumentStatus? status = null)
    {
        await LoadReportLookupsAsync(suppliers: true);
        ViewBag.EnableStatusFilter = true;
        return View("Compras/Compras", await BuildComprasReportAsync("Reporte de compras", from, to, search, supplierId, status));
    }

    public async Task<IActionResult> Clientes(string search = "")
    {
        ViewBag.HideDateFilters = true;
        return View("Clientes/Clientes", await BuildClientesReportAsync("Reporte de clientes", search));
    }

    public async Task<IActionResult> CuentasPorCobrar(DateTime? from, DateTime? to, string search = "", int? customerId = null)
    {
        await LoadReportLookupsAsync(customers: true);
        return View("CuentasPorCobrar/CuentasPorCobrar", await BuildCuentasPorCobrarAsync("Cuentas por cobrar", from, to, search, customerId));
    }

    public async Task<IActionResult> CuentasPorPagar(DateTime? from, DateTime? to, string search = "", int? supplierId = null)
    {
        await LoadReportLookupsAsync(suppliers: true);
        return View("CuentasPorPagar/CuentasPorPagar", await BuildCuentasPorPagarAsync("Cuentas por pagar", from, to, search, supplierId));
    }

    public async Task<IActionResult> NotasCredito(DateTime? from, DateTime? to, string search = "", int? customerId = null)
    {
        await LoadReportLookupsAsync(customers: true);
        return View("NotasCredito/NotasCredito", await BuildNotasCreditoAsync("Reporte de notas de crédito", from, to, search, customerId));
    }

    public async Task<IActionResult> LibroVentas(DateTime? from, DateTime? to, string search = "", int? customerId = null, DocumentStatus? status = null)
    {
        await LoadReportLookupsAsync(customers: true);
        ViewBag.EnableStatusFilter = true;
        return View("LibroVentas/LibroVentas", await BuildVentasReportAsync("Libro de ventas", from, to, search, customerId, status));
    }

    public async Task<IActionResult> LibroCompras(DateTime? from, DateTime? to, string search = "", int? supplierId = null, DocumentStatus? status = null)
    {
        await LoadReportLookupsAsync(suppliers: true);
        ViewBag.EnableStatusFilter = true;
        return View("LibroCompras/LibroCompras", await BuildComprasReportAsync("Libro de compras", from, to, search, supplierId, status));
    }

    public async Task<IActionResult> FlujoCaja(DateTime? from, DateTime? to, string search = "")
        => View("FlujoCaja/FlujoCaja", await BuildFlujoCajaAsync("Flujo de caja", from, to, search));

    public async Task<FileResult> ExportPdf(string type, DateTime? from, DateTime? to, string search = "", int? customerId = null, int? supplierId = null, DocumentStatus? status = null)
    {
        var report = type switch
        {
            "ventas" => await BuildVentasReportAsync("Reporte de ventas", from, to, search, customerId, status),
            "compras" => await BuildComprasReportAsync("Reporte de compras", from, to, search, supplierId, status),
            "clientes" => await BuildClientesReportAsync("Reporte de clientes", search),
            "cxc" => await BuildCuentasPorCobrarAsync("Cuentas por cobrar", from, to, search, customerId),
            "cxp" => await BuildCuentasPorPagarAsync("Cuentas por pagar", from, to, search, supplierId),
            "notas-credito" => await BuildNotasCreditoAsync("Reporte de notas de crédito", from, to, search, customerId),
            "libro-ventas" => await BuildVentasReportAsync("Libro de ventas", from, to, search, customerId, status),
            "libro-compras" => await BuildComprasReportAsync("Libro de compras", from, to, search, supplierId, status),
            _ => await BuildFlujoCajaAsync("Flujo de caja", from, to, search)
        };

        var bytes = pdfReportService.Generate(report);
        Response.Headers.ContentDisposition = $"inline; filename={type}-{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(bytes, "application/pdf");
    }

    private async Task<ReportResultViewModel> BuildVentasReportAsync(string title, DateTime? from, DateTime? to, string search, int? customerId, DocumentStatus? status)
    {
        var query = context.Invoices.Include(x => x.Customer).AsQueryable();
        query = ApplyDates(query, from, to);
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Number.Contains(search) || x.Customer!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search, CustomerId = customerId, Status = status },
            AmountHeader = "Monto",
            Rows = (await query.OrderByDescending(x => x.Date).ToListAsync()).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Number,
                Secondary = $"{x.Customer!.Name} / Saldo {x.BalanceDue:N2}",
                Status = x.Status.GetDisplayName(),
                Amount = x.Total
            }).ToList()
        };
    }

    private async Task<ReportResultViewModel> BuildClientesReportAsync(string title, string search)
    {
        var query = context.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.TaxId.Contains(search) ||
                x.Address.Contains(search) ||
                x.Phone.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { Search = search },
            DateHeader = "Registro",
            MainHeader = "Cliente",
            SecondaryHeader = "Datos",
            StatusHeader = "Pago",
            AmountHeader = "Teléfono",
            Rows = (await query.OrderBy(x => x.Name).ToListAsync()).Select(x => new ReportRowViewModel
            {
                Date = x.CreatedAt.ToString("dd/MM/yyyy"),
                Main = x.Name,
                Secondary = $"{x.TaxId} / {x.Address}",
                Status = x.PaymentMethod.GetDisplayName(),
                AmountText = x.Phone
            }).ToList()
        };
    }

    private async Task<ReportResultViewModel> BuildComprasReportAsync(string title, DateTime? from, DateTime? to, string search, int? supplierId, DocumentStatus? status)
    {
        var query = context.Purchases.Include(x => x.Supplier).Include(x => x.Items).ThenInclude(x => x.ProductService).AsQueryable();
        query = ApplyDates(query, from, to);
        if (supplierId.HasValue) query = query.Where(x => x.SupplierId == supplierId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Number.Contains(search) || x.Supplier!.Name.Contains(search));
        }

        var rows = (await query.OrderByDescending(x => x.Date).ToListAsync()).SelectMany(x => x.Items.Select(item => new ReportRowViewModel
        {
            Date = x.Date.ToString("dd/MM/yyyy"),
            Main = x.Supplier!.Name,
            Secondary = $"{item.ProductService!.Name} / Cant. {item.Quantity:N2}",
            Status = x.Status.GetDisplayName(),
            Amount = item.Total
        })).ToList();

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search, SupplierId = supplierId, Status = status },
            AmountHeader = "Monto",
            Rows = rows
        };
    }

    private async Task<ReportResultViewModel> BuildCuentasPorCobrarAsync(string title, DateTime? from, DateTime? to, string search, int? customerId)
    {
        var invoices = context.Invoices.Include(x => x.Customer).Where(x => x.BalanceDue > 0).AsQueryable();
        invoices = ApplyDates(invoices, from, to);
        if (customerId.HasValue) invoices = invoices.Where(x => x.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            invoices = invoices.Where(x => x.Customer!.Name.Contains(search) || x.Number.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search, CustomerId = customerId },
            AmountHeader = "Monto",
            Rows = (await invoices.OrderByDescending(x => x.Date).ToListAsync()).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Customer!.Name,
                Secondary = $"Factura {x.Number} / {GetAgeRange(x.Date)}",
                Status = x.Status.GetDisplayName(),
                Amount = x.BalanceDue
            }).ToList()
        };
    }

    private async Task<ReportResultViewModel> BuildCuentasPorPagarAsync(string title, DateTime? from, DateTime? to, string search, int? supplierId)
    {
        var invoices = context.SupplierInvoices.Include(x => x.Purchase).ThenInclude(x => x!.Supplier).Where(x => x.Status != Models.DocumentStatus.Paid).AsQueryable();
        invoices = ApplyDates(invoices, from, to);
        if (supplierId.HasValue) invoices = invoices.Where(x => x.Purchase!.SupplierId == supplierId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            invoices = invoices.Where(x => x.Number.Contains(search) || x.Purchase!.Supplier!.Name.Contains(search));
        }

        return new ReportResultViewModel
        {
            Title = title,
            Company = await GetCompanyAsync(),
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search, SupplierId = supplierId },
            AmountHeader = "Monto",
            Rows = (await invoices.OrderByDescending(x => x.Date).ToListAsync()).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Purchase!.Supplier!.Name,
                Secondary = $"Factura proveedor {x.Number} / {GetAgeRange(x.Date)}",
                Status = x.Status.GetDisplayName(),
                Amount = x.Total
            }).ToList()
        };
    }

    private async Task<ReportResultViewModel> BuildNotasCreditoAsync(string title, DateTime? from, DateTime? to, string search, int? customerId)
    {
        var query = context.CreditNotes
            .Include(x => x.Invoice)
            .ThenInclude(x => x!.Customer)
            .AsQueryable();

        query = ApplyDates(query, from, to);
        if (customerId.HasValue) query = query.Where(x => x.Invoice!.CustomerId == customerId.Value);
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
            Filters = new ReportFilterViewModel { From = from, To = to, Search = search, CustomerId = customerId },
            AmountHeader = "Monto",
            Rows = (await query.OrderByDescending(x => x.Date).ToListAsync()).Select(x => new ReportRowViewModel
            {
                Date = x.Date.ToString("dd/MM/yyyy"),
                Main = x.Invoice!.Customer!.Name,
                Secondary = $"Nota {x.Number} / Factura {x.Invoice.Number}",
                Status = x.Status.GetDisplayName(),
                Amount = x.Total
            }).ToList()
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
            AmountHeader = "Monto",
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
        if (days <= 30) return "0-30 días";
        if (days <= 60) return "31-60 días";
        return "61+ días";
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

    private async Task LoadReportLookupsAsync(bool customers = false, bool suppliers = false)
    {
        if (customers)
        {
            ViewBag.Customers = await context.Customers.OrderBy(x => x.Name).ToListAsync();
        }

        if (suppliers)
        {
            ViewBag.Suppliers = await context.Suppliers.OrderBy(x => x.Name).ToListAsync();
        }
    }
}

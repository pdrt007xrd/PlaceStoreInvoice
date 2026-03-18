using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;
using Ventas.Services;
using Ventas.ViewModels;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOrOperador")]
public class VentasController(AppDbContext context, PdfReportService pdfReportService) : Controller
{
    public async Task<IActionResult> Cotizaciones()
    {
        var data = await context.Quotes.Include(x => x.Customer).OrderByDescending(x => x.Date).ToListAsync();
        return View("Cotizaciones/Cotizaciones", data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearCotizacion()
    {
        await LoadLookupsAsync();
        return View("Cotizaciones/CrearCotizacion", new Quote { Number = $"COT-{DateTime.Now:yyyyMMddHHmmss}", Status = DocumentStatus.Draft });
    }

    [HttpPost]
    public async Task<IActionResult> CrearCotizacion(Quote model)
    {
        model.Items = SanitizeItems(model.Items).Select(x => new QuoteItem
        {
            ProductServiceId = x.ProductServiceId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(model.CustomerId, model.Items.FirstOrDefault()?.ProductServiceId);
            return View("Cotizaciones/CrearCotizacion", model);
        }

        model.Total = model.Items.Sum(x => x.Total);
        context.Quotes.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Cotizaciones));
    }

    public async Task<IActionResult> Facturas()
    {
        var data = await context.Invoices.Include(x => x.Customer).OrderByDescending(x => x.Date).ToListAsync();
        return View("Facturas/Facturas", data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearFactura()
    {
        await LoadLookupsAsync();
        return View("Facturas/CrearFactura", new Invoice
        {
            Number = $"FAC-{DateTime.Now:yyyyMMddHHmmss}",
            Status = DocumentStatus.Issued,
            Date = DateTime.Now
        });
    }

    [HttpPost]
    public async Task<IActionResult> CrearFactura(Invoice model)
    {
        model.Items = SanitizeItems(model.Items).Select(x => new InvoiceItem
        {
            ProductServiceId = x.ProductServiceId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(model.CustomerId, model.Items.FirstOrDefault()?.ProductServiceId);
            return View("Facturas/CrearFactura", model);
        }

        model.Date = DateTime.Now;
        model.Total = model.Items.Sum(x => x.Total);
        model.BalanceDue = model.Total;
        context.Invoices.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(FacturaEmitida), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> FacturaEmitida(int id)
    {
        var invoice = await context.Invoices.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);
        if (invoice is null)
        {
            return NotFound();
        }

        return View("Facturas/FacturaEmitida", new InvoicePostPrintViewModel
        {
            InvoiceId = invoice.Id,
            Number = invoice.Number,
            Customer = invoice.Customer?.Name ?? string.Empty,
            Total = invoice.Total,
            BalanceDue = invoice.BalanceDue
        });
    }

    [HttpGet]
    public async Task<IActionResult> EditarFactura(int id)
    {
        var invoice = await context.Invoices
            .Include(x => x.Items)
            .Include(x => x.Receipts)
            .Include(x => x.CreditNotes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Items.Count == 0)
        {
            invoice.Items.Add(new InvoiceItem());
        }

        if (invoice.Receipts.Any() || invoice.CreditNotes.Any())
        {
            TempData["Error"] = "No se puede editar una factura que ya tiene cobros o notas de credito.";
            return RedirectToAction(nameof(Facturas));
        }

        await LoadLookupsAsync(invoice.CustomerId, invoice.Items.FirstOrDefault()?.ProductServiceId);
        return View("Facturas/EditarFactura", invoice);
    }

    [HttpPost]
    public async Task<IActionResult> EditarFactura(Invoice model)
    {
        var invoice = await context.Invoices
            .Include(x => x.Items)
            .Include(x => x.Receipts)
            .Include(x => x.CreditNotes)
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Receipts.Any() || invoice.CreditNotes.Any())
        {
            TempData["Error"] = "No se puede editar una factura que ya tiene cobros o notas de credito.";
            return RedirectToAction(nameof(Facturas));
        }

        model.Items = SanitizeItems(model.Items).Select(x => new InvoiceItem
        {
            ProductServiceId = x.ProductServiceId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(model.CustomerId, model.Items.FirstOrDefault()?.ProductServiceId);
            model.Date = invoice.Date;
            return View("Facturas/EditarFactura", model);
        }

        invoice.CustomerId = model.CustomerId;
        invoice.PaymentMethod = model.PaymentMethod;
        invoice.Status = model.Status;
        invoice.Total = model.Items.Sum(x => x.Total);
        invoice.BalanceDue = invoice.Total;

        context.InvoiceItems.RemoveRange(invoice.Items);
        invoice.Items = model.Items;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Facturas));
    }

    [HttpGet]
    public async Task<FileResult> ImprimirFactura(int id)
    {
        var invoice = await context.Invoices
            .Include(x => x.Customer)
            .Include(x => x.Items)
            .ThenInclude(x => x.ProductService)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice is null)
        {
            throw new InvalidOperationException("Factura no encontrada.");
        }

        var company = await context.Companies.OrderBy(x => x.Id).FirstOrDefaultAsync();
        var document = new InvoicePdfViewModel
        {
            Company = new CompanyHeaderViewModel
            {
                BusinessName = company?.BusinessName ?? string.Empty,
                TaxId = company?.TaxId ?? string.Empty,
                Address = company?.Address ?? string.Empty,
                Phone = company?.Phone ?? string.Empty,
                Email = company?.Email ?? string.Empty
            },
            Number = invoice.Number,
            Date = invoice.Date.ToString("dd/MM/yyyy HH:mm"),
            Customer = invoice.Customer?.Name ?? string.Empty,
            CustomerTaxId = invoice.Customer?.TaxId ?? string.Empty,
            CustomerAddress = invoice.Customer?.Address ?? string.Empty,
            PaymentMethod = invoice.PaymentMethod.ToString(),
            Status = invoice.Status.ToString(),
            Total = invoice.Total,
            Items = invoice.Items.Select(x => new InvoicePdfItemViewModel
            {
                Product = x.ProductService?.Name ?? string.Empty,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Total = x.Total
            }).ToList()
        };

        var bytes = pdfReportService.GenerateInvoice(document);
        Response.Headers.ContentDisposition = $"inline; filename=factura-{invoice.Number}.pdf";
        return File(bytes, "application/pdf");
    }

    public async Task<IActionResult> NotasCredito()
    {
        var data = await context.CreditNotes.Include(x => x.Invoice).ThenInclude(x => x!.Customer).OrderByDescending(x => x.Date).ToListAsync();
        return View("NotasCredito/NotasCredito", data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearNotaCredito()
    {
        ViewBag.Invoices = new SelectList(await context.Invoices.Include(x => x.Customer).Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
        ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        ViewBag.ProductsData = await context.ProductServices.OrderBy(x => x.Name).ToListAsync();
        return View("NotasCredito/CrearNotaCredito", new CreditNote { Number = $"NC-{DateTime.Now:yyyyMMddHHmmss}", Status = DocumentStatus.Issued });
    }

    [HttpPost]
    public async Task<IActionResult> CrearNotaCredito(CreditNote model)
    {
        model.Items = SanitizeItems(model.Items).Select(x => new CreditNoteItem
        {
            ProductServiceId = x.ProductServiceId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

        if (!ModelState.IsValid)
        {
            ViewBag.Invoices = new SelectList(await context.Invoices.Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
            ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("NotasCredito/CrearNotaCredito", model);
        }

        model.Total = model.Items.Sum(x => x.Total);
        var invoice = await context.Invoices.FirstOrDefaultAsync(x => x.Id == model.InvoiceId);
        if (invoice is null)
        {
            ModelState.AddModelError("InvoiceId", "La factura seleccionada no existe.");
            ViewBag.Invoices = new SelectList(await context.Invoices.Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
            ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.ProductsData = await context.ProductServices.OrderBy(x => x.Name).ToListAsync();
            return View("NotasCredito/CrearNotaCredito", model);
        }

        if (model.Total <= 0 || model.Total > invoice.BalanceDue)
        {
            ModelState.AddModelError("Total", "La nota de credito no puede exceder el saldo pendiente de la factura.");
            ViewBag.Invoices = new SelectList(await context.Invoices.Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
            ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.ProductsData = await context.ProductServices.OrderBy(x => x.Name).ToListAsync();
            return View("NotasCredito/CrearNotaCredito", model);
        }

        invoice.BalanceDue -= model.Total;
        invoice.Status = invoice.BalanceDue <= 0 ? DocumentStatus.Cancelled : DocumentStatus.PartiallyPaid;
        context.CreditNotes.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(NotasCredito));
    }

    private async Task LoadLookupsAsync(int? customerId = null, int? productId = null)
    {
        ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", customerId);
        ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", productId);
        ViewBag.CustomersData = await context.Customers.OrderBy(x => x.Name).ToListAsync();
        ViewBag.ProductsData = await context.ProductServices.OrderBy(x => x.Name).ToListAsync();
    }

    private static List<T> SanitizeItems<T>(IEnumerable<T>? items) where T : DocumentItem, new()
    {
        return (items ?? [])
            .Where(x => x.ProductServiceId > 0 && x.Quantity > 0 && x.UnitPrice >= 0)
            .Select(x =>
            {
                x.Total = x.Quantity * x.UnitPrice;
                return x;
            })
            .ToList();
    }
}

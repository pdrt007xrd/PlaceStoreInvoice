using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;
using Ventas.Services;
using Ventas.ViewModels;

namespace Ventas.Controllers;

[Authorize]
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
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(model.CustomerId, model.Items.FirstOrDefault()?.ProductServiceId);
            return View("Facturas/CrearFactura", model);
        }

        model.Date = DateTime.Now;
        model.Total = model.Items.Sum(x => x.Total);
        context.Invoices.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(ImprimirFactura), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditarFactura(int id)
    {
        var invoice = await context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice is null)
        {
            return NotFound();
        }

        if (invoice.Items.Count == 0)
        {
            invoice.Items.Add(new InvoiceItem());
        }

        await LoadLookupsAsync(invoice.CustomerId, invoice.Items.FirstOrDefault()?.ProductServiceId);
        return View("Facturas/EditarFactura", invoice);
    }

    [HttpPost]
    public async Task<IActionResult> EditarFactura(Invoice model)
    {
        var invoice = await context.Invoices
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (invoice is null)
        {
            return NotFound();
        }

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

        context.InvoiceItems.RemoveRange(invoice.Items);
        invoice.Items = model.Items.Select(x => new InvoiceItem
        {
            ProductServiceId = x.ProductServiceId,
            Quantity = x.Quantity,
            UnitPrice = x.UnitPrice,
            Total = x.Total
        }).ToList();

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
        ViewBag.Invoices = new SelectList(await context.Invoices.Include(x => x.Customer).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
        ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        return View("NotasCredito/CrearNotaCredito", new CreditNote { Number = $"NC-{DateTime.Now:yyyyMMddHHmmss}", Status = DocumentStatus.Issued });
    }

    [HttpPost]
    public async Task<IActionResult> CrearNotaCredito(CreditNote model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Invoices = new SelectList(await context.Invoices.OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
            ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("NotasCredito/CrearNotaCredito", model);
        }

        model.Total = model.Items.Sum(x => x.Total);
        context.CreditNotes.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(NotasCredito));
    }

    private async Task LoadLookupsAsync(int? customerId = null, int? productId = null)
    {
        ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", customerId);
        ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", productId);
    }
}

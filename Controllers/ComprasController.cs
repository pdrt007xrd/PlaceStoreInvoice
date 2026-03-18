using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOrOperador")]
public class ComprasController(AppDbContext context) : Controller
{
    public async Task<IActionResult> RegistroCompras()
    {
        var data = await context.Purchases.Include(x => x.Supplier).OrderByDescending(x => x.Date).ToListAsync();
        return View("RegistroCompras/RegistroCompras", data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearRegistroCompra()
    {
        await LoadLookupsAsync();
        return View("RegistroCompras/CrearRegistroCompra", new Purchase { Number = $"COM-{DateTime.Now:yyyyMMddHHmmss}", Status = DocumentStatus.Paid });
    }

    [HttpPost]
    public async Task<IActionResult> CrearRegistroCompra(Purchase model)
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View("RegistroCompras/CrearRegistroCompra", model);
        }

        model.Total = model.Items.Sum(x => x.Total);
        context.Purchases.Add(model);

        foreach (var item in model.Items)
        {
            var product = await context.ProductServices.FindAsync(item.ProductServiceId);
            if (product is not null)
            {
                product.SalePrice = item.UnitCost + item.FixedProfitAmount;
            }
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(RegistroCompras));
    }

    public async Task<IActionResult> FacturasProveedor()
    {
        var data = await context.SupplierInvoices.Include(x => x.Purchase).ThenInclude(x => x!.Supplier).OrderByDescending(x => x.Date).ToListAsync();
        return View("FacturasProveedor/FacturasProveedor", data);
    }

    [HttpGet]
    public async Task<IActionResult> CrearFacturaProveedor()
    {
        ViewBag.Purchases = new SelectList(await context.Purchases.Include(x => x.Supplier).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
        return View("FacturasProveedor/CrearFacturaProveedor", new SupplierInvoice { Number = $"FP-{DateTime.Now:yyyyMMddHHmmss}", Status = DocumentStatus.Paid });
    }

    [HttpPost]
    public async Task<IActionResult> CrearFacturaProveedor(SupplierInvoice model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Purchases = new SelectList(await context.Purchases.OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number");
            return View("FacturasProveedor/CrearFacturaProveedor", model);
        }

        var purchase = await context.Purchases.FindAsync(model.PurchaseId);
        model.Total = purchase?.Total ?? 0;
        context.SupplierInvoices.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(FacturasProveedor));
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Suppliers = new SelectList(await context.Suppliers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        ViewBag.Products = new SelectList(await context.ProductServices.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
    }
}

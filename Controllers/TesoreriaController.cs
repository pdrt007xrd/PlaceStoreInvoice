using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOrOperador")]
public class TesoreriaController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Cobros()
        => View("Cobros/Cobros", await context.Receipts.Include(x => x.Customer).Include(x => x.Invoice).OrderByDescending(x => x.Date).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> CrearCobro(int? invoiceId = null)
    {
        ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        ViewBag.Invoices = new SelectList(await context.Invoices.Include(x => x.Customer).Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number", invoiceId);

        var model = new Receipt();
        if (invoiceId.HasValue)
        {
            var invoice = await context.Invoices.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == invoiceId.Value);
            if (invoice is not null)
            {
                model.InvoiceId = invoice.Id;
                model.CustomerId = invoice.CustomerId;
                model.Amount = invoice.BalanceDue;
                ViewBag.SelectedInvoiceCustomer = invoice.Customer?.Name;
                ViewBag.SelectedInvoiceBalance = invoice.BalanceDue;
            }
        }

        return View("Cobros/CrearCobro", model);
    }

    [HttpPost]
    public async Task<IActionResult> CrearCobro(Receipt model)
    {
        var invoice = model.InvoiceId.HasValue
            ? await context.Invoices.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == model.InvoiceId.Value)
            : null;

        if (invoice is null)
        {
            ModelState.AddModelError("InvoiceId", "Debe seleccionar una factura pendiente.");
        }
        else
        {
            model.CustomerId = invoice.CustomerId;
            if (model.Amount <= 0 || model.Amount > invoice.BalanceDue)
            {
                ModelState.AddModelError("Amount", "El monto debe ser mayor que cero y no puede exceder el saldo pendiente.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            ViewBag.Invoices = new SelectList(await context.Invoices.Include(x => x.Customer).Where(x => x.BalanceDue > 0).OrderByDescending(x => x.Date).ToListAsync(), "Id", "Number", model.InvoiceId);
            ViewBag.SelectedInvoiceCustomer = invoice?.Customer?.Name;
            ViewBag.SelectedInvoiceBalance = invoice?.BalanceDue ?? 0;
            return View("Cobros/CrearCobro", model);
        }

        context.Receipts.Add(model);
        invoice!.BalanceDue -= model.Amount;
        invoice.Status = invoice.BalanceDue <= 0 ? DocumentStatus.Paid : DocumentStatus.PartiallyPaid;
        await RegisterMovementAsync("Ingreso por cobro", model.Amount);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Cobros));
    }

    public async Task<IActionResult> PagosProveedores()
        => View("PagosProveedores/PagosProveedores", await context.SupplierPayments.Include(x => x.Supplier).OrderByDescending(x => x.Date).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> CrearPagoProveedor()
    {
        ViewBag.Suppliers = new SelectList(await context.Suppliers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        return View("PagosProveedores/CrearPagoProveedor", new SupplierPayment());
    }

    [HttpPost]
    public async Task<IActionResult> CrearPagoProveedor(SupplierPayment model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = new SelectList(await context.Suppliers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("PagosProveedores/CrearPagoProveedor", model);
        }

        context.SupplierPayments.Add(model);
        await RegisterMovementAsync("Egreso por pago a proveedor", -model.Amount);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(PagosProveedores));
    }

    public async Task<IActionResult> CajaBancos()
    {
        var accounts = await context.CashBankAccounts.Include(x => x.Movements).ToListAsync();
        return View("CajaBancos/CajaBancos", accounts);
    }

    [HttpGet]
    public IActionResult CrearMovimientoCajaBanco()
    {
        ViewBag.Accounts = new SelectList(context.CashBankAccounts.OrderBy(x => x.Name).ToList(), "Id", "Name");
        return View("CajaBancos/CrearMovimientoCajaBanco", new CashBankMovement());
    }

    [HttpPost]
    public async Task<IActionResult> CrearMovimientoCajaBanco(CashBankMovement model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Accounts = new SelectList(await context.CashBankAccounts.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("CajaBancos/CrearMovimientoCajaBanco", model);
        }

        context.CashBankMovements.Add(model);
        var account = await context.CashBankAccounts.FindAsync(model.CashBankAccountId);
        if (account is not null)
        {
            account.Balance += model.Amount;
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(CajaBancos));
    }

    private async Task RegisterMovementAsync(string description, decimal amount)
    {
        var account = await context.CashBankAccounts.OrderBy(x => x.Id).FirstAsync();
        account.Balance += amount;
        context.CashBankMovements.Add(new CashBankMovement
        {
            CashBankAccountId = account.Id,
            Description = description,
            Amount = amount
        });
    }
}

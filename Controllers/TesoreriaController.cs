using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize]
public class TesoreriaController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Cobros()
        => View("Cobros/Cobros", await context.Receipts.Include(x => x.Customer).OrderByDescending(x => x.Date).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> CrearCobro()
    {
        ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        return View("Cobros/CrearCobro", new Receipt());
    }

    [HttpPost]
    public async Task<IActionResult> CrearCobro(Receipt model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Customers = new SelectList(await context.Customers.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("Cobros/CrearCobro", model);
        }

        context.Receipts.Add(model);
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

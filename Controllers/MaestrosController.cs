using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOrOperador")]
public class MaestrosController(AppDbContext context, ILogger<MaestrosController> logger) : Controller
{
    public async Task<IActionResult> Clientes() => View("Clientes/Clientes", await context.Customers.OrderBy(x => x.Name).ToListAsync());

    [HttpGet]
    public IActionResult CrearCliente() => View("Clientes/CrearCliente", new Customer());

    [HttpPost]
    public async Task<IActionResult> CrearCliente(Customer model)
    {
        if (!ModelState.IsValid) return View("Clientes/CrearCliente", model);
        context.Customers.Add(model);
        await context.SaveChangesAsync();
        logger.LogInformation("Cliente creado correctamente. Id={CustomerId}, Nombre={CustomerName}", model.Id, model.Name);
        TempData["ToastMessage"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Clientes));
    }

    [HttpGet]
    public async Task<IActionResult> EditarCliente(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View("Clientes/EditarCliente", customer);
    }

    [HttpPost]
    public async Task<IActionResult> EditarCliente(Customer model)
    {
        if (!ModelState.IsValid)
        {
            return View("Clientes/EditarCliente", model);
        }

        var customer = await context.Customers.FindAsync(model.Id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.Name = model.Name;
        customer.TaxId = model.TaxId;
        customer.Address = model.Address;
        customer.Phone = model.Phone;
        customer.PaymentMethod = model.PaymentMethod;

        await context.SaveChangesAsync();
        logger.LogInformation("Cliente actualizado correctamente. Id={CustomerId}, Nombre={CustomerName}", customer.Id, customer.Name);
        TempData["ToastMessage"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Clientes));
    }

    public async Task<IActionResult> Proveedores() => View("Proveedores/Proveedores", await context.Suppliers.OrderBy(x => x.Name).ToListAsync());

    [HttpGet]
    public IActionResult CrearProveedor() => View("Proveedores/CrearProveedor", new Supplier());

    [HttpPost]
    public async Task<IActionResult> CrearProveedor(Supplier model)
    {
        if (!ModelState.IsValid) return View("Proveedores/CrearProveedor", model);
        context.Suppliers.Add(model);
        await context.SaveChangesAsync();
        logger.LogInformation("Proveedor creado correctamente. Id={SupplierId}, Nombre={SupplierName}", model.Id, model.Name);
        TempData["ToastMessage"] = "Proveedor creado correctamente.";
        return RedirectToAction(nameof(Proveedores));
    }

    public async Task<IActionResult> ProductosServicios() => View("ProductosServicios/ProductosServicios", await context.ProductServices.OrderBy(x => x.Name).ToListAsync());

    [HttpGet]
    public IActionResult CrearProductoServicio() => View("ProductosServicios/CrearProductoServicio", new ProductService());

    [HttpPost]
    public async Task<IActionResult> CrearProductoServicio(ProductService model)
    {
        if (!ModelState.IsValid) return View("ProductosServicios/CrearProductoServicio", model);
        context.ProductServices.Add(model);
        await context.SaveChangesAsync();
        logger.LogInformation("Producto o servicio creado correctamente. Id={ProductServiceId}, Nombre={ProductServiceName}", model.Id, model.Name);
        TempData["ToastMessage"] = "Producto o servicio creado correctamente.";
        return RedirectToAction(nameof(ProductosServicios));
    }
}

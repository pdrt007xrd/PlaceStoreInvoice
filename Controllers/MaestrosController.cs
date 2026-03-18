using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize]
public class MaestrosController(AppDbContext context) : Controller
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
        return RedirectToAction(nameof(ProductosServicios));
    }
}

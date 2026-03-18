using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ventas.Data;
using Ventas.Models;

namespace Ventas.Controllers;

[Authorize(Policy = "AdminOnly")]
public class ConfiguracionController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Empresa()
    {
        var company = await context.Companies.OrderBy(x => x.Id).FirstAsync();
        return View("Empresa/Empresa", company);
    }

    [HttpPost]
    public async Task<IActionResult> Empresa(CompanySettings model)
    {
        if (!ModelState.IsValid) return View("Empresa/Empresa", model);

        context.Companies.Update(model);
        await context.SaveChangesAsync();
        ViewBag.Message = "Datos de empresa actualizados.";
        return View("Empresa/Empresa", model);
    }

    public async Task<IActionResult> UsuariosRoles()
    {
        var users = await context.Users.Include(x => x.Role).OrderBy(x => x.FullName).ToListAsync();
        return View("UsuariosRoles/UsuariosRoles", users);
    }

    [HttpGet]
    public async Task<IActionResult> CrearUsuario()
    {
        ViewBag.Roles = new SelectList(await context.Roles.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
        return View("UsuariosRoles/CrearUsuario", new User());
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario(User model, string plainPassword)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(plainPassword))
        {
            if (string.IsNullOrWhiteSpace(plainPassword))
            {
                ModelState.AddModelError("plainPassword", "La clave es obligatoria.");
            }

            ViewBag.Roles = new SelectList(await context.Roles.OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
            return View("UsuariosRoles/CrearUsuario", model);
        }

        model.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        context.Users.Add(model);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(UsuariosRoles));
    }
}

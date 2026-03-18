using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Ventas.Models;

namespace Ventas.Data;

public class DbSeeder(AppDbContext context)
{
    public async Task SeedAsync()
    {
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Name = "Administrador" },
                new Role { Name = "Operador" });
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(x => x.Name == "Administrador");
            context.Users.Add(new User
            {
                FullName = "Administrador General",
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                RoleId = adminRole.Id,
                IsActive = true
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Companies.AnyAsync())
        {
            context.Companies.Add(new CompanySettings
            {
                BusinessName = "Place Store",
                TaxId = "000000000",
                Address = "Santo Domingo",
                Phone = "8090000000",
                Email = "info@placestore.local",
                DefaultCurrency = "DOP"
            });
            await context.SaveChangesAsync();
        }

        if (!await context.CashBankAccounts.AnyAsync())
        {
            context.CashBankAccounts.AddRange(
                new CashBankAccount { Name = "Caja General", AccountType = AccountType.Cash, Balance = 0 },
                new CashBankAccount { Name = "Banco Principal", AccountType = AccountType.Bank, Balance = 0 });
            await context.SaveChangesAsync();
        }
    }
}

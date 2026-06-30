using Microsoft.AspNetCore.Identity;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Data
{
    // Classe responsável por criar dados iniciais na aplicação.
    public static class SeedData
    {
        // Cria os papéis e os utilizadores base quando a aplicação arranca.
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Define os papéis que a aplicação vai usar.
            string[] roles = { "Admin", "Escalão A", "Escalão B", "Sem escalão", "Funcionário" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Cria o utilizador administrador padrão.
            var adminEmail = "admin@epge.pt";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nome = "Administrador",
                    EmailConfirmed = true,
                    Escalao = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, "Adm");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Cria o utilizador funcionário padrão.
            var funcEmail = "funcionario@epge.pt";
            var funcUser = await userManager.FindByEmailAsync(funcEmail);

            if (funcUser == null)
            {
                funcUser = new ApplicationUser
                {
                    UserName = funcEmail,
                    Email = funcEmail,
                    Nome = "Funcionário Cantina",
                    EmailConfirmed = true,
                    Escalao = "Funcionário"
                };

                var result = await userManager.CreateAsync(funcUser, "Func");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(funcUser, "Funcionário");
                }
            }
        }
    }
}

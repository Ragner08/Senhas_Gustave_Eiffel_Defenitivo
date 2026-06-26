using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Data;
using Senhas_Gustave_Eiffel.Models;

// Ponto de entrada principal da aplicação.
// Este ficheiro prepara os serviços essenciais, como MVC, autenticação e base de dados.
var builder = WebApplication.CreateBuilder(args);

// Adiciona os controladores e as views ao sistema MVC.
builder.Services.AddControllersWithViews();

// Configura o contexto da base de dados com SQL Server.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura o sistema de identidade para gerir utilizadores, login e papéis.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configura a cookie de autenticação para controlar o login e os acessos.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

var app = builder.Build();

// Configura o pipeline da aplicação e o tratamento de erros em ambiente de produção.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ativa as funcionalidades de HTTPS, ficheiros estáticos e rotas.
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Define a rota inicial da aplicação para abrir diretamente a página de login.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Garante que os papéis e os utilizadores base existam quando a base de dados estiver disponível.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.CanConnect())
        {
            await SeedData.Initialize(services);
        }
    }
    catch
    {
        // Se a base de dados ainda não existir, a aplicação fica pronta para as migrações.
    }
}

app.Run();

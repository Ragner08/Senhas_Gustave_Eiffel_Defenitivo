using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Senhas_Gustave_Eiffel.Models;

namespace Senhas_Gustave_Eiffel.Data
{
    // Contexto principal da base de dados.
    // Aqui são definidas as tabelas e as relações entre os dados da aplicação.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tabela das refeições definidas para cada dia.
        public DbSet<Meal> Meals { get; set; }

        // Tabela das marcações feitas pelos utilizadores.
        public DbSet<Booking> Bookings { get; set; }

        // Tabela das transações da carteira.
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        // Tabela dos alimentos disponíveis para criar os menus.
        public DbSet<FoodItem> FoodItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define a relação entre uma marcação e o utilizador que a fez.
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define a relação entre uma transação da carteira e o respetivo utilizador.
            builder.Entity<WalletTransaction>()
                .HasOne(wt => wt.User)
                .WithMany()
                .HasForeignKey(wt => wt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Garante que o mesmo utilizador não tenha duas marcações no mesmo dia.
            builder.Entity<Booking>()
                .HasIndex(b => new { b.UserId, b.DataMarcacao })
                .IsUnique();

            // Garante que não existam duas refeições para a mesma data.
            builder.Entity<Meal>()
                .HasIndex(m => m.Data)
                .IsUnique();
        }
    }
}

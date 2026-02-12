using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Senhas_Gustave_Eiffel.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Display(Name = "Saldo da Carteira")]
        [DataType(DataType.Currency)]
        public decimal WalletBalance { get; set; } = 0;

        [Display(Name = "Escalão")]
        public string Escalao { get; set; } = "Sem escalão";

        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

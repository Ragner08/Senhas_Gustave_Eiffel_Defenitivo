using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senhas_Gustave_Eiffel.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Data da Marcação")]
        [DataType(DataType.Date)]
        public DateTime DataMarcacao { get; set; }

        [Display(Name = "Escalão Utilizado")]
        public string EscalaoUtilizado { get; set; } = string.Empty;

        [Display(Name = "Valor Pago")]
        [DataType(DataType.Currency)]
        public decimal ValorPago { get; set; }

        [Display(Name = "Data da Marcação (Registo)")]
        public DateTime DataRegisto { get; set; } = DateTime.Now;

        [Display(Name = "Confirmado")]
        public bool Confirmado { get; set; } = true;
    }
}

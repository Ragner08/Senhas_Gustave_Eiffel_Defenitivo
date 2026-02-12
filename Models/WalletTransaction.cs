using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senhas_Gustave_Eiffel.Models
{
    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Valor")]
        [DataType(DataType.Currency)]
        public decimal Valor { get; set; }

        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "Carregamento"; // Carregamento ou Pagamento

        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Display(Name = "Data da Transação")]
        public DateTime DataTransacao { get; set; } = DateTime.Now;
    }
}

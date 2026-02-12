using System.ComponentModel.DataAnnotations;

namespace Senhas_Gustave_Eiffel.Models
{
    public class WalletViewModel
    {
        [Required(ErrorMessage = "O valor é obrigatório")]
        [Range(0.01, 1000, ErrorMessage = "O valor deve estar entre 0.01€ e 1000€")]
        [DataType(DataType.Currency)]
        [Display(Name = "Valor a Adicionar (€)")]
        public decimal Valor { get; set; }
    }

    public class WalletDetailsViewModel
    {
        public decimal SaldoAtual { get; set; }
        public List<WalletTransaction> Transacoes { get; set; } = new List<WalletTransaction>();
    }

    public class AdminUserWalletViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal SaldoAtual { get; set; }
        public List<WalletTransaction> Transacoes { get; set; } = new List<WalletTransaction>();
    }
}

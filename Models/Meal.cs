using System.ComponentModel.DataAnnotations;

namespace Senhas_Gustave_Eiffel.Models
{
    public class Meal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Display(Name = "Sopa")]
        public string? Sopa { get; set; }

        [Display(Name = "Prato Principal")]
        public string? PratoPrincipal { get; set; }

        [Display(Name = "Vegetariano")]
        public string? Vegetariano { get; set; }

        [Display(Name = "Sobremesa")]
        public string? Sobremesa { get; set; }

        [Display(Name = "Criado por")]
        public string? CriadoPor { get; set; }

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Preços por escalão
        [Display(Name = "Preço Escalão A")]
        [DataType(DataType.Currency)]
        public decimal PrecoEscalaoA { get; set; } = 2.00m;

        [Display(Name = "Preço Escalão B")]
        [DataType(DataType.Currency)]
        public decimal PrecoEscalaoB { get; set; } = 3.00m;

        [Display(Name = "Preço Sem Escalão")]
        [DataType(DataType.Currency)]
        public decimal PrecoSemEscalao { get; set; } = 4.00m;
    }
}

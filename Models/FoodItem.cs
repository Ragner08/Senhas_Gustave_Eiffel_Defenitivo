using System.ComponentModel.DataAnnotations;

namespace Senhas_Gustave_Eiffel.Models
{
    public class FoodItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da comida é obrigatório")]
        [Display(Name = "Nome da Comida")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = string.Empty;
    }
}

namespace Senhas_Gustave_Eiffel.Models
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public List<DayViewModel> Days { get; set; } = new List<DayViewModel>();
        public Meal? MealDoDia { get; set; }
        public bool IsFuncionario { get; set; }
        public string UserEscalao { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; }
    }

    public class DayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasBooking { get; set; }
        public bool HasMeal { get; set; }
        public bool IsPast { get; set; }
        public int? BookingId { get; set; }
    }
}

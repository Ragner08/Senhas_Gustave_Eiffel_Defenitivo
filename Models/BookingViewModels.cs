namespace Senhas_Gustave_Eiffel.Models
{
    public class DailyReportViewModel
    {
        public DateTime Data { get; set; }
        public Meal? Meal { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public int TotalEscalaoA { get; set; }
        public int TotalEscalaoB { get; set; }
        public int TotalSemEscalao { get; set; }
        public decimal TotalValor { get; set; }
    }
}

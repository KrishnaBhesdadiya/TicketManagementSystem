namespace BackendExam.Models
{
    public class TicketCreateDTO
    {
        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Priority { get; set; } = null!;
    }
}

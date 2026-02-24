namespace BackendExam.Models
{
    public class CreateTicketDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Priority { get; set; } = "MEDIUM";
    }
    public class AssignTicketDTO
    {
        public int UserId { get; set; } 
    }
    public class UpdateStatusDTO
    {
        public string Status { get; set; } = null!;
    }
}

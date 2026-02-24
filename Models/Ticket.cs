using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendExam.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public int CreatedBy { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public virtual User? AssignedToNavigation { get; set; }
    [JsonIgnore]
    public virtual User? CreatedByNavigation { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
    [JsonIgnore]
    public virtual ICollection<TicketStatusLog> TicketStatusLogs { get; set; } = new List<TicketStatusLog>();
}

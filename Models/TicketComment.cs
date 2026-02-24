using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendExam.Models;

public partial class TicketComment
{
    public int TicketComId { get; set; }

    public int? TicketId { get; set; }

    public int? UserId { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }
    [JsonIgnore]
    public virtual Ticket? Ticket { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; }
}

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendExam.Models;

public partial class TicketStatusLog
{
    public int TicketStatusLogId { get; set; }

    public int? TicketId { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime? ChangedAt { get; set; }
    [JsonIgnore]
    public virtual User? ChangedByNavigation { get; set; }
    [JsonIgnore]
    public virtual Ticket? Ticket { get; set; }
}

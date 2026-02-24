using System.Security.Claims;
using BackendExam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendExam.Controllers
{
    [Route("tickets")]
    [ApiController]
    public class TicketAPIController : ControllerBase
    {
        #region Configuration Fields 
        private readonly BackendExamContext _context;
        private readonly IConfiguration _configuration;
        public TicketAPIController(BackendExamContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion

        #region GetAll  
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || role == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.AssignedToNavigation);

            if (role == "MANAGER")
            {
                // Manager sees all tickets
            }
            else if (role == "SUPPORT")
            {
                query = query.Where(t => t.AssignedTo == userId);
            }
            else if (role == "USER")
            {
                query = query.Where(t => t.CreatedBy == userId);
            }
            else
            {
                return Forbid();
            }

            var tickets = await query
                .Select(t => new
                {
                    t.TicketId,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    CreatedBy = t.CreatedByNavigation.UserName,
                    AssignedTo = t.AssignedToNavigation != null
                        ? t.AssignedToNavigation.UserName
                        : null,
                    t.CreatedAt
                })
                .ToListAsync();

            return Ok(tickets);
            //var tickets = await _context.Tickets
            //    .Include(u => u.CreatedByNavigation)
            //    .Include(u => u.AssignedToNavigation)
            //    .ToListAsync();
            //return Ok(tickets);
        }
        #endregion

        #region CreateTicket
        [Authorize(Roles = "MANAGER,USER")]
        [HttpPost]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDTO model)
        {
            // 🔍 Validate Priority
            var validPriorities = new[] { "LOW", "MEDIUM", "HIGH" };

            if (!validPriorities.Contains(model.Priority.ToUpper()))
                return BadRequest(new { message = "Invalid priority value." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var ticket = new Ticket
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority.ToUpper(),
                Status = "OPEN",
                CreatedBy = userId,
                AssignedTo = null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket created successfully.",
                ticketId = ticket.TicketId,
                title = ticket.Title,
                status = ticket.Status,
                priority = ticket.Priority
            });
        }
        #endregion

        #region Delete
        [Authorize(Roles="MANAGER")]
        [HttpDelete]
        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Assign Ticket
        [Authorize(Roles = "MANAGER,SUPPORT")]
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketDTO model)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var supportUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (supportUser == null || supportUser.Role.RoleName != "SUPPORT")
                return BadRequest(new { message = "Assigned user must be SUPPORT role." });

            ticket.AssignedTo = model.UserId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket assigned successfully.",
                ticketId = ticket.TicketId,
                assignedTo = supportUser.UserName
            });
        }
        #endregion

        #region Update Ticket Status
        [Authorize(Roles = "MANAGER,SUPPORT")]
        [HttpPatch("tickets/{id}/status")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] UpdateStatusDTO model)
        {
            var validStatuses = new[] { "OPEN", "IN_PROGRESS", "RESOLVED", "CLOSED" };

            if (!validStatuses.Contains(model.Status.ToUpper()))
                return BadRequest(new { message = "Invalid status value." });

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            string oldStatus = ticket.Status;
            string newStatus = model.Status.ToUpper();

            if (oldStatus == newStatus)
                return BadRequest(new { message = "Status is already the same." });

            // Update ticket
            ticket.Status = newStatus;

            // Insert into status logs
            var statusLog = new TicketStatusLog
            {
                TicketId = ticket.TicketId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow
            };

            _context.TicketStatusLogs.Add(statusLog);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Ticket status updated successfully.",
                ticketId = ticket.TicketId,
                oldStatus,
                newStatus
            });
        }
        #endregion
    }
}

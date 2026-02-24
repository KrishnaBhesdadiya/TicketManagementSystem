using System.Security.Claims;
using BackendExam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendExam.Controllers
{
    [Route("")]
    [ApiController]
    public class CommentsAPIController : ControllerBase
    {
        #region Configuration Fields 
        private readonly BackendExamContext _context;
        private readonly IConfiguration _configuration;
        public CommentsAPIController(BackendExamContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion

        #region HelperMethod
        private bool CanAccessTicket(Ticket ticket, string role, int userId)
        {
            if (role == "MANAGER")
                return true;

            if (role == "SUPPORT" && ticket.AssignedTo == userId)
                return true;

            if (role == "USER" && ticket.CreatedBy == userId)
                return true;

            return false;
        }
        #endregion

        #region CreateComment
        [Authorize]
        [HttpPost("tickets/{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDTO model)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!CanAccessTicket(ticket, role!, userId))
                return Forbid();

            var comment = new TicketComment
            {
                TicketId = id,
                UserId = userId,
                Comment = model.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment added successfully.",
                commentId = comment.TicketComId
            });
        }
        #endregion

        #region GetComments
        [Authorize]
        [HttpGet("tickets/{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!CanAccessTicket(ticket, role!, userId))
                return Forbid();

            var comments = await _context.TicketComments
                .Where(c => c.TicketId == id)
                .Select(c => new
                {
                    c.TicketComId,
                    c.Comment,
                    c.CreatedAt,
                    UserName = c.User.UserName
                })
                .ToListAsync();

            return Ok(comments);
        }
        #endregion

        #region EditComment
        [Authorize]
        [HttpPatch("comments/{id}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDTO model)
        {
            var comment = await _context.TicketComments.FindAsync(id);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (role != "MANAGER" && comment.UserId != userId)
                return Forbid();

            comment.Comment = model.Comment;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment updated successfully." });
        }
        #endregion

        #region DeleteComment
        [Authorize]
        [HttpDelete("comments/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.TicketComments.FindAsync(id);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (role != "MANAGER" && comment.UserId != userId)
                return Forbid();

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully." });
        }
        #endregion
    }
}

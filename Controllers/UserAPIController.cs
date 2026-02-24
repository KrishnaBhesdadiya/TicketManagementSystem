using System.Security.Claims;
using BackendExam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendExam.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        #region Configuration Fields 
        private readonly BackendExamContext _context;
        private readonly IConfiguration _configuration;
        public UserAPIController(BackendExamContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion

        #region GetAll  
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "MANAGER")
            {
                return StatusCode(403, new { message = "Only MANAGER can access user list." });
            }
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return Ok(users);
        }
        #endregion

        #region CreateUser
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InsertCustomer([FromBody] CreateUserDTO model)
        {
            var checkrole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (checkrole != "MANAGER")
            {
                return StatusCode(403, new { message = "Only MANAGER can access user list." });
            }
            var existinguser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (existinguser != null)
                return BadRequest(new { message = "Email already exists." });

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToUpper() == model.Role.ToUpper());

            if (role == null)
                return BadRequest(new { message = "Invalid role." });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Password = hashedPassword,
                RoleId = role.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully.",
                userId = user.UserId,
                userName = user.UserName,
                email = user.Email,
                role = role.RoleName
            });
        }
        #endregion
    }
}

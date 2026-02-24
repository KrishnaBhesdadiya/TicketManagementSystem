using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendExam.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendExam.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        #region Configuration Fields 
        private readonly BackendExamContext _context;
        private readonly IConfiguration _configuration;
        public AuthAPIController(BackendExamContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        #endregion

        #region Generate JWT token
        // 🔑 Generate Token with Role & Expiry from appsettings.json
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            var expiryMinutes = Convert.ToDouble(jwtSettings["TokenExpiryMinutes"]);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region LoginAPI
        // ✅ LOGIN API
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUser)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginUser.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid Credentials." });

            // 🔐 Verify bcrypt password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                loginUser.Password,
                user.Password
            );

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                userId = user.UserId,  
                userName = user.UserName,
                email = user.Email,
                role = user.Role.RoleName
            });
        }
        #endregion
    }
}

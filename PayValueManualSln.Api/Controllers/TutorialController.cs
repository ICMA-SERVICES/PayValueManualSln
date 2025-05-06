using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Mvc;
using PayValueManualSln.Application.DTOs.Account;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Application.DTOs.Tutorial;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace PayValueManualSln.Api.Controllers
{
    public class TutorialController : Controller
    {
        private readonly List<UserCredential> _users;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _configuration;
        public TutorialController(List<UserCredential> users,JwtService jwtService,IConfiguration config)
        {
            _users = users;
            _jwtService = jwtService;
            _configuration = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        { var users = _configuration.GetSection("Users").Get<List<UserCredential>>();
        var user = users.FirstOrDefault(u =>
            u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
            && u.Password == request.Password);

        if (user == null)
            return Unauthorized("Invalid credentials");

            // Generate JWT
            var now = DateTime.UtcNow;
        var jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, "User")
            }),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
            NotBefore = now,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
        }

    }
}

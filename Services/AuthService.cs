using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UrlShortener.Data;
using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync (RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return null;
            var user =new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = GenerateToken(user),
                Username = user.Username
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return new AuthResponseDto
            {
                Token = GenerateToken(user),
                Username = user.Username
            };
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpiryHours"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.Username)
            };

            var token = new JwtSecurityToken
            (
                issuer : _config["Jwt:Issuer"],
                audience : _config["Jwt:Audience"],
                claims: claims,
                expires:expiry,
                signingCredentials : creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

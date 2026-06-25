using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using AbsensiApi.Data;
using AbsensiApi.Models;

namespace AbsensiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
public async Task<IActionResult> Register(User user)
{
    // 1. Simpan User
    user.Password = HashPassword(user.Password);
    _context.Users.Add(user);
    await _context.SaveChangesAsync(); // Sekarang user.Id sudah tersedia

    // 2. OTOMATIS buat Student profile yang terhubung ke User tersebut
    var newStudent = new Student
    {
        Nama = user.NamaLengkap,
        NomorInduk = "AUTO-" + user.Id, // Contoh generate nomor induk
        UserId = user.Id // Menghubungkan secara otomatis!
    };
    
    _context.Students.Add(newStudent);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Registrasi berhasil dan profil siswa dibuat!" });
}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginDto)
        {
            var hashedPassword = HashPassword(loginDto.Password);
            
            // 1. Validasi User
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.Password == hashedPassword);

            if (user == null)
            {
                return Unauthorized(new { message = "Username atau password salah!" });
            }

            // 2. Cari StudentId yang terkait dengan User ini
            // Pastikan di Model Student ada property 'UserId'
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
            
            // Jika user belum memiliki profil siswa, beri nilai 0
            int studentId = student?.Id ?? 0;

            // 3. Proses JWT Token
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("NamaLengkap", user.NamaLengkap),
                    // Mengirimkan StudentId ke dalam Token
                    new Claim("StudentId", studentId.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { 
                message = "Login sukses!", 
                token = tokenString,
                nama = user.NamaLengkap,
                studentId = studentId // Mengirimkan ID agar Flutter bisa langsung tahu
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}

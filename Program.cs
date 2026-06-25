using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization; 
using AbsensiApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Konfigurasi CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://absensi-web-kappa.vercel.app")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. Daftarkan Koneksi Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Konfigurasi Autentikasi JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,         //  Diubah jadi false agar tidak pilih-pilih server
        ValidateAudience = false,       //  Diubah jadi false agar Flutter & Vercel bebas masuk
        ValidateLifetime = true,        //  Tetap periksa masa kedaluwarsa token
        ValidateIssuerSigningKey = true, //  Tetap wajib mencocokkan kunci rahasia
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// MATIKAN BARIS INI: Beri tanda komentar agar tidak merusak routing di Railway
// app.UseHttpsRedirection();

// PERBAIKAN DI SINI: Menambahkan routing agar sistem CORS mengenali endpoint dengan benar
app.UseRouting();

app.UseCors("AllowAll");

// 5. Jalankan Autentikasi & Otorisasi
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();

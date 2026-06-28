# Absensi API - Backend

Ini adalah server backend berbasis C# .NET yang berfungsi sebagai penyedia layanan API untuk sistem absensi siswa di SMKN 11 Malang. Server ini mengelola seluruh data pengguna, data siswa, dan riwayat kehadiran yang tersimpan di database Supabase.

## Teknologi Utama
- C# / .NET
- Supabase Database
- JWT Authentication
- Railway Deployment

## Fitur Backend
- Registrasi akun pengguna baru dan otomatis terhubung ke tabel siswa.
- Autentikasi login yang menghasilkan token keamanan JWT.
- Endpoint pencatatan absensi masuk dan pulang siswa secara realtime.

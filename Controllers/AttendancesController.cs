using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AbsensiApi.Data;
using AbsensiApi.Models;

namespace AbsensiApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AttendancesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendancesController(AppDbContext context)
        {
            _context = context;
        }

        //  READ: Mengambil semua absensi beserta data siswanya
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendances()
        {
            return await _context.Attendances
                .Include(a => a.Student) // Wajib untuk memunculkan data nama siswa
                .OrderByDescending(a => a.Tanggal)
                .ToListAsync();
        }

        // CREATE / SMART UPDATE: Menambah absensi baru atau update jam pulang otomatis
        [HttpPost]
        public async Task<ActionResult<Attendance>> PostAttendance(Attendance attendance)
        {
            // Standardisasi format Tanggal ke UTC
            DateTime checkDate = attendance.Tanggal == DateTime.MinValue ? DateTime.UtcNow : attendance.Tanggal.ToUniversalTime();

            // 1. Validasi keberadaan siswa
            var studentExists = await _context.Students.AnyAsync(s => s.Id == attendance.StudentId);
            if (!studentExists)
            {
                return BadRequest(new { message = $"Siswa dengan ID {attendance.StudentId} tidak ditemukan!" });
            }

            // 2. Cek apakah siswa SUDAH punya data absensi hari ini
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == attendance.StudentId && 
                                          a.Tanggal.Year == checkDate.Year &&
                                          a.Tanggal.Month == checkDate.Month &&
                                          a.Tanggal.Day == checkDate.Day);

            if (existingAttendance != null)
            {
                // ✨ JIKA KLIK ABSEN PULANG: Jangan bikin data baru, update saja data yang ada!
                if (attendance.Status == "Pulang")
                {
                    existingAttendance.Status = "Pulang";
                    existingAttendance.JamPulang = DateTime.UtcNow; // Catat jam pulang sekarang

                    await _context.SaveChangesAsync();
                    
                    // Load data nama siswa agar langsung ter-update di React
                    await _context.Entry(existingAttendance).Reference(a => a.Student).LoadAsync();

                    return Ok(new { message = "Absen Pulang berhasil dicatat!", data = existingAttendance });
                }

                // JIKA DOUBLE ABSEN MASUK: Cegah duplikasi data hadir di hari yang sama
                if (attendance.Status == "Hadir")
                {
                    return BadRequest(new { message = "Siswa sudah melakukan absen masuk hari ini!" });
                }
            }

            // 3. JIKA BELUM ADA DATA HARI INI (Absen Masuk Baru)
            attendance.Tanggal = checkDate;

            if (attendance.Status == "Hadir" && attendance.JamMasuk == null)
            {
                attendance.JamMasuk = DateTime.UtcNow;
            }
            else if (attendance.Status == "Pulang" && attendance.JamPulang == null)
            {
                // Antisipasi jika siswa langsung klik pulang tanpa absen masuk terlebih dahulu
                attendance.JamPulang = DateTime.UtcNow;
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Load data relasi Student
            await _context.Entry(attendance).Reference(a => a.Student).LoadAsync();

            return Ok(new { message = "Absensi berhasil dicatat!", data = attendance });
        }

        // EDIT/UPDATE: Mengubah data absensi secara manual dari Web Admin
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAttendance(int id, Attendance attendance)
        {
            if (id != attendance.Id)
            {
                return BadRequest(new { message = "ID tidak cocok!" });
            }

            var existingAttendance = await _context.Attendances.FindAsync(id);
            if (existingAttendance == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            // Update field dasar
            existingAttendance.Status = attendance.Status;
            existingAttendance.Tanggal = attendance.Tanggal.ToUniversalTime();

            // Update jam jika admin mengeditnya secara manual
            if (attendance.JamMasuk != null) existingAttendance.JamMasuk = attendance.JamMasuk?.ToUniversalTime();
            if (attendance.JamPulang != null) existingAttendance.JamPulang = attendance.JamPulang?.ToUniversalTime();

            // Logika Otomatis jika admin mengubah status ke Pulang tapi jam kosong
            if (existingAttendance.Status == "Pulang" && existingAttendance.JamPulang == null)
            {
                existingAttendance.JamPulang = DateTime.UtcNow;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Attendances.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Data tidak ditemukan." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Data berhasil diperbarui!" });
        }

        // DELETE: Menghapus catatan absensi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
            {
                return NotFound(new { message = "Data tidak ditemukan." });
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus!" });
        }
    }
}
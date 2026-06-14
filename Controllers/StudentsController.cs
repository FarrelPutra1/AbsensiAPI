using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AbsensiApi.Data;
using AbsensiApi.Models;

namespace AbsensiApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Alamat API akan menjadi: api/students
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Hubungkan dengan DbContext yang sudah kita buat sebelumnya
        public StudentsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. READ: Mengambil semua data siswa
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            // Sesuai kriteria penguji, untuk READ disarankan raw SQL / langsung dari table
            return await _context.Students.ToListAsync();
        }

        // 2. READ: Mengambil satu data siswa berdasarkan ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound(new { message = "Siswa tidak ditemukan" });
            }

            return student;
        }

        // 3. CREATE: Menambah siswa baru
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        // 4. UPDATE: Mengubah data siswa
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest(new { message = "ID tidak cocok" });
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Students.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Siswa tidak ditemukan" });
                }
                throw;
            }

            return Ok(new { message = "Data siswa berhasil diperbarui" });
        }

        // 5. DELETE: Menghapus data siswa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound(new { message = "Siswa tidak ditemukan" });
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Siswa berhasil dihapus" });
        }
    }
}
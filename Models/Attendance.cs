using System.ComponentModel.DataAnnotations.Schema;

namespace AbsensiApi.Models
{
    [Table("attendances")]
    public class Attendance
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("tanggal")]
        public DateTime Tanggal { get; set; }

        [Column("status")]
        public string Status { get; set; } = string.Empty;

        [Column("jam_masuk")]
        public DateTime? JamMasuk { get; set; }

        [Column("jam_pulang")]
        public DateTime? JamPulang { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
    }
}
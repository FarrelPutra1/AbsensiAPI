using System.ComponentModel.DataAnnotations.Schema;

namespace AbsensiApi.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("nama_lengkap")]
        public string NamaLengkap { get; set; } = string.Empty;
    }
}
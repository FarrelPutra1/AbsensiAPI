using System.ComponentModel.DataAnnotations.Schema; 
using System.ComponentModel.DataAnnotations;      

[Table("students")]
public class Student
{
    [Column("id")]
    public int Id { get; set; }

    [Column("nomor_induk")]
    public string NomorInduk { get; set; } = string.Empty;

    [Column("nama")]
    public string Nama { get; set; } = string.Empty;

    [Column("user_id")]
    public int UserId { get; set; } 
}
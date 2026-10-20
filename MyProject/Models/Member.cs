using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    [Table("User")]
    public class Member
    {
        [Key]
        [Column("User_ID")]// 指定學號為 Primary Key (主鍵)
        [MaxLength(20)]

        public string StudentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string Email { get; set; }
        [MaxLength(50)]
        public string Department { get; set; } // 系級
        [Required]
        [Column("Password_Hash")]
        [MaxLength(50)]

        public string Password { get; set; }

        
        

    }
}

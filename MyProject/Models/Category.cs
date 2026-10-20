using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        [Column("Category_ID")]
        [MaxLength(50)] // 🛡️ 對應資料庫的 VARCHAR(50)
        public string CategoryId { get; set; }

        [Required]
        [Column("Category_Name")]
        [MaxLength(100)] // 🛡️ 對應資料庫的 NVARCHAR(100)
        public string CategoryName { get; set; }
    }
}
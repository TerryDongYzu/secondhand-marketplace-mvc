using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    // 🌟 1. 修正資料表名稱，對準 SSMS 裡的 Comment 表
    [Table("Comment")]
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 告訴 C# 這個會由資料庫自動產生，不要傳 NULL 過去檢查
        [Column("Comment_ID")]
        public int CommentId { get; set; } // 留言編號

        [Required]
        [Column("Content")] // 🌟 2. 補上資料行名牌
        public string Content { get; set; } // 留言內容

        [Column("Timestamp")] // 🌟 2. 補上資料行名牌
        public DateTime Timestamp { get; set; } = DateTime.Now; // 留言時間

        [Column("Item_ID")]
        [MaxLength(50)]
        public string ItemId { get; set; } // 外鍵：關聯到物品

        [Column("User_ID")]
        [MaxLength(50)] // 🌟 3. 配合資料庫，將長度限制放寬到 50
        public string UserId { get; set; } // 外鍵：關聯到留言者 (Member的學號)

        // ==========================================
        // 關聯屬性 (維持原樣即可，EF Core 會自動處理)
        // ==========================================
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        [ForeignKey("UserId")]
        public virtual Member User { get; set; }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    [Table("Item")]
    public class Item
    {
        [Key]
        [Column("Item_ID")]
        [MaxLength(50)]
        public string ItemId { get; set; }

        [Required]
        [Column("Title")] // 🌟 補上明確的名牌
        [MaxLength(200)]
        public string Title { get; set; } // 物品名稱

        [Column("Description")] // 🌟 補上明確的名牌
        public string Description { get; set; } // 詳細描述

        [Column("Item_Condition")]
        [MaxLength(50)]
        public string ItemCondition { get; set; } // 物品新舊程度

        [Column("Price", TypeName = "decimal(10, 2)")] // 🌟 解決小數點精度警告
        public decimal Price { get; set; } // 刊登金額

        [Column("Status")] // 🌟 補上明確的名牌
        [MaxLength(50)]
        public string Status { get; set; } // 物品狀態 (0:可購買, 1:已成交)

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // 刊登日期

        [Column("Owner_ID")]
        [MaxLength(50)]
        public string OwnerId { get; set; } // 刊登者學號

        // 🚨 這是最關鍵的一行！告訴 EF Core：「這個欄位只是網頁暫用的，資料庫裡沒有它，別去抓！」
        [NotMapped]
        public string BuyerId { get; set; }// 記錄是誰買走這個物品的

        [Column("Category_ID")]
        [MaxLength(50)]
        public string CategoryId { get; set; } // 外鍵：關聯至分類表

        // ==========================================
        // Entity Framework 關聯屬性 (Navigation Property)
        // ==========================================
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // 🌟 順手補上賣家(Member)的關聯，以後抓資料更方便
        [ForeignKey("OwnerId")]
        public virtual Member? Owner { get; set; }
    }
}
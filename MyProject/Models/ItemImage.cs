using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore; // 🌟 補上這個，為了支援複合主鍵的標籤

namespace MyProject.Models
{
    [Table("Item_Images")]
    // 🌟 告訴大腦：這張表的主鍵是由這兩個欄位合體組成的！
    [PrimaryKey(nameof(ItemId), nameof(ImageUrl))]
    public class ItemImage
    {
        [Column("Item_ID")]
        [MaxLength(50)]
        public string ItemId { get; set; } // 外鍵：這是哪項物品的圖片

        [Column("Image_URL")]
        [MaxLength(255)] // 🌟 配合資料庫的瘦身計畫，從 2083 改成 255
        public string ImageUrl { get; set; } // 圖片的存放路徑

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
    }
}
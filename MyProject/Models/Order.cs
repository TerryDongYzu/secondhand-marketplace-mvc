using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    // 🌟 1. 告訴大腦，這個類別現在對應到期末報告的 Transaction 表格
    [Table("Transaction")]
    public class Order
    {
        // 🌟 2. 修正型別為 string，並對應 Order_ID
        [Key]
        [Column("Order_ID")]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // 自動產生唯一序號

        [Required]
        [Column("Item_ID")]
        public string ItemId { get; set; } // 買了哪個物品

        [Required]
        [Column("Buyer_ID")]
        public string BuyerId { get; set; } // 買家學號

        // 🌟 3. 補上資料庫有的 Seller_ID 與狀態欄位
        [Column("Seller_ID")]
        public string SellerId { get; set; }

        [Column("Transaction_Status")]
        public string TransactionStatus { get; set; } = "處理中";

        [Required(ErrorMessage = "請填寫希望的面交地點")]
        [MaxLength(200)]
        [Column("Meet_Location")]
        public string MeetLocation { get; set; } // 面交地點

        // 🌟 4. 配合資料庫改成 DateTime 型別
        [Required(ErrorMessage = "請填寫希望的面交時間")]
        [Column("Meet_Time")]
        public DateTime? MeetTime { get; set; } // 面交時間

        // ==========================================
        // 🛡️ 這些是前端表單有，但資料庫(為了正規化)沒有的欄位
        // 加上 [NotMapped] 就不會報錯了！
        // ==========================================

        [Required(ErrorMessage = "請填寫收件人真實姓名")]
        [Column("Buyer_Name")]
        public string BuyerName { get; set; }

     
        [Required(ErrorMessage = "請填寫聯絡資訊")]
        [Column("Contact_Info")]
        public string ContactInfo { get; set; }

        [Column("Order_Date")]
        public DateTime OrderDate { get; set; } = DateTime.Now; // 如果舊程式有用到，保留但不存入DB
        [Column("Reject_Reason")]
        public string? RejectReason { get; set; } // 允許為空，因為接受的訂單沒有拒絕理由

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }
    }
}
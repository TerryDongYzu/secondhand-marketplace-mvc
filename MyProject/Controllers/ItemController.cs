using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Models;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using X.PagedList.Extensions;

namespace MyProject.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly CmsContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ItemController(CmsContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        [AllowAnonymous] // 允許未登入者逛大廳
        public IActionResult Index(string searchString, int? page)
        {
            ViewBag.CurrentFilter = searchString;

          
            var items = _context.Items.Include(i => i.Category).AsQueryable();

          
            if (!String.IsNullOrEmpty(searchString))
            {
                items = items.Where(s => s.Title != null && s.Title.Contains(searchString));
            }

            items = items.OrderByDescending(i => i.CreatedAt);

            //設定分頁
            int pageSize = 4;
            int pageNumber = (page ?? 1);

            // 先把這一頁要顯示的物品切出來
            var pagedItems = items.ToPagedList(pageNumber, pageSize);

            var itemIdsOnThisPage = pagedItems.Select(i => i.ItemId).ToList();

            var coverImages = new Dictionary<string, string>();

            foreach (var id in itemIdsOnThisPage)
            {            
                var firstImage = _context.ItemImages.FirstOrDefault(img => img.ItemId == id);
                if (firstImage != null)
                {
                    coverImages[id] = firstImage.ImageUrl;
                }
            }

            ViewBag.CoverImages = coverImages;
            return View(pagedItems);
        }

        // 物品詳情 (Details)
        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (item == null) return NotFound();

            ViewBag.ItemImages = await _context.ItemImages
                .Where(img => img.ItemId == id)
                .ToListAsync();

            ViewBag.Comments = await _context.Comments
                .Where(c => c.ItemId == id)
                .OrderByDescending(c => c.Timestamp)
                .ToListAsync();

            return View(item);
        }

        // 3. 上架新物品
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ItemCondition,Price,CategoryId")] Item item, List<IFormFile> files)
        {
            item.ItemId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            item.Status = "0";
            item.CreatedAt = DateTime.Now;
            item.OwnerId = User.Identity.IsAuthenticated ? User.Identity.Name : "admin";
            item.BuyerId = "";

            ModelState.Remove("ItemId");
            ModelState.Remove("Status");
            ModelState.Remove("OwnerId");
            ModelState.Remove("Category");
            ModelState.Remove("BuyerId");

            if (ModelState.IsValid)
            {

                
                _context.Add(item);
                await _context.SaveChangesAsync();

                if (files != null && files.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var itemImage = new ItemImage
                            {
                                ItemId = item.ItemId,
                                ImageUrl = "/uploads/" + uniqueFileName
                            };
                            _context.ItemImages.Add(itemImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
            return View(item);
        }


        // 顯示編輯畫面
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (item == null) return NotFound();

            if (item.Status == "1")
            {
                TempData["ErrorMessage"] = "此物品已經售出，無法再進行修改！";
                return RedirectToAction(nameof(MyItems));
            }

            // 不是物品主人或 admin 沒資格編輯
            if (item.OwnerId != User.Identity.Name && User.Identity.Name != "admin")
            {
                return NotFound();
            }

            ViewBag.CurrentImages = await _context.ItemImages
                .Where(img => img.ItemId == id)
                .ToListAsync();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ItemId,Title,Description,ItemCondition,Price,CategoryId,Status,CreatedAt,OwnerId")] Item item, List<IFormFile> files, string[] deletedImages)
        {
            if (id != item.ItemId) return NotFound();

            var currentItemInDb = _context.Items.AsNoTracking().FirstOrDefault(i => i.ItemId == id);
            if (currentItemInDb == null) return NotFound();
            if (currentItemInDb.OwnerId != User.Identity.Name && User.Identity.Name != "admin") return NotFound();

            // 強制移除不需要驗證的導覽屬性
            ModelState.Remove("Category");
            ModelState.Remove("OwnerId");
            ModelState.Remove("ItemImages");
            ModelState.Remove("Orders");
            ModelState.Remove("BuyerId");


            // 偵錯核心一：如果驗證失敗，直接抓出哪一個欄位在搞鬼！
            if (!ModelState.IsValid)
            {
                var failedFields = ModelState.Where(x => x.Value.Errors.Count > 0).Select(x => x.Key).ToList();
                // 把錯誤原因塞進 ViewBag 傳給畫面看
                ViewBag.DebugMessage = "❌ 系統拒絕更新！驗證失敗的欄位有：" + string.Join(", ", failedFields);

                // 重新準備需要的圖片與分類
                ViewBag.CurrentImages = await _context.ItemImages.Where(img => img.ItemId == id).ToListAsync();
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
                return View(item);
            }

            try
            {
                item.OwnerId = currentItemInDb.OwnerId;
                item.BuyerId = currentItemInDb.BuyerId ?? "";

                _context.Update(item);
                await _context.SaveChangesAsync();


                // 記錄到底有沒有成功收到前端傳來的刪除名單       
                int deletedCount = 0;
                if (deletedImages != null && deletedImages.Length > 0)
                {
                    foreach (var urlToDelete in deletedImages)
                    {
                        var imgToDelete = await _context.ItemImages
                            .FirstOrDefaultAsync(img => img.ItemId == id && img.ImageUrl == urlToDelete);

                        if (imgToDelete != null)
                        {
                            var imagePath = imgToDelete.ImageUrl.TrimStart('/');
                            var physicalPath = Path.Combine(_hostEnvironment.WebRootPath, imagePath);
                            if (System.IO.File.Exists(physicalPath))
                            {
                                System.IO.File.Delete(physicalPath);
                            }
                            _context.ItemImages.Remove(imgToDelete);
                            deletedCount++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // 處理上傳新圖片
                int uploadedCount = 0;
                if (files != null && files.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(fileStream);
                            }

                            var itemImage = new ItemImage
                            {
                                ItemId = item.ItemId,
                                ImageUrl = "/uploads/" + uniqueFileName
                            };
                            _context.ItemImages.Add(itemImage);
                            uploadedCount++;
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // 成功時彈出資訊
                TempData["SuccessMessage"] = $"🎉 更新成功！刪除了 {deletedCount} 張圖，新增了 {uploadedCount} 張圖。";
                return RedirectToAction(nameof(MyItems));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(item.ItemId)) return NotFound();
                else throw;
            }
        }

       
        // 刪除物品 (Delete)
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var item = await _context.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (item == null) return NotFound();

            // 已售出的物品禁止進入刪除確認畫面
            if (item.Status == "1")
            {
                TempData["ErrorMessage"] = "此物品已售出，必須保留作為雙方的交易憑證，無法刪除！";
                return RedirectToAction(nameof(MyItems));
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                // 要刪除的最後一刻再次檢查狀態
                if (item.Status == "1")
                {
                    TempData["ErrorMessage"] = "警告：無法刪除已成交的訂單！";
                    return RedirectToAction(nameof(MyItems));
                }

                bool isAdmin = User.Identity.Name == "admin";
                if (!isAdmin && item.OwnerId != User.Identity.Name) return NotFound();
 
                var itemImages = await _context.ItemImages.Where(img => img.ItemId == id).ToListAsync();
                foreach (var img in itemImages)
                {
                    var imagePath = img.ImageUrl.TrimStart('/');
                    var physicalPath = Path.Combine(_hostEnvironment.WebRootPath, imagePath);
                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ItemExists(string id)
        {
            return _context.Items.Any(e => e.ItemId == id);
        }

        // 新增留言 (AddComment)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(string itemId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction(nameof(Details), new { id = itemId });

            var comment = new Comment
            {
                ItemId = itemId,
                Content = content,
                UserId = User.Identity.Name,
                Timestamp = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = itemId });
        }


        // 賣家後台：我的上架物品
        public async Task<IActionResult> MyItems()
        {
            var currentUserId = User.Identity.Name;
            var myItems = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.OwnerId == currentUserId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(myItems);
        }

        // 狀態切換：標記為已成交
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSold(string id)
        {
            var item = await _context.Items.FindAsync(id);
            bool isAdmin = User.Identity.Name == "admin";

            if (item == null || (!isAdmin && item.OwnerId != User.Identity.Name)) return NotFound();

            item.Status = "1";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyItems));
        }

        // 進入填寫面交資料頁面
        public async Task<IActionResult> Buy(string id)
        {
            if (id == null) return NotFound();
            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemId == id);
            if (item == null) return NotFound();

            var currentUserId = User.Identity.Name;

            if (item.OwnerId == currentUserId)
            {
                TempData["ErrorMessage"] = "不能購買自己上架的物品喔！";
                return RedirectToAction("Details", new { id = id });
            }
            if (item.Status != "0")
            {
                TempData["ErrorMessage"] = "該物品已被其他人買走囉！";
                return RedirectToAction(nameof(Index));
            }

            // 自動預抓目前登入買家的學號與姓名
            var currentMember = await _context.Members.FirstOrDefaultAsync(m => m.StudentId == currentUserId);

            // 建立一份預填好的訂單結構傳給畫面
            var order = new Order
            {
                ItemId = item.ItemId,
                BuyerId = currentUserId,
                BuyerName = currentMember?.Name ?? "", // 自動帶入姓名，免去重複填寫
                SellerId = item.OwnerId // 🌟【修復 1】補上這一行！把這件物品的主人學號（賣家）傳給前端 Hidden 欄位！
            };

            ViewBag.ItemTitle = item.Title;
            ViewBag.ItemPrice = item.Price;

            return View(order);
        }

        // 提交面交資料並成立訂單
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 🌟【修復 2】在 [Bind(...)] 的最後面加上 ,SellerId，允許 C# 接收這個欄位！
        public async Task<IActionResult> Buy([Bind("ItemId,BuyerId,BuyerName,ContactInfo,MeetLocation,MeetTime,SellerId")] Order order)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.ItemId == order.ItemId);
            if (item == null) return NotFound();

            if (item.Status != "0")
            {
                TempData["ErrorMessage"] = "手速太慢！該物品已被買走。";
                return RedirectToAction(nameof(Index));
            }

            // 🌟【修復 3】雙重保險防線：後端在檢查前，直接強制把剛查到的物品主人學號再次寫入
            order.SellerId = item.OwnerId;

            ModelState.Remove("Item");   // 排除導覽屬性的驗證
            ModelState.Remove("Seller"); // 🌟【修復 4】如果你的 Order Model 裡面有 public Member Seller 這樣的關聯，也要一併排除，免得它抱怨外鍵關聯物件不能為 Null

            if (ModelState.IsValid)
            {
                order.OrderDate = DateTime.Now;
                _context.Add(order);

                // 更新物品狀態與買家學號
                item.Status = "1"; // 標記已成交
                item.BuyerId = order.BuyerId;
                _context.Update(item);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"🎉 成功下單購買「{item.Title}」！面交資訊已送達賣家。";
                return RedirectToAction(nameof(MyOrders));
            }

            ViewBag.ItemTitle = item.Title;
            ViewBag.ItemPrice = item.Price;
            return View(order);
        }

        // 你的訂單 - 讀取訂單詳細面交資訊
        // 你的訂單 - 讀取訂單詳細面交資訊
        public async Task<IActionResult> MyOrders()
        {
            var currentUserId = User.Identity.Name;

            // 🌟 1. 交易紀錄 (進行中/歷史緩衝區)：不論是買或賣，只要是 處理中/已接受/已拒絕/已取消 都放在這
            var ongoingOrders = await _context.Orders
                .Include(o => o.Item)
                .Where(o => (o.BuyerId == currentUserId || o.SellerId == currentUserId)
                            && o.TransactionStatus != "已完成")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 🌟 2. 我買進的物品：限定自己是買家，且狀態必須是「已完成」
            var myPurchases = await _context.Orders
                .Include(o => o.Item)
                .Where(o => o.BuyerId == currentUserId && o.TransactionStatus == "已完成")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 🌟 3. 我賣出的物品：限定自己是賣家，且狀態必須是「已完成」
            var mySales = await _context.Orders
                .Include(o => o.Item)
                .Where(o => o.SellerId == currentUserId && o.TransactionStatus == "已完成")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // 4. 蒐集這批訂單裡相關的會員資料做字典 (維持你原本的漂亮設計)
            var allSellerIds = ongoingOrders.Select(o => o.SellerId)
                                .Concat(myPurchases.Select(o => o.SellerId)).Distinct().ToList();
            var sellers = await _context.Members
                .Where(m => allSellerIds.Contains(m.StudentId))
                .ToDictionaryAsync(m => m.StudentId, m => m);

            ViewBag.OngoingOrders = ongoingOrders;
            ViewBag.MyPurchases = myPurchases;
            ViewBag.MySales = mySales;
            ViewBag.Sellers = sellers;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(string orderId)
        {
            var order = await _context.Orders.Include(o => o.Item).FirstOrDefaultAsync(o => o.Id == orderId);

            // 安全檢查：只有買家本人有權利按下確認結案
            if (order != null && order.BuyerId == User.Identity.Name)
            {
                order.TransactionStatus = "已完成";

                if (order.Item != null)
                {
                    order.Item.Status = "2"; // 🌟 標記為完全完售，徹底從賣場下架
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🎉 交易完成！這筆紀錄已移至「我買進的物品」。";
            }
            return RedirectToAction(nameof(MyOrders));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(string orderId, string cancelReason)
        {
            var order = await _context.Orders.Include(o => o.Item).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order != null)
            {
                order.TransactionStatus = "已取消";
                order.RejectReason = $"[取消原因] {cancelReason}"; // 借用這個欄位紀錄取消原因

                if (order.Item != null)
                {
                    order.Item.Status = "0";      // 🌟 核心魔術：商品狀態回歸「上架中」，重新開放購買！
                    order.Item.BuyerId = null;     // 清空被綁定的買家
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "❌ 交易已取消，該物品已重新回到賣場上架。";
            }
            return RedirectToAction(nameof(MyOrders));
        }
        // 賣家按下「接受訂單」
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder(string orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.TransactionStatus = "已接受"; // 狀態改為已接受
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "已成功接受訂單！";
            }
            return RedirectToAction(nameof(MyOrders));
        }

        // 賣家按下「拒絕訂單」
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOrder(string orderId, string rejectReason)
        {
            var order = await _context.Orders.Include(o => o.Item).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.TransactionStatus = "已拒絕"; // 狀態改為已拒絕
                order.RejectReason = rejectReason;  // 記錄拒絕理由

                // 🌟 關鍵：訂單被拒絕了，所以要把物品狀態改回 "0" (上架中)，讓其他人可以買
                if (order.Item != null)
                {
                    order.Item.Status = "0";
                    order.Item.BuyerId = null;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "已婉拒該筆訂單，物品已重新開放購買。";
            }
            return RedirectToAction(nameof(MyOrders));
        }
    }
}
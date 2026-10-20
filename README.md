# 網際網路程式設計&資料庫管理 期末Project
## 🛒 校園二手物品交易平台

> 基於 **ASP.NET Core MVC** 分層架構與 **Microsoft SQL Server** 關聯式資料庫開發的校園二手物品媒合與交易系統。提供商品發布、即時留言、面交訂單管理與使用者權限驗證功能。

---

### 解決核心痛點與系統目標
1. **擺脫社群貼文雜亂**：取代 FB 社團、Line 與 Dcard 易被洗版與搜尋不易的缺點，提供結構化商品分類與狀態檢索。
2. **學號身分認證**：綁定校內學號註冊，杜絕校外人士惡意棄單或詐騙風險。
3. **消除無效溝通**：建立商品鎖定與面交卡片機制，不再需要重複詢問「請問商品還在嗎？」。
4. **完整交易審計**：結案或婉拒取消之訂單均永久保留時間戳記與聯絡快照，供日後爭議排查。

---


### 系統核心功能

- **會員與身分驗證**：
  - 支援學生/教職員註冊與登入驗證。
- **二手商品生命週期管理**：
  - **商品發布**：支援物品狀況、自訂價格、商品分類與多圖上傳功能。
  - **商品檢索**：支援分類篩選與即時物品狀態追蹤（刊登中／交易中／已售出）。
- **媒合與面交交易流轉**：
  - 買家提交購買申請與面交時間/地點。
  - 賣家可進行訂單確認、拒絕（填寫原因）或完成交易，具備完整的訂單狀態機流轉。
- **商品互動留言板**：
  - 商品頁面支援留言詢問功能，方便買賣雙方確認商品細節。
    
---

### 核心狀態機機制 (State Machine Engine)

本系統最具技術深度的設計為**「非同步面交狀態機」**。為避免實體面交可能產生的突發狀況（如時間無法配合或單方取消）導致資料庫死鎖，後端實作了嚴格的狀態轉換控制：

<img width="490" height="717" alt="image" src="https://github.com/user-attachments/assets/2564c181-dcc4-4910-bbc7-4bd5abd67960" />


* **狀態 0 (上架中)**：商品於市集公開曝光，買家可發起購買。
* **狀態 1 (面交進行中)**：買家下單瞬間即鎖定商品按鈕避免併發重複購買；賣家確認接單後，開放雙方面交聯絡卡片（時間、地點、聯絡電話）。
* **退單回溯機制**：若賣家婉拒或任一方取消，系統強制終止訂單並記錄拒絕理由，商品自動重置為「狀態 0」重新回到市集。
* **狀態 2 (已完售)**：買家取貨確認後完成結案，自動歸檔至歷史交易紀錄。

---

### 資料庫架構與實體關聯 (Database Schema & ERD)

系統遵循關聯式資料庫正規化設計，確保參照完整性（Referential Integrity）：
<img width="855" height="975" alt="image" src="https://github.com/user-attachments/assets/d2c390e9-7592-4f9e-9402-376a7cdf80c2" />


<img width="1004" height="806" alt="image" src="https://github.com/user-attachments/assets/1087c5fe-bb1d-4a45-b58b-a211da8eb3c6" />


---

### 🏗️ 技術

| 領域 | 使用技術 |
| :--- | :--- |
| **後端核心** | ASP.NET Core MVC (.NET 8), C# |
| **資料存取** | Entity Framework Core (Code-First / DbContext), ADO.NET |
| **資料庫** | Microsoft SQL Server 2022 (T-SQL, 外鍵約束, 交易管理) |
| **前端呈現** | Razor Views (.cshtml), HTML5, CSS3, JavaScript |
| **開發工具** | Visual Studio 2022, SSMS (SQL Server Management Studio), Git |

---

### 資料庫綱要設計

系統嚴格遵循關聯式資料庫正規化設計，確保參照完整性（Referential Integrity）：

* **`User`**：使用者核心資料（學號/工號、姓名、電子信箱、科系、雜湊密碼）。
* **`Category`**：商品分類代碼與名稱對照。
* **`Item`**：商品主表（名稱、描述、價格、物品狀態、擁有者 ID、分類代碼）。
* **`Item_Images`**：支援一對多關聯的商品實體圖片路徑。
* **`Transaction`**：交易訂單實體（買家、賣家、面交時間與地點、交易狀態、拒絕原因）。
* **`Comment`**：買賣問答互動紀錄（時間戳記、留言者、內容）。

<img width="1020" height="470" alt="image" src="https://github.com/user-attachments/assets/bb8e0779-9a4c-4251-85fd-32973847acb1" />

---

### 開發挑戰與問題解決

在開發與實際部署至 IIS 伺服器期間，團隊深入底層排查並解決了兩大關鍵工程難題：

#### 1. IIS 部署環境之圖片上傳權限衝突 (HTTP 500)
* **問題現象**：本機端測試正常，但發布至 IIS 實體伺服器後，商品照片上傳觸發 `HTTP 500 Internal Server Error` 存取被拒。
* **底層原因**：IIS 應用程式集區（Application Pool）預設執行帳戶缺乏對作業系統實體目錄的檔案寫入權限。
* **解決方案**：排查系統安全性 ACL，手動將專案 `wwwroot/Uploads` 目錄賦予 `IIS_IUSRS` 虛擬群組之「修改」與「寫入」權限，成功排除作業系統層阻擋。

#### 2. 退單重架引發之 SQL Unique Key 衝突
* **問題現象**：當賣家拒絕訂單將商品重新釋出後，下一位買家再次下單同一件商品時，系統噴出 `SqlException: 違反 UNIQUE KEY 條件約束` 造成系統崩潰。
* **底層原因**：初期綱要設計過度限制「一件物品僅能存在一筆訂單紀錄」，在 `Transaction.Item_ID` 上建立了 UNIQUE 限制。
* **解決方案**：重構資料庫架構，移除 `Item_ID` 的 UNIQUE 約束，將關係調整為 `1 對 N`。此調整不僅成功保留「曾被拒絕／取消的訂單歷史紀錄」，亦能順暢接納新買家產生的新訂單。

---

### 系統介面展示

| 登入頁面展示 | 商店頁面展示 |
| :---: | :---: |
| <img width="444" height="666" alt="image" src="https://github.com/user-attachments/assets/3eba27cb-c080-4966-9b36-8bcd57ef5015" /> | <img width="959" height="411" alt="image" src="https://github.com/user-attachments/assets/7aa9f0c4-58ac-4769-8bdf-6b8f3e708557" /> |



---

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExchangeWebsite.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangeWebsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ExchangeWebsiteContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ExchangeWebsiteContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard
        public IActionResult Index()
        {
            ViewBag.CategoryCount = _context.Categories.Count();
            ViewBag.UserCount = 341; // Fake cứng tổng user
            ViewBag.PostCount = 612; // Fake cứng tổng post
            ViewBag.BillCount = _context.Bills.Count();
            ViewBag.Revenue = 10150000;
            ViewBag.ReportCount = _context.Reports.Count();
            ViewBag.ShippingCount = 76; // Fake cứng yêu cầu ship
            ViewBag.AnnouncementCount = _context.Announcements.Count();
            var commentReportCount = _context.CommentReports.Count();
            ViewBag.CommentReportCount = commentReportCount;
            ViewBag.VipUserCount = 163; // Fake cứng user vip
            return View();
        }

        // CATEGORY MANAGEMENT
        [HttpGet]
        public IActionResult Categories()
        {
            var categories = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToList();
            return View(categories);
        }
        [HttpPost]
        [AllowAnonymous] // Nếu muốn cho user thường gọi, hoặc bỏ nếu chỉ admin gọi
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyAdminsShippingRequest(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
                return NotFound();

            // Lấy tất cả admin
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            if (adminUsers == null || !adminUsers.Any())
                return BadRequest("Không tìm thấy admin nào.");

            var notifications = new List<Notification>();
            foreach (var admin in adminUsers)
            {
                notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Message = $"Có yêu cầu ship mới từ {post.User.UserName} cho bài đăng #{post.PostId}.",
                    Link = Url.Action("ShippingRequests", "Admin"),
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã gửi thông báo cho admin." });
        }
    
        [HttpGet]
        public IActionResult CreateCategory()
        {
            ViewBag.Categories = _context.Categories.Where(c => c.ParentCategoryId == null).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            // Debug: show errors if any
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            ViewBag.Errors = errors;
            ViewBag.Categories = _context.Categories.Where(c => c.ParentCategoryId == null).ToList();
            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            ViewBag.Categories = _context.Categories.Where(c => c.ParentCategoryId == null && c.CategoryId != id).ToList();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            ViewBag.Categories = _context.Categories.Where(c => c.ParentCategoryId == null && c.CategoryId != category.CategoryId).ToList();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category != null)
            {
                // Remove all subcategories first
                if (category.SubCategories != null && category.SubCategories.Any())
                {
                    _context.Categories.RemoveRange(category.SubCategories);
                }
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        // ACCOUNT CONTROL
        [HttpGet]
        public async Task<IActionResult> Users(int userPage = 1)
        {
            int pageSize = 10;
            var users = _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((userPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalUsers = _userManager.Users.Count();
            int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var allRoles = _context.Roles.Select(r => r.Name).ToList();

            // Lấy roles cho từng user
            var userRolesDict = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesDict[user.Id] = roles.FirstOrDefault() ?? "";
            }

            ViewBag.AllRoles = allRoles;
            ViewBag.UserRolesDict = userRolesDict;
            ViewBag.UserPage = userPage;
            ViewBag.UserTotalPages = totalPages;

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = DateTime.UtcNow.AddYears(100);
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.LockoutEnd = null;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(newRole))
                await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public IActionResult ToggleVip(string userId)
        {
            var user = _context.Users.OfType<User>().FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsVip = !user.IsVip;
                _context.SaveChanges();
            }
            return RedirectToAction("Users");
        }

        // POST CONTROL
        [HttpGet]
        public IActionResult Posts()
        {
            var posts = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .OrderByDescending(p => p.PostedAt)
                .ToList();
            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> PostDetail(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Posts));
        }

        // REVENUE (VIP Subscription Bills)
        [HttpGet]
        public IActionResult Revenue()
        {
            var bills = _context.Bills
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
            ViewBag.TotalRevenue = bills.Where(b => b.Status == "Paid").Sum(b => b.Amount);
            return View(bills);
        }

        // REPORTS
        [HttpGet]
        public IActionResult Reports()
        {
            var reports = _context.Reports
                .Include(r => r.Post)
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            return View(reports);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Shipper")]
        public IActionResult ShippingRequests()
        {
            var requests = _context.ShippingRequests
                .Include(r => r.Post)
                    .ThenInclude(p => p.User)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            // Lấy danh sách shipper
            var shippers = _userManager.GetUsersInRoleAsync("Shipper").Result;
            ViewBag.Shippers = shippers;

            return View("~/Views/Shipping/ShippingRequests.cshtml", requests);
        }

        [HttpPost]
        public async Task<IActionResult> AssignShipper(int id, string shipperId)
        {
            var request = _context.ShippingRequests.Find(id);
            if (request != null)
            {
                request.ShipperId = shipperId;
                request.Status = "Đang đến chỗ người gửi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ShippingRequests");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Shipper")]
        public async Task<IActionResult> UpdateShippingStatus(int id, string currentStatus)
        {
            var request = _context.ShippingRequests.Find(id);
            if (request != null)
            {
                if (currentStatus == "Đang đến chỗ người gửi")
                    request.Status = "Đơn của bạn đang đến kho";
                else if (currentStatus == "Đơn của bạn đang đến kho")
                    request.Status = "Đơn của bạn đang vận chuyển đến người nhận";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ShippingRequests");
        }

        // ANNOUNCEMENTS
        [HttpGet]
        [AllowAnonymous] // Cho phép tất cả truy cập
        public IActionResult Announcements()
        {
            var announcements = _context.Announcements.OrderByDescending(a => a.CreatedAt).ToList();
            return View(announcements);
        }

        [HttpGet]
        public IActionResult CreateAnnouncement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAnnouncement(Announcement model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.Announcements.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Announcements");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CommentReports()
        {
            var commentReports = _context.CommentReports
                .Include(r => r.Comment).ThenInclude(c => c.Post)
                .Include(r => r.ReportedByUser)
                .OrderByDescending(r => r.ReportedAt)
                .ToList();
            return View(commentReports);
        }

      
    }
}

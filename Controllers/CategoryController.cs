using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using ExchangeWebsite.Models;

namespace ExchangeWebsite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ExchangeWebsiteContext _context;
        private readonly UserManager<User> _userManager;

        public CategoryController(ExchangeWebsiteContext context, UserManager<User> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public IActionResult Index(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            var posts = _context.Posts
                .Where(p => p.CategoryId == id)
                .Include(p => p.PostImages)
                .OrderByDescending(p => p.PostId)
                .ToList();

            ViewBag.Posts = posts;
            return View(category);
        }

        public async Task<IActionResult> Post(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostImages)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null) return NotFound();
            return View(post);
        }

        [HttpGet]
        public IActionResult Index(int id, int? subCategoryId, string sort, string q)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound();

            var postsQuery = _context.Posts
                .Include(p => p.PostImages)
                .Where(p => p.CategoryId == id);

            if (subCategoryId.HasValue)
            {
                postsQuery = postsQuery.Where(p => p.CategoryId == subCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                postsQuery = postsQuery.Where(p =>
                    p.Title.Contains(q) ||
                    (p.Description != null && p.Description.Contains(q))
                );
            }

            postsQuery = sort switch
            {
                "date_asc" => postsQuery.OrderBy(p => p.PostedAt),
                "date_desc" => postsQuery.OrderByDescending(p => p.PostedAt),
                "price_asc" => postsQuery.OrderBy(p => p.Price),
                "price_desc" => postsQuery.OrderByDescending(p => p.Price),
                _ => postsQuery.OrderByDescending(p => p.PostId)
            };

            var posts = postsQuery.ToList();
            ViewBag.Posts = posts;
            return View(category);
        }

        [Authorize]
        [HttpGet]
        public IActionResult CreatePost()
        {
            var subCategories = _context.Categories
                .Where(c => c.ParentCategoryId != null)
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.Categories = subCategories;
            ViewBag.Conditions = new List<string> { "Mới", "Gần như mới", "Tốt", "Khá", "Hỏng" };
            ViewBag.Languages = new List<string> { "Tiếng Anh", "Tiếng Việt", "Tiếng Pháp", "Tiếng Đức" };

            var allCategories = _context.Categories
                .Select(c => new {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ParentCategoryId = c.ParentCategoryId
                }).ToList();
            ViewBag.AllCategories = allCategories;

            ViewBag.ParentCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(
            [Bind("Title,Price,City,District,StreetAddress,ZipCode,Description,Make,ModelNumber,Condition,CryptocurrencyAccepted,DeliveryAvailable,ContactEmail,PhoneNumber,ShowAddress,CategoryId,Language")] Post post,
            List<IFormFile> images)
        {
            var allowedConditions = new List<string> { "Mới", "Gần như mới", "Tốt", "Khá", "Hỏng" };

            // Validate Condition
            if (string.IsNullOrWhiteSpace(post.Condition) || !allowedConditions.Contains(post.Condition))
            {
                ModelState.AddModelError("Condition", "Giá trị tình trạng không hợp lệ.");
            }

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                post.UserId = userId;
            }

            // VIP post limit logic (unchanged)
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && (!user.IsVip || (user.VipExpiration != null && user.VipExpiration <= DateTime.UtcNow)))
            {
                var today = DateTime.UtcNow.Date;
                var userPostCount = _context.Posts.Count(p =>
                    p.UserId == userId &&
                    p.PostedAt >= today && p.PostedAt < today.AddDays(1));
                if (userPostCount >= 1)
                {
                    ModelState.AddModelError("", "Bạn đã đạt giới hạn tối đa 1 bài đăng. Vui lòng nâng cấp lên VIP để đăng không giới hạn.");
                    ViewBag.ShowVipModal = true;
                    ViewBag.ParentCategories = _context.Categories
                        .Where(c => c.ParentCategoryId == null)
                        .OrderBy(c => c.CategoryName)
                        .ToList();
                    ViewBag.Conditions = new List<string> { "Mới", "Gần như mới", "Tốt", "Khá", "Hỏng" };
                    ViewBag.Languages = new List<string> { "Tiếng Anh", "Tiếng Việt", "Tiếng Pháp", "Tiếng Đức" };

                    return View(post);
                }
            }

            post.PostedAt = DateTime.UtcNow;

            ViewBag.Categories = _context.Categories
                .Where(c => c.ParentCategoryId != null)
                .OrderBy(c => c.CategoryName)
                .ToList();
            ViewBag.Conditions = new List<string> { "Mới", "Gần như mới", "Tốt", "Khá", "Hỏng" };
            ViewBag.Languages = new List<string> { "Tiếng Anh", "Tiếng Việt", "Tiếng Pháp", "Tiếng Đức" };

            // --- Image validation ---
            long totalSize = 0;
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    // Check file size
                    totalSize += image.Length;
                    // Check file type
                    if (!image.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("images", "Chỉ cho phép tải lên tệp hình ảnh.");
                        break;
                    }
                }
                if (totalSize > 30 * 1024 * 1024)
                {
                    ModelState.AddModelError("images", "File phải nhỏ hơn 30MB.");
                }
            }
            // --- End image validation ---

            // Extra business logic validation (optional)
            if (post.Price.HasValue && post.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Count > 0)
                {
                    ViewBag.Errors = errors;
                }
                return View(post);
            }

            _context.Add(post);
            await _context.SaveChangesAsync();

            if (images != null && images.Count > 0)
            {
                string uploadsRoot = "/var/data/uploads/posts";
                if (!Directory.Exists(uploadsRoot))
                    Directory.CreateDirectory(uploadsRoot);

                foreach (var image in images)
                {
                    if (image.Length > 0)
                    {
                        string uniqueFileName = $"{post.PostId}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                        string filePath = Path.Combine(uploadsRoot, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        var postImage = new PostImage
                        {
                            PostId = post.PostId,
                            ImagePath = $"/uploads/posts/{uniqueFileName}"
                        };
                        _context.PostImages.Add(postImage);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Bài đăng đã tạo thành công";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult MyPost()
        {
            var userId = _userManager.GetUserId(User);
            var posts = _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.PostImages)
                .OrderByDescending(p => p.PostedAt)
                .ToList();

            ViewBag.Posts = posts;
            return View();
        }

        [HttpGet]
        public IActionResult Search(string q)
        {
            var postsQuery = _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                postsQuery = postsQuery.Where(p =>
                    p.Title.Contains(q) ||
                    (p.Description != null && p.Description.Contains(q))
                );
            }

            var posts = postsQuery.OrderByDescending(p => p.PostedAt).ToList();
            ViewBag.Posts = posts;
            ViewBag.SearchQuery = q;
            return View("SearchResults");
        }

        [HttpGet]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return NotFound();

            // Optional: Only allow the owner or admin to delete
            var userId = _userManager.GetUserId(User);
            if (post.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return View(post);
        }

        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostConfirmed(int id)
        {
            var post = await _context.Posts
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
                return NotFound();

            // Optional: Only allow the owner or admin to delete
            var userId = _userManager.GetUserId(User);
            if (post.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Delete images from disk
            if (post.PostImages != null)
            {
                var uploadsRoot = "/var/data/uploads/posts";
                foreach (var image in post.PostImages)
                {
                    var fileName = Path.GetFileName(image.ImagePath);
                    var filePath = Path.Combine(uploadsRoot, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _context.PostImages.Remove(image);
                }
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bài đăng đã xóa thành công";
            return RedirectToAction("MyPost");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportPost(int postId, string description)
        {
            if (!User.Identity.IsAuthenticated) return Unauthorized();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Post", new { id = postId });

            var report = new Report
            {
                PostId = postId,
                ReporterId = userId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["ReportSuccess"] = "Cảm ơn bạn đã báo cáo bài viết!";
            return RedirectToAction("Post", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestShipping(
     int postId,
     string senderName,
     string senderPhone,
     string senderCity,
     string senderDistrict,
     string senderAddress,
     decimal shippingPrice)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            // Lưu ShippingRequest mới
            var shippingRequest = new ShippingRequest
            {
                PostId = postId,
                SenderName = senderName,
                SenderPhone = senderPhone,
                SenderCity = senderCity,
                SenderDistrict = senderDistrict,
                SenderAddress = senderAddress,
                ShippingPrice = shippingPrice,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };
            _context.Add(shippingRequest);

            post.ShippingRequested = true;
            await _context.SaveChangesAsync();

            // Gửi thông báo cho admin
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var context = scope.ServiceProvider.GetRequiredService<ExchangeWebsiteContext>();
                var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
                var notifications = new List<Notification>();
                foreach (var admin in adminUsers)
                {
                    notifications.Add(new Notification
                    {
                        UserId = admin.Id,
                        Message = $"Có yêu cầu ship mới từ {senderName} cho bài đăng #{post.PostId}.",
                        Link = Url.Action("ShippingRequests", "Admin"),
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                context.Notifications.AddRange(notifications);
                await context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Đã gửi yêu cầu ship!";
            return RedirectToAction("Post", new { id = postId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProvincesProxy()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using var client = new HttpClient(handler);
            var response = await client.GetAsync("https://provinces.open-api.vn/api/?depth=2");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
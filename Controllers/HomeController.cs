namespace ExchangeWebsite.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController : Controller
    {
        private readonly ExchangeWebsiteContext _context;

        public HomeController(ExchangeWebsiteContext context)
        {
            _context = context;
        }

        public IActionResult Index(int vipPage = 1, int vipPageSize = 3)
        {
            var categories = _context.Categories
                .Include(c => c.SubCategories)
                .Where(c => c.ParentCategoryId == null)
                .ToList();

            var vipPostsQuery = _context.Posts
                 .Include(p => p.PostImages)
                 .Include(p => p.User)
                 .Where(p => p.User != null && p.User.IsVip && (p.User.VipExpiration == null || p.User.VipExpiration > DateTime.UtcNow))
                 .OrderByDescending(p => p.PostedAt);

            var totalVipPosts = vipPostsQuery.Count();
            var featuredVipPosts = vipPostsQuery
                .Skip((vipPage - 1) * vipPageSize)
                .Take(vipPageSize)
                .ToList();

            ViewBag.FeaturedVipPosts = featuredVipPosts;
            ViewBag.VipPage = vipPage;
            ViewBag.VipPageSize = vipPageSize;
            ViewBag.VipTotalPages = (int)Math.Ceiling((double)totalVipPosts / vipPageSize);

            return View(categories);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
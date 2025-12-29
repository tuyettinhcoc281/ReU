using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExchangeWebsite.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using ExchangeWebsite.Controllers;

[Authorize] // Chỉ yêu cầu đăng nhập, không giới hạn role
public class OrderTrackingController : Controller
{
    private readonly ExchangeWebsiteContext _context;
    private readonly UserManager<User> _userManager;

    public OrderTrackingController(ExchangeWebsiteContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        // Lấy tất cả ShippingRequests liên quan đến user (tùy logic của bạn)
        var requests = await _context.ShippingRequests
            .Include(r => r.Post)
            .Where(r => r.Post.UserId == currentUser.Id)
            .ToListAsync();

        return View(requests);
    }
}
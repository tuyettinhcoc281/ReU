using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExchangeWebsite.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using ExchangeWebsite.Controllers;

[Authorize(Roles = "Shipper")]
public class ShippingController : Controller
{
    private readonly ExchangeWebsiteContext _context;
    private readonly UserManager<User> _userManager;

    public ShippingController(ExchangeWebsiteContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var assignedCount = _context.ShippingRequests.Count(r => r.ShipperId == currentUser.Id && r.Status != "Hoàn thành");
        var completedCount = _context.ShippingRequests.Count(r => r.ShipperId == currentUser.Id && r.Status == "Hoàn thành");
        ViewBag.AssignedCount = assignedCount;
        ViewBag.CompletedCount = completedCount;
        return View();
    }

    public async Task<IActionResult> ShippingRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var requests = _context.ShippingRequests
            .Include(r => r.Post)
            .Where(r => r.ShipperId == currentUser.Id && r.Status != "Hoàn thành")
            .ToList();
        return View(requests);
    }

    public async Task<IActionResult> CompletedRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var requests = _context.ShippingRequests
            .Include(r => r.Post)
            .Where(r => r.ShipperId == currentUser.Id && r.Status == "Hoàn thành")
            .ToList();
        return View(requests);
    }
    [Authorize(Roles = "Shipper")]
    [HttpPost]
    public async Task<IActionResult> UpdateShippingStatus(int shippingRequestId, string newStatus)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var request = await _context.ShippingRequests
            .FirstOrDefaultAsync(r => r.ShippingRequestId == shippingRequestId && r.ShipperId == currentUser.Id);

        if (request == null)
        {
            return NotFound();
        }

        request.Status = newStatus;
        await _context.SaveChangesAsync();

        // Optionally, add a TempData or ViewBag message for feedback
        TempData["StatusMessage"] = "Shipping status updated successfully.";

        // Redirect back to the list or details pageĐang đến chỗ người nhận	
        return RedirectToAction("ShippingRequests");

    }
    [Authorize(Roles = "Shipper")]
    [HttpGet]
    public async Task<IActionResult> ShipperShippingRequests()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var requests = await _context.ShippingRequests
            .Include(r => r.Post)
            .ThenInclude(p => p.User)
            .Where(r => r.ShipperId == currentUser.Id)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        ViewBag.Shippers = new List<User> { currentUser }; // Optional, for view compatibility
        return View("ShipperShippingRequests", requests);
    }
}
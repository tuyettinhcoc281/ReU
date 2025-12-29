using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExchangeWebsite.Models;
using System.Threading.Tasks;
using System.Linq;

namespace ExchangeWebsite.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ExchangeWebsiteContext _context;
        private readonly UserManager<User> _userManager;

        public NotificationController(ExchangeWebsiteContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            var unread = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead);
            foreach (var n in unread)
                n.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
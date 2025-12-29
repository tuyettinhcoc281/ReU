using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExchangeWebsite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ExchangeWebsite.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ExchangeWebsiteContext _context;
        private readonly UserManager<User> _userManager;

        public CommentController(ExchangeWebsiteContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int postId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Post", "Category", new { id = postId });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Forbid();
            }

            var user = await _userManager.FindByIdAsync(userId);

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Notification cho chủ bài viết
            if (parentCommentId == null)
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post != null && post.UserId != userId)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = post.UserId,
                        Message = $"{user.UserName} đã bình luận bài viết của bạn.",
                        Link = Url.Action("Post", "Category", new { id = postId }),
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
            }
            // Notification cho chủ comment gốc khi có reply trực tiếp vào comment gốc
            else
            {
                var parentComment = await _context.Comments.FindAsync(parentCommentId);
                if (parentComment != null && parentComment.UserId != userId)
                {
                    // Chỉ gửi nếu parentComment là comment gốc (không phải reply của reply)
                    if (parentComment.ParentCommentId == null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = parentComment.UserId,
                            Message = $"{user.UserName} đã trả lời bình luận của bạn.",
                            Link = Url.Action("Post", "Category", new { id = postId }) + $"#comment-{parentCommentId}",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }

          

            return RedirectToAction("Post", "Category", new { id = postId });
        }

        [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(int id)
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                var isAdmin = user != null && await _userManager.IsInRoleAsync(user, "Admin");

                // Check if user is owner of the comment or admin
                if (comment.UserId != userId && !isAdmin)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xóa comment này";
                    return RedirectToAction("Post", "Category", new { id = comment.PostId });
                }

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comment đã xóa thành công";
                return RedirectToAction("Post", "Category", new { id = comment.PostId });
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Report(int id, string reason)
            {
                var comment = await _context.Comments.FindAsync(id);
                if (comment == null)
                {
                    return NotFound();
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Forbid();
                }

                // Check if user already reported this comment
                var existingReport = await _context.CommentReports
                    .FirstOrDefaultAsync(r => r.CommentId == id && r.ReportedByUserId == userId);

                if (existingReport != null)
                {
                    TempData["InfoMessage"] = "Bạn đã report comment này rồi!";
                    return RedirectToAction("Post", "Category", new { id = comment.PostId });
                }

                var report = new CommentReport
                {
                    CommentId = id,
                    ReportedByUserId = userId,
                    Reason = reason,
                    ReportedAt = DateTime.Now,
                    Status = "Pending"
                };

                _context.CommentReports.Add(report);
                await _context.SaveChangesAsync();

                // Notify admins about the new report
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in admins)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = admin.Id,
                        Message = $"Có báo cáo mới về bình luận.",
                        Link = Url.Action("CommentReports", "Admin"),
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Comment reported successfully";
                return RedirectToAction("Post", "Category", new { id = comment.PostId });
            }
        }
    }

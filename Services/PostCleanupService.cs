using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using ExchangeWebsite.Controllers; // For ExchangeWebsiteContext

public class PostCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    // For testing: run every 24 hours
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public PostCleanupService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ExchangeWebsiteContext>();
                var cutoff = DateTime.UtcNow.AddMonths(-1);
                var oldPosts = await db.Posts
                    .Include(p => p.PostImages)
                    .Where(p => p.PostedAt < cutoff)
                    .ToListAsync(stoppingToken);

                if (oldPosts.Any())
                {
                    db.Posts.RemoveRange(oldPosts);
                    await db.SaveChangesAsync(stoppingToken);
                }
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
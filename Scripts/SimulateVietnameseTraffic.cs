using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

public class SimulateVietnameseTraffic
{
    private static readonly string[] Pages = { "/", "/about", "/products", "/contact", "/blog", "/login", "/register" };
    private static readonly string[] Referrers = { "https://google.com.vn", "https://facebook.com", "https://zalo.me", "https://dantri.com.vn", "" };
    private static readonly string[] UserAgents = {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:115.0) Gecko/20100101 Firefox/115.0",
        "Mozilla/5.0 (Linux; Android 10; SM-A505F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Mobile Safari/537.36",
        "Mozilla/5.0 (iPhone; CPU iPhone OS 16_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Mobile/15E148 Safari/604.1"
    };
    private static readonly Random Rand = new();

    public static async Task Main(string[] args)
    {
        int userCount = Rand.Next(1000, 1201); // 1k - 1k2 user
        var tasks = new List<Task>();

        for (int i = 0; i < userCount; i++)
        {
            tasks.Add(SimulateUserSession());
            await Task.Delay(Rand.Next(100, 500)); // Giãn cách tạo user, tránh burst quá mạnh
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"Đã mô phỏng xong {userCount} user.");
    }

    private static async Task SimulateUserSession()
    {
        using var client = new HttpClient();
        string baseUrl = "https://reusevn.onrender.com";
        string page = Pages[Rand.Next(Pages.Length)];
        string url = baseUrl + page;
        string userAgent = UserAgents[Rand.Next(UserAgents.Length)];
        string referrer = Referrers[Rand.Next(Referrers.Length)];

        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("vi-VN,vi;q=0.9");
        if (!string.IsNullOrEmpty(referrer))
            client.DefaultRequestHeaders.Referrer = new Uri(referrer);

        try
        {
            var response = await client.GetAsync(url);
            // Giả lập thời gian ở lại web ~1 phút
            await Task.Delay(Rand.Next(55_000, 65_000));
        }
        catch
        {
            // Bỏ qua lỗi kết nối
        }
    }
}
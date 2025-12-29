using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExchangeWebsite.Models;
using System.Threading.Tasks;

public class VietQRModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public VietQRModel(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public bool PaymentSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync(bool? success)
    {
        PaymentSuccess = success == true;

        if (PaymentSuccess && User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.IsVip = true;
                user.VipExpiration = DateTime.UtcNow.AddMonths(1);
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        return Page();
    }
}
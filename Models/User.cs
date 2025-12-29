namespace ExchangeWebsite.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class User : IdentityUser
{
    public string FirstName { get; set; } 
    public string LastName { get; set; } 
    public bool IsVip { get; set; }
    public DateTime? VipExpiration { get; set; }
}


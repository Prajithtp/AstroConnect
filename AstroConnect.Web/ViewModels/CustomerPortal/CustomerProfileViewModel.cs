using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.ViewModels.CustomerPortal;

public class CustomerProfileViewModel
{
    public int CustomerId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Date of Birth")]
    public DateTime BirthDate { get; set; }

    [Display(Name = "Birth Time")]
    public TimeSpan BirthTime { get; set; }

    public string BirthPlace { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
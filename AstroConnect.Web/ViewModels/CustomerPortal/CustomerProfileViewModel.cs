using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.ViewModels.CustomerPortal;

public class CustomerProfileViewModel
{
    // =========================================================
    // ID
    // =========================================================

    public int CustomerId { get; set; }


    // =========================================================
    // BASIC INFORMATION
    // =========================================================

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Full name must be between 2 and 100 characters.")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;


    // Email is displayed on the profile but is read-only.
    // The Profile POST action restores it from the database.
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(
        20,
        MinimumLength = 7,
        ErrorMessage = "Phone number must be between 7 and 20 characters.")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;


    // =========================================================
    // BIRTH INFORMATION
    // =========================================================

    [Required(ErrorMessage = "Date of birth is required.")]
    [Display(Name = "Date of Birth")]
    public DateTime BirthDate { get; set; }


    [Required(ErrorMessage = "Birth time is required.")]
    [Display(Name = "Birth Time")]
    public TimeSpan BirthTime { get; set; }


    [Required(ErrorMessage = "Birth place is required.")]
    [StringLength(
        150,
        ErrorMessage = "Birth place cannot exceed 150 characters.")]
    [Display(Name = "Birth Place")]
    public string BirthPlace { get; set; } = string.Empty;


    // =========================================================
    // PERSONAL INFORMATION
    // =========================================================

    [Required(ErrorMessage = "Gender is required.")]
    [StringLength(
        30,
        ErrorMessage = "Gender cannot exceed 30 characters.")]
    public string Gender { get; set; } = string.Empty;


    [StringLength(
        300,
        ErrorMessage = "Address cannot exceed 300 characters.")]
    public string Address { get; set; } = string.Empty;
}
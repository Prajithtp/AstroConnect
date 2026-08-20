using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.Models.ViewModels;

public class AstrologerCreateViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public int Experience { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    public string Language { get; set; } = "Malayalam";

    public string Biography { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
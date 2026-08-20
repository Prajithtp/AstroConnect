using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.Models.ViewModels;

public class CustomerCreateViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public string BirthTime { get; set; } = string.Empty;

    [Required]
    public string BirthPlace { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.ViewModels.Service
{
    public class ServiceCreateViewModel
    {
        [Required]
        [Display(Name = "Service Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Duration (Minutes)")]
        [Range(15, 180)]
        public int DurationInMinutes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
using System.ComponentModel.DataAnnotations;

namespace AstroConnect.Web.Models
{
    public class ServiceEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int DurationInMinutes { get; set; }
    }
}
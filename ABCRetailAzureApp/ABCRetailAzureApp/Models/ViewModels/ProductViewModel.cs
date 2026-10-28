using System.ComponentModel.DataAnnotations;

namespace ABCRetailAzureApp.Models.ViewModels
{
    public class ProductViewModel
    {
        public Product Product { get; set; } = new Product();

        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}

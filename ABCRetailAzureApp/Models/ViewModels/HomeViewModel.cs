namespace ABCRetailAzureApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public List<Product> RecentProducts { get; set; } = new List<Product>();
        public List<CustomerProfile> RecentCustomers { get; set; } = new List<CustomerProfile>();
    }
}

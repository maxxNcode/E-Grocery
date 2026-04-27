using E_Grocery.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Grocery.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Apple", Price = 1.50m, Description = "Fresh red apple", ImageUrl = "/images/apple.jpg", StockQty = 100 },
                new Product { Id = 2, Name = "Banana", Price = 0.75m, Description = "Ripe banana", ImageUrl = "/images/banana.jpg", StockQty = 150 },
                new Product { Id = 3, Name = "Orange", Price = 1.25m, Description = "Juicy orange", ImageUrl = "/images/orange.jpg", StockQty = 80 }
            };
            return View(products);
        }
    }
}
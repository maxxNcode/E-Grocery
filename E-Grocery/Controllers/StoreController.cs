using E_Grocery.Data;
using E_Grocery.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Grocery.Controllers
{
    public class StoreController : Controller
    {
        private readonly AppDbContext _context;

        public StoreController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Cart()
        {
            var cartItems = _context.CartItems.ToList();
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var products = _context.Products.ToList();
            Product product = null;
            foreach (var p in products)
            {
                if (p.Id == productId)
                {
                    product = p;
                    break;
                }
            }

            if (product != null)
            {
                var cartItems = _context.CartItems.ToList();
                CartItem existing = null;
                foreach (var c in cartItems)
                {
                    if (c.ProductId == productId)
                    {
                        existing = c;
                        break;
                    }
                }

                if (existing != null)
                {
                    existing.Quantity += quantity;
                }
                else
                {
                    _context.CartItems.Add(new CartItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        Quantity = quantity
                    });
                }
                _context.SaveChanges();
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder()
        {
            var cartItems = _context.CartItems.ToList();
            foreach (var item in cartItems)
            {
                _context.CartItems.Remove(item);
            }
            _context.SaveChanges();
            return View("OrderComplete");
        }
    }
}
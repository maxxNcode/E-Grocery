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
            ViewBag.IsSearch = false;
            ViewBag.SearchTerm = "";
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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null || userEmail == "")
            {
                return RedirectToAction("Login", "Account");
            }

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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null || userEmail == "")
            {
                userEmail = "Guest";
            }
            var cartItems = _context.CartItems.ToList();
            var products = _context.Products.ToList();

            foreach (var item in cartItems)
            {
                foreach (var p in products)
                {
                    if (p.Id == item.ProductId)
                    {
                        p.StockQty -= item.Quantity;
                        if (p.StockQty < 0) p.StockQty = 0;
                        break;
                    }
                }

                var order = new Order
                {
                    UserEmail = userEmail,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Total = item.Price * item.Quantity,
                    OrderDate = DateTime.Now
                };
                _context.Orders.Add(order);
            }

            foreach (var item in cartItems)
            {
                _context.CartItems.Remove(item);
            }

            _context.SaveChanges();
            return View("OrderComplete");
        }

        public IActionResult OrderHistory()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null || userEmail == "")
            {
                userEmail = "Guest";
            }
            var allOrders = _context.Orders.ToList();
            var userOrders = new List<Order>();
            foreach (var o in allOrders)
            {
                if (o.UserEmail == userEmail)
                {
                    userOrders.Add(o);
                }
            }
            return View(userOrders);
        }

        public IActionResult Search(string searchTerm)
        {
            var allProducts = _context.Products.ToList();
            var filtered = new List<Product>();

            if (searchTerm == null || searchTerm == "")
            {
                filtered = allProducts;
                ViewBag.IsSearch = false;
            }
            else
            {
                var term = searchTerm.ToLower();
                foreach (var p in allProducts)
                {
                    if (p.Name != null && p.Name.ToLower().Contains(term))
                    {
                        filtered.Add(p);
                    }
                }
                ViewBag.IsSearch = true;
                ViewBag.SearchTerm = searchTerm;
            }

            return View("Index", filtered);
        }
    }
}
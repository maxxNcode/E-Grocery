using E_Grocery.Data;
using E_Grocery.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Grocery.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int? id)
        {
            // If id is provided, load product for editing
            if (id.HasValue)
            {
                var product = _context.Products.Find(id.Value);
                if (product != null)
                {
                    var model = new ProductCreateViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        Image = product.ImageUrl,
                        StockQty = product.StockQty
                    };
                    ViewData["Products"] = _context.Products.ToList();
                    return View(model);
                }
            }

            // Fresh create mode — just show the list
            ViewData["Products"] = _context.Products.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id > 0)
                {
                    // UPDATE existing product
                    var product = _context.Products.Find(model.Id);
                    if (product != null)
                    {
                        product.Name = model.Name;
                        product.Description = model.Description;
                        product.Price = model.Price;
                        product.ImageUrl = model.Image;
                        product.StockQty = model.StockQty;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    // CREATE new product
                    var product = new Product
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        ImageUrl = model.Image,
                        StockQty = model.StockQty
                    };
                    _context.Products.Add(product);
                    _context.SaveChanges();
                }

                return RedirectToAction("Create");
            }

            // Validation failed — reload the list so it still shows
            ViewData["Products"] = _context.Products.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Create");
        }
    }
}
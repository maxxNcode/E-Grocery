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
        public IActionResult Create()
        {
            ViewData["Products"] = _context.Products.ToList();
            ViewData["CartCount"] = _context.CartItems.ToList().Count;
            ViewData["HideNav"] = true;
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var products = _context.Products.ToList();
            Product product = null;
            foreach (var p in products)
            {
                if (p.Id == id)
                {
                    product = p;
                    break;
                }
            }

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
                ViewData["CartCount"] = _context.CartItems.ToList().Count;
                ViewData["HideNav"] = true;
                return View("Create", model);
            }

            return RedirectToAction("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imagePath = model.Image;

                if (model.ProductImage != null && model.ProductImage.Length > 0)
                {
                    var ext = "";
                    var originalName = model.ProductImage.FileName;
                    if (originalName.Contains("."))
                    {
                        ext = originalName.Substring(originalName.LastIndexOf("."));
                    }
                    var fileName = DateTime.Now.Ticks.ToString() + ext;
                    var filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\products\\" + fileName;

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProductImage.CopyTo(stream);
                    }

                    imagePath = "/images/products/" + fileName;
                }

                if (model.Id > 0)
                {
                    // UPDATE existing product
                    var products = _context.Products.ToList();
                    Product product = null;
                    foreach (var p in products)
                    {
                        if (p.Id == model.Id)
                        {
                            product = p;
                            break;
                        }
                    }

                    if (product != null)
                    {
                        product.Name = model.Name;
                        product.Description = model.Description;
                        product.Price = model.Price;
                        if (imagePath != null && imagePath != "")
                        {
                            product.ImageUrl = imagePath;
                        }
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
                        ImageUrl = imagePath,
                        StockQty = model.StockQty
                    };
                    _context.Products.Add(product);
                    _context.SaveChanges();
                }

                return RedirectToAction("Create");
            }

            ViewData["Products"] = _context.Products.ToList();
            ViewData["CartCount"] = _context.CartItems.ToList().Count;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var products = _context.Products.ToList();
            Product product = null;
            foreach (var p in products)
            {
                if (p.Id == id)
                {
                    product = p;
                    break;
                }
            }

            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Create");
        }
    }
}
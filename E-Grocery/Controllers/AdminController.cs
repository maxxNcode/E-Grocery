using E_Grocery.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Grocery.Controllers
{
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index", "Store");
            }
            return View(model);
        }
    }
}

using E_Grocery.Data;
using E_Grocery.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Grocery.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var users = _context.Users.ToList();
                User user = null;
                foreach (var u in users)
                {
                    if (u.Email == model.Email && u.Password == model.Password)
                    {
                        user = u;
                        break;
                    }
                }

                if (user != null)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    return RedirectToAction("Index", "Store");
                }

                ModelState.AddModelError("", "Invalid email or password");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserEmail");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null || email == "")
            {
                return RedirectToAction("Login");
            }

            var users = _context.Users.ToList();
            User user = null;
            foreach (var u in users)
            {
                if (u.Email == email)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(User model, IFormFile profilePicture)
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (email == null || email == "")
            {
                return RedirectToAction("Login");
            }

            var users = _context.Users.ToList();
            User user = null;
            foreach (var u in users)
            {
                if (u.Email == email)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FullName = model.FullName;

            if (profilePicture != null && profilePicture.Length > 0)
            {
                var ext = "";
                var originalName = profilePicture.FileName;
                if (originalName.Contains("."))
                {
                    ext = originalName.Substring(originalName.LastIndexOf("."));
                }
                var fileName = DateTime.Now.Ticks.ToString() + ext;
                var filePath = Directory.GetCurrentDirectory() + "\\wwwroot\\images\\profiles\\" + fileName;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profilePicture.CopyTo(stream);
                }

                user.ProfilePicture = "/images/profiles/" + fileName;
            }

            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
    }
}
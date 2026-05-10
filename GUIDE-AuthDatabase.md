# Guide: Make Login & Register Work with the Database

This guide follows **exactly** the patterns from your class notes (`A Deeper Look into Views and Models.md`).

---

## What We Will Do

1. Add the Entity Framework NuGet packages
2. Create a `User` model (like the notes' `Student` model)
3. Create a `Data` folder + `AppDbContext`
4. Add a connection string in `appsettings.json`
5. Register the DbContext in `Program.cs`
6. Inject `AppDbContext` into `AccountController`
7. Save users on Register
8. Query users on Login
9. Run migrations to create the database table

---

## Step 1: Install Entity Framework Packages

Open **Tools > NuGet Package Manager > Package Manager Console** and run:

```powershell
dotnet add E-Grocery package Microsoft.EntityFrameworkCore
dotnet add E-Grocery package Microsoft.EntityFrameworkCore.SqlServer
dotnet add E-Grocery package Microsoft.EntityFrameworkCore.Tools
```

These match the notes exactly:
- `EntityFrameworkCore`
- `EntityFrameworkCore.SqlServer`
- `EntityFrameworkCore.Tools`

---

## Step 2: Create the `User` Model

Create a new file: `E-Grocery/Models/User.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace E_Grocery.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
```

**Why?** The notes create a `Student` model with `Id`, `Name`, `Course`. We create a `User` model with `Id`, `FullName`, `Email`, `Password`.

> **Note:** `ConfirmPassword` is NOT stored in the database. It only lives in the `RegisterViewModel` for form validation.

---

## Step 3: Create the `Data` Folder and `AppDbContext`

Create folder: `E-Grocery/Data/`

Create file: `E-Grocery/Data/AppDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using E_Grocery.Models;

namespace E_Grocery.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
```

**Why?** The notes show this exact pattern: create a `Data` folder, inherit from `DbContext`, inject `DbContextOptions`, and add a `DbSet<T>`.

---

## Step 4: Add the Connection String

Open: `E-Grocery/appsettings.json`

Add this **inside the top-level curly braces** (add a comma after the `Logging` block if needed):

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EGroceryDB;TrustServerCertificate=True;"
}
```

**Full example:**

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EGroceryDB;TrustServerCertificate=True;"
    }
}
```

**Why?** The notes use `ELNET1DB`; we use `EGroceryDB` for our app.

---

## Step 5: Register `AppDbContext` in `Program.cs`

Open: `E-Grocery/Program.cs`

Add these `using` statements at the **very top** of the file:

```csharp
using Microsoft.EntityFrameworkCore;
using E_Grocery.Data;
```

Then, inside `Program.cs`, find the line:

```csharp
var builder = WebApplication.CreateBuilder(args);
```

After `builder.Services.AddControllersWithViews();` (or anywhere in the services block), add:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

**Why?** The notes show this exact registration pattern.

---

## Step 6: Update `AccountController` to Use the Database

Open: `E-Grocery/Controllers/AccountController.cs`

Replace the entire file with:

```csharp
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
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
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
    }
}
```

**Why?** This follows the notes' controller pattern exactly:
- Inject `AppDbContext` via constructor
- Use `_context.Users.Add(user); _context.SaveChanges();` to save (same as `_context.Students.Add(student)`)
- Use `_context.Users.FirstOrDefault(...)` to query (same as `_context.Students.ToList()` but with a `where` filter)
- Add `[ValidateAntiForgeryToken]` on POST actions (shown in the notes)
- Check `ModelState.IsValid` first (shown in the notes)

---

## Step 7: Run Migrations

Open **Tools > NuGet Package Manager > Package Manager Console**.

Make sure the **Default Project** dropdown is set to `E-Grocery`.

Run these commands in order:

```powershell
Add-Migration InitialCreate
Update-Database
```

**Why?** The notes say: "Enter these in order: 1. Add-Migration InitialCreate 2. Update-Database"

This will create the `Users` table in SQL Server LocalDB.

---

## Step 8: Test It

1. Run the app (`Ctrl+F5` or `dotnet run`)
2. Go to **Register** and create an account
3. Go to **Login** and try logging in with that email and password

If registration works but login fails, check:
- Did you run `Update-Database`?
- Is the email typed exactly the same (case-sensitive in this simple version)?

---

## Summary of the Pattern (from the notes)

| Notes Example | Our Auth Version |
|---|---|
| `Student` model | `User` model |
| `AppDbContext` with `DbSet<Student>` | `AppDbContext` with `DbSet<User>` |
| `_context.Students.Add(student);` | `_context.Users.Add(user);` |
| `_context.SaveChanges();` | `_context.SaveChanges();` |
| `_context.Students.ToList();` | `_context.Users.FirstOrDefault(...)` |
| `Create` action with `ModelState.IsValid` | `Register` action with `ModelState.IsValid` |

---

## Important Notes

- This stores passwords as **plain text** for learning purposes (the notes don't cover hashing).
- In a real project, you would hash passwords using something like `BCrypt` or ASP.NET Core Identity.
- The `ConfirmPassword` field stays in `RegisterViewModel` only — it never goes to the database.

---

## Admin Panel Access

The admin panel is **not linked anywhere in the header**. It is accessed manually by typing the URL:

```
https://localhost:PORT/Admin/Create
```

This keeps it simple — no role system, no extra login gate, no code beyond what the notes teach. The page just exists at its own route.

To add the `Admin` link back to the header later, edit `_Layout.cshtml` and add:

```html
<a asp-controller="Admin" asp-action="Create" class="hidden sm:block text-white hover:text-gray-200 text-sm font-medium transition">
    Admin
</a>
```

---

## Troubleshooting: Migration Errors

If you see this error in the Package Manager Console:

```
System.MissingMethodException: Method not found: 'System.String ...
System.TypeLoadException: Method 'Identifier' in type ... does not have an implementation.
```

**Cause:** The EF Core tools in the console are cached to an older version (e.g., 8.0.0.0) while your project uses a newer one (e.g., 10.0.7).

**Fix:** Close Visual Studio completely and reopen it. Then run:

```powershell
Add-Migration InitialCreate
Update-Database
```

**Alternative (CLI):**

If the Package Manager Console keeps failing, use the .NET CLI from a fresh terminal:

```powershell
dotnet tool install --global dotnet-ef   # only once
cd E-Grocery
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> If `dotnet ef` is not found after installing, **restart your terminal** so Windows refreshes the PATH.

---

## Step 9: Save Products to the Database

Right now the admin `Create` page only validates the form and redirects. Let's make it actually save products to the database — same pattern as saving users.

### 9A: Add `Products` to `AppDbContext`

Open: `E-Grocery/Data/AppDbContext.cs`

Add the `DbSet<Product>`:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }   // NEW
}
```

**Why?** The notes show `public DbSet<Student> Students { get; set; }`. We add a `Products` table the same way.

### 9B: Update `AdminController`

Open: `E-Grocery/Controllers/AdminController.cs`

Replace it with:

```csharp
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = model.Image,
                    StockQty = 0   // default, or add a field to the form later
                };

                _context.Products.Add(product);
                _context.SaveChanges();

                return RedirectToAction("Index", "Store");
            }
            return View(model);
        }
    }
}
```

**Why?** Same exact pattern as the notes:
- Inject `AppDbContext`
- Create a `Product` object from the ViewModel
- `_context.Products.Add(product); _context.SaveChanges();`
- Redirect after save

### 9C: Add a New Migration

Since you changed `AppDbContext` (added `DbSet<Product>`), you need a new migration:

```powershell
Add-Migration AddProductsTable
Update-Database
```

This creates the `Products` table in SQL Server.

---

## Step 10: Display Products from the Database

Right now the `StoreController` hardcodes a list of products. Let's make it read from the database instead.

### 10A: Update `StoreController`

Open: `E-Grocery/Controllers/StoreController.cs`

Replace it with:

```csharp
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
    }
}
```

**Why?** The notes show `var students = _context.Students.ToList();`. We do the exact same thing with `_context.Products.ToList();`.

### 10B: Update the Store View

Open: `E-Grocery/Views/Store/Index.cshtml`

Replace it with:

```html
@model IEnumerable<E_Grocery.Models.Product>

@{
    ViewData["Title"] = "Products";
}

<div class="min-h-screen bg-gray-50 px-4 py-8">
    <div class="max-w-6xl mx-auto">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Our Products</h1>
        <p class="text-gray-500 mb-8">Fresh groceries delivered to your door</p>

        @if (!Model.Any())
        {
            <div class="text-center py-16">
                <p class="text-gray-400 text-lg">No products available yet.</p>
                <p class="text-gray-400 text-sm mt-2">Visit /Admin/Create to add your first product.</p>
            </div>
        }
        else
        {
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                @foreach (var product in Model)
                {
                    <div class="bg-white rounded-2xl shadow-sm hover:shadow-md transition p-5">
                        <div class="h-40 bg-gray-100 rounded-xl mb-4 flex items-center justify-center overflow-hidden">
                            @if (!string.IsNullOrEmpty(product.ImageUrl))
                            {
                                <img src="@product.ImageUrl" alt="@product.Name" class="h-full w-full object-cover" />
                            }
                            else
                            {
                                <span class="text-gray-300 text-sm">No image</span>
                            }
                        </div>
                        <h3 class="font-semibold text-gray-800 text-lg">@product.Name</h3>
                        <p class="text-gray-500 text-sm mt-1 line-clamp-2">@product.Description</p>
                        <div class="flex items-center justify-between mt-4">
                            <span class="text-[#2e7d32] font-bold text-lg">₱@product.Price.ToString("N2")</span>
                            <button class="bg-[#2e7d32] text-white px-3 py-1.5 rounded-lg text-sm hover:bg-[#1b5e20] transition">
                                Add to Cart
                            </button>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
</div>
```

**Why?** The notes show:

```html
<ul>
@foreach (var s in Model)
{
    <li>@s.Name - @s.Course</li>
}
</ul>
```

We use the same `@foreach` pattern to loop through products and display them in cards.

---

## Step 11: Test the Full Flow

1. Run the app
2. Go to `/Admin/Create` and add a product (e.g., Apple, Banana)
3. Go to `/Store/Index` — you should see your products displayed
4. Register a new user at `/Account/Register`
5. Log in at `/Account/Login`

If products don't appear:
- Did you run `Add-Migration AddProductsTable` and `Update-Database`?
- Did `SaveChanges()` run without errors in the admin controller?

---

## Full File Map

| File | What It Does |
|---|---|
| `Models/User.cs` | Database table for registered users |
| `Models/Product.cs` | Database table for products |
| `Data/AppDbContext.cs` | Connects models to SQL Server |
| `appsettings.json` | Connection string for LocalDB |
| `Program.cs` | Registers `AppDbContext` |
| `Controllers/AccountController.cs` | Register/Login with database |
| `Controllers/AdminController.cs` | Save products to database |
| `Controllers/StoreController.cs` | Read products from database |
| `Views/Store/Index.cshtml` | Display product cards |

---

## Step 12: Add `StockQty` to the Admin Form

The `Product` model already has a `StockQty` field, but the create form doesn't ask for it. Let's add it.

### 12A: Add `StockQty` to `ProductCreateViewModel`

Open: `E-Grocery/Models/ProductCreateViewModel.cs`

Add the property:

```csharp
[Required(ErrorMessage = "Stock quantity is required")]
[Range(0, 10000, ErrorMessage = "Stock must be between 0 and 10000")]
public int StockQty { get; set; }
```

### 12B: Add Input Field to the Create Form

Open: `E-Grocery/Views/Admin/Create.cshtml`

Add this input **after** the Image field and **before** the Save button:

```html
<div>
    <label asp-for="StockQty" class="block text-sm font-medium text-gray-700 mb-1"></label>
    <input asp-for="StockQty" type="number" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition" placeholder="e.g. 100" />
    <span asp-validation-for="StockQty" class="text-red-500 text-xs mt-1 block"></span>
</div>
```

### 12C: Update `AdminController`

Open: `E-Grocery/Controllers/AdminController.cs`

Update the `Product` creation inside the `[HttpPost] Create` action:

```csharp
var product = new Product
{
    Name = model.Name,
    Description = model.Description,
    Price = model.Price,
    ImageUrl = model.Image,
    StockQty = model.StockQty   // NEW
};
```

### 12D: Add Migration (Optional)

`StockQty` already exists on the `Product` model from the original project. The database table already has this column if you ran the earlier migrations. If you created products before adding this field, run:

```powershell
Add-Migration AddStockQtyToProductCreate
Update-Database
```

But if your `Products` table was just created in the previous migration, this step is not needed.

---

## Step 13: Add a Product Detail Modal

Right now the store cards truncate long descriptions with `line-clamp-2`. Let's add a modal that opens when you click a product card, showing the full description, stock quantity, and letting you pick how many to add to cart.

### 13A: Update the Store View

Open: `E-Grocery/Views/Store/Index.cshtml`

Replace the entire file with:

```html
@model IEnumerable<E_Grocery.Models.Product>

@{
    ViewData["Title"] = "Products";
}

<div class="min-h-screen bg-gray-50 px-4 py-8">
    <div class="max-w-6xl mx-auto">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">Our Products</h1>
        <p class="text-gray-500 mb-8">Fresh groceries delivered to your door</p>

        @if (!Model.Any())
        {
            <div class="text-center py-16">
                <p class="text-gray-400 text-lg">No products available yet.</p>
                <p class="text-gray-400 text-sm mt-2">Visit /Admin/Create to add your first product.</p>
            </div>
        }
        else
        {
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
                @foreach (var product in Model)
                {
                    <div onclick="openModal(@product.Id)" class="bg-white rounded-2xl shadow-sm hover:shadow-md transition p-5 cursor-pointer">
                        <div class="h-40 bg-gray-100 rounded-xl mb-4 flex items-center justify-center overflow-hidden">
                            @if (!string.IsNullOrEmpty(product.ImageUrl))
                            {
                                <img src="@product.ImageUrl" alt="@product.Name" class="h-full w-full object-cover" />
                            }
                            else
                            {
                                <span class="text-gray-300 text-sm">No image</span>
                            }
                        </div>
                        <h3 class="font-semibold text-gray-800 text-lg">@product.Name</h3>
                        <p class="text-gray-500 text-sm mt-1 line-clamp-2">@product.Description</p>
                        <div class="flex items-center justify-between mt-4">
                            <span class="text-[#2e7d32] font-bold text-lg">₱@product.Price.ToString("N2")</span>
                            <button onclick="event.stopPropagation(); openModal(@product.Id)" class="bg-[#2e7d32] text-white px-3 py-1.5 rounded-lg text-sm hover:bg-[#1b5e20] transition">
                                Add to Cart
                            </button>
                        </div>
                    </div>

                    <!-- Modal for this product -->
                    <div id="modal-@product.Id" class="fixed inset-0 bg-black/50 hidden items-center justify-center z-50 p-4">
                        <div class="bg-white rounded-2xl shadow-xl max-w-md w-full p-6 relative" onclick="event.stopPropagation()">
                            <button onclick="closeModal(@product.Id)" class="absolute top-3 right-3 text-gray-400 hover:text-gray-600 text-xl font-bold">&times;</button>

                            <div class="h-48 bg-gray-100 rounded-xl mb-4 flex items-center justify-center overflow-hidden">
                                @if (!string.IsNullOrEmpty(product.ImageUrl))
                                {
                                    <img src="@product.ImageUrl" alt="@product.Name" class="h-full w-full object-cover" />
                                }
                                else
                                {
                                    <span class="text-gray-300 text-sm">No image</span>
                                }
                            </div>

                            <h2 class="text-2xl font-bold text-gray-800 mb-2">@product.Name</h2>
                            <p class="text-gray-600 text-sm mb-4 leading-relaxed">@product.Description</p>

                            <div class="flex items-center gap-6 text-sm text-gray-500 mb-6">
                                <span>Price: <strong class="text-[#2e7d32]">₱@product.Price.ToString("N2")</strong></span>
                                <span>In Stock: <strong class="text-[#2e7d32]">@product.StockQty</strong></span>
                            </div>

                            <div class="flex items-center gap-4 mb-4">
                                <label class="text-sm font-medium text-gray-700">Quantity:</label>
                                <div class="flex items-center border border-gray-300 rounded-lg overflow-hidden">
                                    <button type="button" onclick="adjustQty(@product.Id, -1, @product.StockQty)" class="px-3 py-1 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold">-</button>
                                    <input id="qty-@product.Id" type="number" value="1" min="1" max="@product.StockQty" readonly class="w-12 text-center py-1 border-x border-gray-300 text-sm" />
                                    <button type="button" onclick="adjustQty(@product.Id, 1, @product.StockQty)" class="px-3 py-1 bg-gray-100 hover:bg-gray-200 text-gray-700 font-bold">+</button>
                                </div>
                            </div>

                            <button class="w-full bg-[#2e7d32] text-white font-semibold py-2.5 rounded-lg hover:bg-[#1b5e20] transition duration-200 shadow-md">
                                Add to Cart — ₱@product.Price.ToString("N2")
                            </button>
                        </div>
                    </div>
                }
            </div>
        }
    </div>
</div>

@section Scripts {
    <script>
        function openModal(id) {
            document.getElementById('modal-' + id).classList.remove('hidden');
            document.getElementById('modal-' + id).classList.add('flex');
        }

        function closeModal(id) {
            document.getElementById('modal-' + id).classList.remove('flex');
            document.getElementById('modal-' + id).classList.add('hidden');
        }

        // Close modal when clicking outside
        document.addEventListener('click', function(event) {
            if (event.target.classList.contains('bg-black/50')) {
                event.target.classList.remove('flex');
                event.target.classList.add('hidden');
            }
        });

        function adjustQty(id, change, max) {
            var input = document.getElementById('qty-' + id);
            var val = parseInt(input.value) + change;
            if (val >= 1 && val <= max) {
                input.value = val;
            }
        }
    </script>
}
```

### How It Works

- **Clicking a card** opens that product's modal (`openModal(id)`)
- **Clicking "Add to Cart"** also opens the modal (using `event.stopPropagation()` so the card click doesn't double-fire)
- **The modal shows:**
  - Full product image (larger)
  - Full description (no truncation)
  - Price and stock quantity
  - A quantity selector with +/- buttons (capped at available stock)
  - An "Add to Cart" button
- **Clicking the X button** or **clicking the dark background** closes the modal
- **`line-clamp-2`** on the card keeps descriptions short in the grid

### Why This Pattern?

- No external libraries — just Razor `@foreach` to generate one modal per product, and vanilla JavaScript to toggle `hidden`/`flex` classes
- `event.stopPropagation()` prevents the card click from opening the modal when you click inside the modal content
- `event.stopPropagation()` on the "Add to Cart" button prevents bubbling up to the card click
- `z-50` ensures the modal sits above everything else

---

## Summary of New Changes

| File | Change |
|---|---|
| `Models/ProductCreateViewModel.cs` | Added `StockQty` property |
| `Views/Admin/Create.cshtml` | Added `StockQty` input field |
| `Controllers/AdminController.cs` | Map `model.StockQty` to `product.StockQty` |
| `Views/Store/Index.cshtml` | Added product detail modal with full description, image, price, stock, and quantity selector |

---

## Step 14: Show Product List on the Admin Page

Let's display all existing products on the same page as the create form, with **Edit** and **Delete** buttons for each product.

### 14A: Add `Id` to `ProductCreateViewModel`

Open: `E-Grocery/Models/ProductCreateViewModel.cs`

Add an `Id` property at the top:

```csharp
public int Id { get; set; }   // 0 = create mode, >0 = edit mode
```

This lets the same form handle both creating new products and editing existing ones.

### 14B: Rewrite `AdminController`

Open: `E-Grocery/Controllers/AdminController.cs`

Replace the entire file with:

```csharp
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
```

**Why?**
- `Create(int? id)` loads a product for editing when an `id` is passed
- `Create(ProductCreateViewModel model)` checks `model.Id`: if `> 0` it updates, otherwise it creates
- `Delete(int id)` finds and removes the product, then saves
- `ViewData["Products"]` passes the product list to the view (same idea as passing data to a view, just using the built-in dictionary)

### 14C: Rewrite the Create View

Open: `E-Grocery/Views/Admin/Create.cshtml`

Replace the entire file with:

```html
@model E_Grocery.Models.ProductCreateViewModel

@{
    ViewData[index: "Title"] = Model?.Id > 0 ? "Edit Product" : "Add New Product";
    var products = ViewData["Products"] as List<E_Grocery.Models.Product> ?? new List<E_Grocery.Models.Product>();
}

<div class="min-h-[calc(100vh-120px)] bg-gray-50 px-4 py-8">
    <div class="max-w-6xl mx-auto">

        <!-- Stats Cards -->
        <div class="grid grid-cols-3 gap-4 mb-8">
            <div class="bg-white rounded-xl shadow-sm p-4 text-center">
                <div class="text-xs text-gray-500 uppercase tracking-wide mb-1">Total Products</div>
                <div class="text-2xl font-bold text-[#2e7d32]">@products.Count</div>
            </div>
            <div class="bg-white rounded-xl shadow-sm p-4 text-center">
                <div class="text-xs text-gray-500 uppercase tracking-wide mb-1">Added to Cart</div>
                <div class="text-2xl font-bold text-[#2e7d32]">12</div>
            </div>
            <div class="bg-white rounded-xl shadow-sm p-4 text-center">
                <div class="text-xs text-gray-500 uppercase tracking-wide mb-1">Sold</div>
                <div class="text-2xl font-bold text-[#2e7d32]">8</div>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">

            <!-- Form -->
            <div class="bg-white rounded-2xl shadow-lg p-8">
                <div class="text-center mb-8">
                    <div class="inline-flex items-center justify-center w-16 h-16 bg-[#2e7d32] rounded-full mb-4">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                    </div>
                    <h2 class="text-2xl font-bold text-gray-800">
                        @(Model?.Id > 0 ? "Edit Product" : "Add New Product")
                    </h2>
                    <p class="text-gray-500 mt-1">
                        @(Model?.Id > 0 ? "Update the product details below" : "Fill in the product details below")
                    </p>
                </div>

                <form asp-action="Create" method="post" class="space-y-5">
                    <input type="hidden" asp-for="Id" />
                    <div asp-validation-summary="ModelOnly" class="text-red-500 text-sm"></div>

                    <div>
                        <label asp-for="Name" class="block text-sm font-medium text-gray-700 mb-1"></label>
                        <input asp-for="Name" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition" placeholder="e.g. Apple" />
                        <span asp-validation-for="Name" class="text-red-500 text-xs mt-1 block"></span>
                    </div>

                    <div>
                        <label asp-for="Description" class="block text-sm font-medium text-gray-700 mb-1"></label>
                        <textarea asp-for="Description" rows="3" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition resize-none" placeholder="e.g. Fresh red apple from the farm"></textarea>
                        <span asp-validation-for="Description" class="text-red-500 text-xs mt-1 block"></span>
                    </div>

                    <div>
                        <label asp-for="Price" class="block text-sm font-medium text-gray-700 mb-1"></label>
                        <input asp-for="Price" type="number" step="0.01" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition" placeholder="0.00" />
                        <span asp-validation-for="Price" class="text-red-500 text-xs mt-1 block"></span>
                    </div>

                    <div>
                        <label asp-for="Image" class="block text-sm font-medium text-gray-700 mb-1"></label>
                        <input asp-for="Image" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition" placeholder="e.g. /images/apple.jpg" />
                        <span asp-validation-for="Image" class="text-red-500 text-xs mt-1 block"></span>
                    </div>

                    <div>
                        <label asp-for="StockQty" class="block text-sm font-medium text-gray-700 mb-1"></label>
                        <input asp-for="StockQty" type="number" class="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#2e7d32] focus:border-transparent transition" placeholder="e.g. 100" />
                        <span asp-validation-for="StockQty" class="text-red-500 text-xs mt-1 block"></span>
                    </div>

                    <button type="submit" class="w-full bg-[#2e7d32] text-white font-semibold py-2.5 rounded-lg hover:bg-[#1b5e20] transition duration-200 shadow-md">
                        @(Model?.Id > 0 ? "Update Product" : "Save Product")
                    </button>

                    @if (Model?.Id > 0)
                    {
                        <a asp-action="Create" class="block text-center text-sm text-gray-500 hover:text-gray-700 font-medium transition mt-3">
                            Cancel — Add New Product
                        </a>
                    }
                </form>

                <div class="mt-6 text-center">
                    <a asp-controller="Store" asp-action="Index" class="text-sm text-gray-500 hover:text-gray-700 font-medium transition">
                        Back to Store
                    </a>
                </div>
            </div>

            <!-- Product List -->
            <div class="bg-white rounded-2xl shadow-lg p-8">
                <h3 class="text-xl font-bold text-gray-800 mb-6">Existing Products</h3>

                @if (!products.Any())
                {
                    <div class="text-center py-12">
                        <p class="text-gray-400 text-lg">No products yet.</p>
                        <p class="text-gray-400 text-sm mt-2">Add your first product using the form.</p>
                    </div>
                }
                else
                {
                    <div class="space-y-3 max-h-[600px] overflow-y-auto pr-2">
                        @foreach (var p in products)
                        {
                            <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl border border-gray-100">
                                <div class="min-w-0">
                                    <div class="font-semibold text-gray-800 truncate">@p.Name</div>
                                    <div class="text-sm text-gray-500 mt-0.5">₱@p.Price.ToString("N2") • Stock: @p.StockQty</div>
                                </div>
                                <div class="flex gap-2 ml-4 shrink-0">
                                    <a asp-action="Create" asp-route-id="@p.Id"
                                       class="px-3 py-1.5 bg-blue-50 text-blue-700 rounded-lg text-sm hover:bg-blue-100 transition font-medium">
                                        Edit
                                    </a>
                                    <button type="button" onclick="openDeleteModal(@p.Id, '@p.Name.Replace("'", "\\'")')"
                                            class="px-3 py-1.5 bg-red-50 text-red-700 rounded-lg text-sm hover:bg-red-100 transition font-medium">
                                        Delete
                                    </button>
                                </div>
                            </div>
                        }
                    </div>
                }
            </div>
        </div>
    </div>
</div>

<!-- Delete Confirmation Modal -->
<div id="deleteModal" class="fixed inset-0 bg-black/50 hidden items-center justify-center z-50 p-4">
    <div class="bg-white rounded-2xl shadow-xl max-w-sm w-full p-6 text-center">
        <div class="inline-flex items-center justify-center w-12 h-12 bg-red-100 rounded-full mb-4">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
        </div>
        <h3 class="text-lg font-bold text-gray-800 mb-2">Delete Product?</h3>
        <p class="text-gray-500 mb-6">Are you sure you want to delete <strong id="deleteProductName"></strong>?</p>

        <form id="deleteForm" asp-action="Delete" method="post" class="flex gap-3 justify-center">
            <input type="hidden" name="id" id="deleteProductId" />
            <button type="button" onclick="closeDeleteModal()" class="px-5 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition font-medium">
                Cancel
            </button>
            <button type="submit" class="px-5 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 transition font-medium">
                Delete
            </button>
        </form>
    </div>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        function openDeleteModal(id, name) {
            document.getElementById('deleteProductId').value = id;
            document.getElementById('deleteProductName').textContent = name;
            document.getElementById('deleteModal').classList.remove('hidden');
            document.getElementById('deleteModal').classList.add('flex');
        }

        function closeDeleteModal() {
            document.getElementById('deleteModal').classList.remove('flex');
            document.getElementById('deleteModal').classList.add('hidden');
        }

        // Close modal when clicking outside
        document.addEventListener('click', function(event) {
            if (event.target.id === 'deleteModal') {
                closeDeleteModal();
            }
        });
    </script>
}
```

### How It Works

- **Two-column layout:** The form is on the left, the product list is on the right (on large screens)
- **Stats cards:** Now show the real `products.Count` for "Total Products"
- **Edit button:** Links to `/Admin/Create?id=X` which loads that product into the form
- **Delete button:** Opens a confirmation modal with the product name
- **Form adapts:** Title and button text change between "Add" and "Edit" mode based on `Model.Id`
- **Hidden `Id` field:** When editing, this tells the POST action which product to update
- **Cancel link:** When editing, a link lets you switch back to "Add New Product" mode

### Important Notes

- `asp-route-id="@p.Id"` generates the URL `/Admin/Create?id=5` automatically
- The `Delete` action uses `[HttpPost]` for safety — never use `[HttpGet]` for delete operations
- `Replace("'", "\\'")` escapes single quotes in product names so the JavaScript doesn't break
- After any create, update, or delete, the controller redirects back to `Create` so the list refreshes

---

## Summary of New Changes

| File | Change |
|---|---|
| `Models/ProductCreateViewModel.cs` | Added `Id` property for edit mode |
| `Controllers/AdminController.cs` | `Create` action now handles both create and update; added `Delete` action |
| `Views/Admin/Create.cshtml` | Two-column layout with form + product list; Edit/Delete buttons; delete confirmation modal |

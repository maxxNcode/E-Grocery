# E-Grocery Project Defense Reviewer

## 1. Project Overview

**E-Grocery ni Aling Bebang** is an ASP.NET Core MVC web application that lets users browse grocery products, add them to a cart, and place orders. Admins can create, update, and delete products.

**Tech Stack:**D
- ASP.NET Core MVC
- Entity Framework Core (EF Core)
- SQL Server LocalDB
- Razor Views with Tailwind CSS

---

## 2. Architecture (MVC Pattern)

| Layer | What It Does | Our Files |
|-------|-------------|-----------|
| **Model** | Holds data classes and validation rules | `User.cs`, `Product.cs`, `CartItem.cs`, `Order.cs`, `RegisterViewModel.cs`, `LoginViewModel.cs`, `ProductCreateViewModel.cs` |
| **View** | Displays the UI (HTML) | `.cshtml` files in `Views/` folder |
| **Controller** | Handles user requests, talks to the database | `AccountController.cs`, `StoreController.cs`, `AdminController.cs` |
| **DbContext** | Bridge between our code and the database | `AppDbContext.cs` |

---

## 3. Database (EF Core)

### Connection String
```json
"Server=(localdb)\\mssqllocaldb;Database=EGroceryDB;TrustServerCertificate=True;"
```

### Tables (DbSets in `AppDbContext`)
- `Users` — stores registered accounts (with profile picture)
- `Products` — stores grocery items
- `CartItems` — stores items added to cart
- `Orders` — stores completed transactions

### Migrations
When we add a new model or change a model, we run:
```powershell
Add-Migration MigrationName
Update-Database
```
This creates the table in SQL Server automatically.

---

## 4. Session (Login Tracking)

We use `HttpContext.Session` to remember the logged-in user's email:
- On login success: `HttpContext.Session.SetString("UserEmail", user.Email)`
- To check if logged in: `HttpContext.Session.GetString("UserEmail")`
- On logout: `HttpContext.Session.Remove("UserEmail")`

This lets us show "My Account" and "Logout" instead of "Login" and "Register" in the header.

---

## 5. Account Management (Profile Page)

### How It Works:
1. User clicks **My Account** in the header
2. `AccountController.Profile()` checks session for the user's email
3. Finds the user in the database using `ToList()` + `foreach`
4. Returns `Profile.cshtml` with the user's data
5. User can edit their **Full Name** and upload a **Profile Picture**
6. On submit, `Profile` POST action:
   - Saves the new name to the database
   - If a picture was uploaded:
     - Creates a unique filename using `DateTime.Now.Ticks.ToString()`
     - Saves the file to `wwwroot/images/profiles/`
     - Stores the path (e.g., `/images/profiles/123456.jpg`) in the database
   - Calls `_context.SaveChanges()`
   - Redirects back to Profile page

### Profile Picture Upload:
- The form uses `enctype="multipart/form-data"` (required for file uploads)
- The controller accepts `IFormFile profilePicture`
- File is saved to disk using `FileStream` inside a `using` block
- Path is saved in the `User.ProfilePicture` column
- The view displays the image using `<img src="@Model.ProfilePicture" />`
- If no picture, shows a letter avatar with the user's first initial

---

## 6. Transaction (Order History)

### How Orders Are Saved:
1. When user clicks **Place Order** in the cart
2. `StoreController.PlaceOrder()` gets all cart items
3. For EACH cart item, it creates an `Order` record:
   ```csharp
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
   ```
4. Then it removes all cart items and calls `SaveChanges()`
5. Shows "Order Complete!" page

### Viewing Order History:
- `StoreController.OrderHistory()` gets the user's email from session
- Gets all orders with `ToList()`, filters with `foreach`
- Returns matching orders to `OrderHistory.cshtml`
- View shows product name, date, quantity, price, and total for each order

---

## 7. Search Bar

### How It Works:
1. User types in the search bar in the header and clicks Search
2. Form GETs to `StoreController.Search(string searchTerm)`
3. Controller gets ALL products with `ToList()`
4. Loops through and checks if `product.Name.ToLower().Contains(searchTerm.ToLower())`
5. Matching products are added to a new list
6. Returns `View("Index", filtered)` — reuses the same Index view with filtered results

**No LINQ `Where()`** — we only use `ToList()` + `foreach` as taught in the notes.

---

## 8. Account Registration Flow

### Step-by-Step:

1. **User opens `/Account/Register`**
   - Browser sends GET request to `AccountController.Register()`
   - Controller returns the `Register.cshtml` view (empty form)

2. **User fills the form and clicks "Create Account"**
   - Form POSTs to `AccountController.Register(RegisterViewModel model)`

3. **Validation triggers first (before database)**
   - `ModelState.IsValid` checks the `[Required]`, `[EmailAddress]`, `[StringLength]`, `[Compare]` rules in `RegisterViewModel`
   - If invalid → form redisplays with error messages (no database touch)
   - If valid → continues to step 4

4. **Controller creates a User object**
   ```csharp
   var user = new User
   {
       FullName = model.FullName,
       Email = model.Email,
       Password = model.Password
   };
   ```

5. **Saves to database**
   ```csharp
   _context.Users.Add(user);
   _context.SaveChanges();
   ```

6. **Redirects to Login page**
   ```csharp
   return RedirectToAction("Login");
   ```

### What If User Types a Number in the Name Field?

**Validation triggers FIRST.** The `FullName` field uses:
```csharp
[Required(ErrorMessage = "Full name is required")]
[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
```

- A single number like `1` → fails `MinimumLength = 2` → error shows
- An empty field → fails `[Required]` → error shows
- Numbers mixed with letters like `John123` → passes length check (no number-only restriction in our code) → goes to database

**Key point for defense:** Validation happens in the **ViewModel** before the controller ever touches the database.

---

## 9. Login Flow

1. **User opens `/Account/Login`** → GET returns empty form
2. **User submits email and password**
3. **Validation runs** (`[Required]`, `[EmailAddress]`)
4. If valid, controller fetches ALL users from DB:
   ```csharp
   var users = _context.Users.ToList();
   ```
5. Loops through to find a match:
   ```csharp
   foreach (var u in users)
   {
       if (u.Email == model.Email && u.Password == model.Password)
       {
           user = u;
           break;
       }
   }
   ```
6. If found → redirects to `Store/Index`
7. If not found → adds error: `ModelState.AddModelError("", "Invalid email or password")`

---

## 10. Product Management (Admin) Flow

### Create Product
1. Admin goes to `/Admin/Create`
2. Fills form (Name, Description, Price, Image URL or Image Upload, Stock)
3. Form POSTs to `AdminController.Create(ProductCreateViewModel model)`
4. `ModelState.IsValid` checks validation rules
5. If an image file was uploaded:
   - Creates a unique filename using `DateTime.Now.Ticks.ToString()`
   - Saves to `wwwroot/images/products/`
   - Uses the saved path as `ImageUrl`
6. If `model.Id == 0` → it's a NEW product
7. Creates `Product` object, calls `_context.Products.Add(product)`, then `_context.SaveChanges()`
8. Redirects back to `/Admin/Create`

### Edit Product
1. Admin clicks **Edit** on a product → goes to `/Admin/Edit/5`
2. `Edit(int id)` action finds the product by looping through `_context.Products.ToList()`
3. Copies product data into `ProductCreateViewModel`
4. Returns `View("Create", model)` — same form, but pre-filled (shows current image)
5. Admin changes data and submits
6. Since `model.Id > 0`, controller knows it's an UPDATE, not create
7. Finds existing product, updates fields, calls `_context.SaveChanges()`

### Delete Product
1. Admin clicks **Delete** → confirmation modal appears
2. Confirms → form POSTs to `AdminController.Delete(int id)`
3. Controller finds product by looping through `ToList()`
4. Calls `_context.Products.Remove(product)` and `_context.SaveChanges()`
5. Redirects back to `/Admin/Create`

### Product Image Upload:
- Admin can either **upload an image file** or type an **image URL**
- Uploaded images are saved to `wwwroot/images/products/`
- Stored path goes into `Product.ImageUrl`
- Store page and admin list display the image using `<img src="@product.ImageUrl" />`

---

## 11. Cart & Order Flow

### Add to Cart
1. User clicks product → modal opens with quantity selector
2. User picks quantity, clicks "Add to Cart"
3. Form POSTs `productId` and `quantity` to `StoreController.AddToCart()`
4. Controller:
   - Gets all products via `_context.Products.ToList()`
   - Loops to find matching `productId`
   - Gets all cart items via `_context.CartItems.ToList()`
   - Checks if product already in cart:
     - **Yes** → increases `Quantity`
     - **No** → creates new `CartItem` and `_context.CartItems.Add(...)`
   - Calls `_context.SaveChanges()`
   - Redirects to `Store/Cart`

### View Cart
1. `StoreController.Cart()` gets all items: `_context.CartItems.ToList()`
2. Passes them to `Cart.cshtml` view
3. View displays each item, calculates total using a `foreach` loop

### Place Order
1. User clicks **Place Order** on cart page
2. Form POSTs to `StoreController.PlaceOrder()`
3. Controller gets all cart items, loops through them, calls `_context.CartItems.Remove(item)` for each
4. Calls `_context.SaveChanges()` — cart is now empty
5. Returns `View("OrderComplete")` — shows "Order Complete! Your order is on the way."

---

## 12. Routing

Configured in `Program.cs`:
```csharp
app.MapControllerRoute(
    name: "Default",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "Store", action = "Index" });
```

This means:
- `/` → `StoreController.Index()`
- `/Account/Login` → `AccountController.Login()`
- `/Admin/Create` → `AdminController.Create()`
- `/Store/Cart` → `StoreController.Cart()`
- `/Admin/Edit/5` → `AdminController.Edit(id: 5)`

The `id?` means the `id` parameter is optional.

---

## 13. Key Validation Rules

| Field | Rules |
|-------|-------|
| **FullName** | `[Required]`, `[StringLength(100, MinimumLength = 2)]` |
| **Email** | `[Required]`, `[EmailAddress]` |
| **Password** | `[Required]`, `[StringLength(100, MinimumLength = 6)]` |
| **ConfirmPassword** | `[Required]`, `[Compare("Password")]` |
| **Product Name** | `[Required]`, `[StringLength(100, MinimumLength = 2)]` |
| **Price** | `[Required]`, `[Range(0.01, 9999.99)]` |
| **StockQty** | `[Required]`, `[Range(0, 10000)]` |

**Validation runs automatically** when you use `asp-validation-for` + `_ValidationScriptsPartial` in the view. The browser shows errors without reloading the page.

---

## 14. Common Defense Questions & Answers

### Q: "What is MVC?"
**A:** Model-View-Controller. Model holds data, View shows UI, Controller handles requests and connects them.

### Q: "What is Entity Framework Core?"
**A:** It's an ORM (Object-Relational Mapper). It lets us work with database tables as C# objects instead of writing raw SQL.

### Q: "What does `DbContext` do?"
**A:** It's the bridge. It tracks our objects and translates `Add()`, `Remove()`, `SaveChanges()` into SQL INSERT, DELETE, UPDATE commands.

### Q: "What is `ModelState.IsValid`?"
**A:** It checks if all validation attributes on the ViewModel passed. If false, the form shows errors and the database is never touched.

### Q: "What triggers first — validation or database?"
**A:** Validation triggers FIRST. `ModelState.IsValid` runs before any database code. Invalid data never reaches the database.

### Q: "What is `ViewData`?"
**A:** A dictionary used to pass extra data from the controller to the view. We use it to send the product list to the admin create page.

### Q: "What is the difference between `User` model and `RegisterViewModel`?"
**A:** `User` is the database entity (what gets saved). `RegisterViewModel` is for the form only — it has extra fields like `ConfirmPassword` and validation rules that don't exist in the database table.

### Q: "How does the cart work?"
**A:** Cart items are stored in the database (`CartItems` table), not in browser memory. When you add a product, it creates a `CartItem` record. When you place an order, all `CartItem` records are deleted.

### Q: "Why use `ToList()` + `foreach` instead of `Find()`?"
**A:** Our class notes teach `ToList()` to get all records and `foreach` to loop through them. It matches the patterns we learned.

---

## 15. Syntax We Use (Class Notes Compliant)

We only use patterns taught in our class notes. Here is what we use and what we avoid:

| What We Use | What We Avoid | Why |
|---|---|---|
| `ToList()` + `foreach` loops | LINQ `Where()`, `FirstOrDefault()`, `Find()` | Notes teach `ToList()` + `foreach` |
| `if (x == null \|\| x == "")` | `string.IsNullOrEmpty(x)` | Notes don't cover `IsNullOrEmpty` |
| `if (x == null)` | `x ?? y` (null-coalescing) | Notes don't cover `??` |
| `IActionResult` (synchronous) | `async Task<IActionResult>` | Notes don't cover `async/await` |
| `DateTime.Now.Ticks.ToString()` | `Guid.NewGuid()` | Notes don't cover `Guid` |
| `string` with `= string.Empty` | `string?` (nullable reference types) | Notes don't cover nullable reference types |
| `ViewData["Key"]` | `ViewContext.RouteData.Values` | Notes don't cover `ViewContext` |
| Manual null checks in views | `@Html.Raw()` or complex helpers | Keeping it simple per notes |

**Key defense point:** Every pattern we use comes directly from the notes. We do not use advanced C# features that were not taught in class.

---

## 16. File Map (Know Where Things Are)

| File | Purpose |
|------|---------|
| `Program.cs` | App startup, routing, DB connection |
| `appsettings.json` | Connection string |
| `Data/AppDbContext.cs` | DbContext with DbSets |
| `Models/User.cs` | Database model for users |
| `Models/Product.cs` | Database model for products |
| `Models/CartItem.cs` | Database model for cart |
| `Models/RegisterViewModel.cs` | Form + validation for registration |
| `Models/LoginViewModel.cs` | Form + validation for login |
| `Models/ProductCreateViewModel.cs` | Form + validation for product create/edit |
| `Controllers/AccountController.cs` | Login & Register actions |
| `Controllers/StoreController.cs` | Product listing, cart, order |
| `Controllers/AdminController.cs` | Create, Edit, Delete products |
| `Views/Account/Register.cshtml` | Registration page |
| `Views/Account/Login.cshtml` | Login page |
| `Views/Store/Index.cshtml` | Product listing page |
| `Views/Store/Cart.cshtml` | Shopping cart page |
| `Views/Store/OrderComplete.cshtml` | Order confirmation page |
| `Views/Store/OrderHistory.cshtml` | Transaction / order history |
| `Views/Admin/Create.cshtml` | Admin product form + list |
| `Views/Account/Profile.cshtml` | Account management + profile picture upload |
| `Views/Shared/_Layout.cshtml` | Master page (header, footer, search, nav) |

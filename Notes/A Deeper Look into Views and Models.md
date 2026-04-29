# A Deeper Look into Views and Models

## VIEWS IN ASP.NET MVC

## What is a View?

##### A View is responsible for displaying data to the user.

##### It contains HTML + Razor syntax and is returned by a Controller action.

##### In MVC:

##### Model → holds data

##### View → displays data

##### Controller → connects Model and View

## VIEW ENGINE AND RAZOR VIEW ENGINE

## Purpose of a View Engine

##### A View Engine :

##### Converts server-side code into HTML

##### Allows mixing C# code with HTML

##### Generates dynamic web pages

## Razor View Engine

##### The Razor View Engine is the default view engine in ASP.NET Core MVC.

##### ✔ Uses @ symbol to switch between HTML and C#

##### ✔ Clean and readable syntax

##### ✔ Supports layouts, partial views, and helpers

##### Razor files use the extension: .cshtml

## PROGRAMMING IN RAZOR VIEW ENGINE


### Basic Razor Syntax

```
<h1>@ViewData["Title"]</h1>
```
### Razor Programming Constructs

#### 1. Variables

###### @{

```
string name = "Juan";
}
<p>Hello, @name</p>
```
#### 2. Conditional Statements

```
@if (Model.Age >= 18)
{
<p>Adult</p>
}
else
{
<p>Minor</p>
}
```
#### 3. Loops

```
@foreach (var item in Model.Students)
{
<p>@item.Name</p>
}
```
#### 4. Expressions

```
<p>Total: @(Model.Price * Model.Quantity)</p>
```
## LAYOUT IN ASP.NET CORE MVC

### What is a Layout?

##### A Layout is a template page that defines the common structure of the website.

##### ✔ Header

##### ✔ Footer

##### ✔ Navigation bar

##### ✔ Shared styles and scripts

##### Layout file location: Views/Shared/_Layout.cshtml


### Features of Layouts

#### 1. @RenderBody()

##### Displays the content of individual views.

```
<body>
@RenderBody()
</body>
```
##### This is the "main event." Every layout file must have exactly one @RenderBody() call. It acts as a

##### placeholder for the content of your specific View (like Index.cshtml or Contact.cshtml).

##### How it works: When a View is rendered, the entire content of that View is injected right where

##### @RenderBody() is placed in the _Layout.cshtml.

##### Requirement: It is mandatory. If you don't include it, your application will throw an error because

##### it doesn't know where to put your page content.

#### 2. @RenderSection()

##### Used for optional sections like scripts.

```
@RenderSection("Scripts", required: false)
```
##### Sometimes, you need to inject code into the layout that isn't part of the main body—like a specific

##### JavaScript library for a gallery page or a unique CSS file for the dashboard.

##### Placement: You usually place these in the <head> (for styles) or at the very bottom of the <body>

##### (for scripts).

##### Syntax: @RenderSection("SectionName", required: false)

##### The "Required" Parameter

##### required: true : If the View doesn't define this section, the page will crash.

##### required: false : (Recommended) If the View doesn't have the section, the layout just ignores it

##### and moves on.

##### @RenderSection calls on the @section SectionName { <script> <!-- code --> </script> } in the View.

#### 3. Centralized Design

##### One layout

##### Many views


##### Easy maintenance

## HTML HELPERS

### What are HTML Helpers?

##### HTML Helpers are C# methods that generate HTML elements.

##### ✔ Reduce manual HTML coding

##### ✔ Strongly typed

##### ✔ Secure and clean syntax

### Common HTML Helpers

#### TextBox

```
@Html.TextBox("Username")
```
#### Label

```
@Html.Label("Username")
```
#### Password Box

```
@Html.Password("Password")
```
#### Form

```
@using (Html.BeginForm("Login", "Account"))
{
<input type="submit" value="Login" />
}
```
##### There are three main categories of helpers you'll encounter:

#### A. Inline Helpers

##### These are created within the Razor view itself using the @helper tag (though these are less common

##### in modern ASP.NET Core, where Tag Helpers are often preferred).

#### B. Built-in Helpers

##### These are the standard methods provided by the Html property in a view.

##### Standard Helpers: Create basic elements that aren't tied to a model (e.g., @Html.ActionLink()).


##### Strongly-Typed Helpers: Use "lambda expressions" to bind to specific model properties (e.g.,

##### @Html.TextBoxFor(m => m.FirstName)). These are preferred because they provide compile-time

##### checking.

### How to Use Them

##### To use HTML Helpers effectively, you usually want them to "talk" to a Model. Here is a comparison of

##### how they look versus standard HTML.

#### Creating a Link

##### Instead of <a href="/Home/Contact">Contact</a>, you use:

##### Razor CSHTML

```
@Html.ActionLink("Contact Us", "Contact", "Home")
```
#### Creating a Form (Strongly Typed)

##### If you have a model called User, the helper handles the name, id, and value attributes for you

##### automatically.

##### Razor CSHTML

```
@model User
```
```
@using (Html.BeginForm("Save", "User"))
{
<div>
@Html.LabelFor(m => m.UserName)
@Html.TextBoxFor(m => m.UserName)
@Html.ValidationMessageFor(m => m.UserName)
</div>
```
```
<button type="submit">Submit</button>
}
```
## PARTIAL VIEWS

### What is a Partial View?

##### A Partial View is a reusable piece of a View.

##### ✔ Used for headers, footers, menus

##### ✔ Avoids code duplication

##### Naming convention: _PartialName.cshtml


### Using a Partial View

```
@Html.Partial("_Header")
```
##### or

```
<partial name="_Header" />
```
## Create a Partial View (Header Example)

### Step 1: Go to the Shared Folder

```
Views
└── Shared
```
### Step 2: Add a Partial View

##### Create a new file:

```
_Header.cshtml
```
##### ► The underscore (_) means it is a partial view.

### Step 3: Write Simple HTML in _Header.cshtml

```
<header style="background-color:#f0f0f0; padding:10px;">
<h2>ELNET1 Sample System</h2>
<hr />
</header>
```
## Use the Partial View in a Page

### Option 1: Using Tag Helper (Recommended)

##### Open any View, for example:

```
Views/Home/Index.cshtml
```
```
<partial name="_Header" /> <h3>Welcome to Home Page</h3> <p>This is the main content.</p>
```
### Option 2: Using HTML Helper

```
@Html.Partial("_Header")
```

## Using Partial View in Layout

##### Open:

```
Views/Shared/_Layout.cshtml
```
##### Add:

```
<body>
<partial name="_Header" />
@RenderBody()
</body>
```
##### ► Now the header appears on ALL pages.

## TAG HELPERS

### What are Tag Helpers?

##### Tag Helpers:

##### Look like HTML

##### Add server-side functionality

##### Replace many HTML Helpers

##### ✔ Cleaner syntax

##### ✔ IntelliSense support

##### ✔ Easy to read

### Example: Anchor Tag Helper

```
<a asp-controller="Home" asp-action="Index">Home</a>
```
### Form Tag Helper

```
<form asp-action="Create" asp-controller="Student">
<input asp-for="Name" />
<button type="submit">Save</button>
</form>
```

### Why use Tag Helpers?

##### The primary reason is readability. Front-end developers can look at a Tag Helper and understand

##### the structure of the page without knowing C#.

##### Natural Syntax: They look like standard HTML.

##### IntelliSense: Visual Studio provides rich autocomplete for both the HTML tag and the asp-

##### attributes.

##### Cleaner Code: You don't have to use @class = "btn" workarounds to add CSS classes.

### Common Tag Helpers and How to Use Them

##### To use them, you must ensure your _ViewImports.cshtml file contains this line: @addTagHelper *,

Microsoft.AspNetCore.Mvc.TagHelpers

#### A. The Form Helper

##### Instead of @using (Html.BeginForm()), you simply use the asp-controller and asp-action attributes on a

##### standard form tag.

##### HTML

```
<form asp-controller="Account" asp-action="Login" method="post">
</form>
```
#### B. Input Helpers (Model Binding)

##### The asp-for attribute is the "magic" attribute. It links the input to a specific property in your C# Model.

##### It automatically sets the type, id, name, and even the value.

##### HTML

```
@model LoginViewModel
```
```
<label asp-for="Email"></label>
<input asp-for="Email" class="form-control" />
<span asp-validation-for="Email" class="text-danger"></span>
```
#### C. Link Helpers (Anchor)

##### Instead of hardcoding a URL (which can break if you change your routes), use the Anchor Tag

##### Helper:

##### HTML

```
<a asp-controller="Products" asp-action="Details" asp-route-id="5">View Item</a>
```

#### D. Image Helpers (Cache Busting)

##### One of the coolest utilities is asp-append-version. It adds a unique hash to your image URL. If the

##### image changes on the server, the hash changes, forcing the browser to download the new version

##### instead of using a cached one.

##### HTML

```
<img src="~/images/logo.png" asp-append-version="true" />
```
### Comparing the Old vs. New

##### If you wanted to create a styled text input for a "Username" field, here is how the evolution looks:

##### Method Syntax Style

##### Raw HTML <input type="text" name="Username" class="form-control">

##### HTML

##### Helper

```
@Html.TextBoxFor(m => m.Username, new { @class = "form-control" })
```
##### Tag Helper <input asp-for="Username" class="form-control" />

## MODELS IN ASP.NET MVC

### What is a Model?

##### A Model :

##### Represents application data

##### Contains properties and business rules

##### Communicates with the database

##### Example Model:

```
public class Student
{
public int Id { get; set; }
public string Name { get; set; }
public string Course { get; set; }
}
```
## VIEW MODELS


### What is a ViewModel?

##### A ViewModel is a model designed specifically for a View.

##### ✔ Combines multiple models

##### ✔ Contains only needed data

##### ✔ Improves security and performance

### Example ViewModel

```
public class StudentViewModel
{
public string Name { get; set; }
public string Course { get; set; }
}
```
## DATA FLOW IN ASP.NET MVC (MODEL &

## VIEWMODEL)

### MVC Data Flow

##### 1. User sends a request

##### 2. Controller processes the request

##### 3. Model fetches or processes data

##### 4. Controller sends data to View / ViewModel

##### 5. View displays data to user

##### Flow: User → View → Controller → Model → Controller → ViewModel/View

## ENTITY FRAMEWORK (EF)

### Purpose of Entity Framework

##### Entity Framework is an Object-Relational Mapper (ORM).

##### ✔ Works with databases using C#

##### ✔ No manual SQL required

##### ✔ Faster development


### Features and Benefits

##### Automatic table mapping

##### Language Integrated Query (LINQ) queries

##### Create, read, update and delete (CRUD) operations

##### Database migrations

##### Security against Structured Query Language (SQL) Injection

## CRUD OPERATIONS USING ENTITY FRAMEWORK

### 1. Adding Data

```
_context.Students.Add(student);
_context.SaveChanges();
```
### 2. Updating Data

```
_context.Students.Update(student);
_context.SaveChanges();
```
### 3. Deleting Data

```
_context.Students.Remove(student);
_context.SaveChanges();
```
### 4. Retrieving Data

```
var students = _context.Students.ToList();
```
## USING ENTITY FRAMEWORK IN ASP.NET MVC

### Steps:

##### 1. Install Entity Framework packages


##### A. EntityFrameworkCore

##### B. EntityFrameworkCore.SqlServer

##### C. EntityFrameworkCore.Tools

##### 2. Create Model classes

##### 3. Create DbContext

##### 4. Configure database connection

##### 5. Use EF in Controllers

### Example DbContext

```
public class AppDbContext : DbContext
{
public DbSet<Student> Students { get; set; }
}
```
## Create a Model (Student)

### Step 1: Create a Model Class

##### Folder:

```
Models
```
##### File:

```
Student.cs
```
```
public class Student
{
public int Id { get; set; }
public string Name { get; set; }
public string Course { get; set; }
}
```
## Create the DbContext

### Step 2: Create a Data Folder

```
Data
```
### Step 3: Create DbContext Class

##### File:


```
AppDbContext.cs
```
```
using Microsoft.EntityFrameworkCore;
using YourProjectName.Models;
```
```
public class AppDbContext : DbContext
{
public AppDbContext(DbContextOptions<AppDbContext> options)
: base(options)
{
}
```
```
public DbSet<Student> Students { get; set; }
}
```
##### ► DbSet<Student> represents the Students table.

## Register DbContext (Program.cs)

##### Open:

```
Program.cs
```
##### Add:

```
using Microsoft.EntityFrameworkCore;
using YourProjectName.Data;
```
##### Inside builder.Services:

```
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));
```
## Add Connection String

##### Open:

```
appsettings.json
```
```
"ConnectionStrings": {
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ELNET1DB;TrustServerCertificate=True;"
}
```
##### ► This uses a database named ELNET1DB.


```
Create the Database
```
##### Open:

```
Tools -> NuGet Package Manager -> Package Manager Console
```
##### Enter these in order:

##### 1. Add-Migration InitialCreate

##### 2. Update-Database

##### Enter these in order when changing details in a model:

##### 1. Add-Migration InitialCreate

##### 2. Add-Migration SyncPendingChanges

##### 3. Update-Database

## Use DbContext in a Controller

### Step 4: Inject DbContext

##### Example:

```
Controllers/StudentController.cs
```
```
using YourProjectName.Data;
using YourProjectName.Models;
```
```
public class StudentController : Controller
{
private readonly AppDbContext _context;
```
```
public StudentController(AppDbContext context)
{
_context = context;
}
```
```
public IActionResult Index()
{
var students = _context.Students.ToList();
return View(students);
}
[HttpGet]
public IActionResult Create()
{
return View();
}
```
```
[HttpPost]
```

```
[ValidateAntiForgeryToken]
public IActionResult Create(Student student)
{
if (ModelState.IsValid)
{
_context.Students.Add(student); // Stage the data
_context.SaveChanges(); // Push to SQL Server
return RedirectToAction("Index");
}
return View(student); // If something goes wrong, remain in the page
}
```
###### }

## Send Data to the Database

##### Create:

```
Views/Student/Create.cshtml
```
```
@{
ViewData["Title"] = "Create";
}
```
```
@model Student
```
```
<h2>Add New Student</h2>
```
```
<form asp-action="Create" method="post">
<div>
<label>Name:</label>
<input asp-for="Name" class="form-control" />
</div>
```
```
<div>
<label>Course:</label>
<input asp-for="Course" class="form-control" />
</div>
```
```
<button type="submit">Save Student</button>
</form>
```
```
<a asp-action="Index">Back to List</a>
```
## Display Data in View

##### Create:

```
Views/Student/Index.cshtml
```
```
@{
ViewData["Title"] = "Index";
}
```
```
@model List<Student>
```

```
<h2>Student List</h2>
```
```
<p>
<a asp-action="Create">Add New Student</a>
</p>
```
```
<ul>
@foreach (var s in Model)
{
<li>@s.Name - @s.Course</li>
}
</ul>
```
### Partial View Flow:

```
Layout / View
↓
Partial View
↓
Reusable UI Component
```
### Entity Framework Flow:

```
Controller
↓
DbContext
↓
Database
↓
Model
↓
View
```
## SUMMARY

##### 1. Views handle presentation

##### 2. Razor mixes HTML and C#

##### 3. Layouts provide consistent design

##### 4. HTML Helpers and Tag Helpers simplify coding

##### 5. Models store data

##### 6. ViewModels shape data for Views

##### 7. Entity Framework manages database operations



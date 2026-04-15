# a2n.DynData

**Turn your Entity Framework Core models into a fully functional REST API — instantly.**

No hand-written controllers. No repetitive CRUD boilerplate. Just register your `DbContext`, and DynData generates every endpoint you need: data tables with server-side paging, advanced filtering, CRUD operations, dropdown lookups, and CSV/Excel exports — all out of the box.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0%20%7C%2010.0-purple)](https://dotnet.microsoft.com/)

---

## Why DynData?

Building admin panels, back-office tools, or data-driven dashboards in ASP.NET Core usually means writing the same controller actions over and over for every entity. DynData eliminates that entirely.

With a single line of service registration, every `DbSet<>` in your `DbContext` becomes a queryable, sortable, filterable, exportable API endpoint — complete with a JavaScript client library that plugs directly into [DataTables.js](https://datatables.net/) and [jQuery QueryBuilder](https://querybuilder.js.org/).

### What you get

- **Automatic REST API** from any EF Core `DbContext` — zero controller code required
- **Server-side DataTables** with paging, sorting, multi-column search, and global search
- **Advanced filtering** via jQuery QueryBuilder integration (AND/OR logic, nested groups)
- **Full CRUD** — Create, Read, Update, Delete endpoints for every entity
- **Data export** to CSV and Excel (.xlsx) with no external dependencies
- **Dropdown/lookup endpoints** with server-side search for foreign key relationships
- **Custom query templates** for exposing joins, projections, and computed views alongside raw tables
- **Per-endpoint authorization** through a clean `IDynDataAPIAuth` interface
- **Lifecycle hooks** — intercept creates, updates, deletes, metadata generation, and exports
- **Multi-database support** — SQL Server, PostgreSQL, MySQL/MariaDB, and SQLite

---

## Supported Platforms

| Framework | Status |
|-----------|--------|
| .NET 10.0 | ✅ Supported |
| .NET 9.0  | ✅ Supported |
| .NET 8.0  | ✅ Supported |

---

## Installation

### 1. Add the project reference

Reference the `a2n.DynData` project in your web application:

```xml
<ProjectReference Include="path\to\a2n.DynData\a2n.DynData.csproj" />
```

### 2. Make your DbContext inherit from `DynDbContext`

```csharp
using a2n.DynData;
using Microsoft.EntityFrameworkCore;

public partial class MyAppDbContext : DynDbContext
{
    public MyAppDbContext() { }

    public MyAppDbContext(DbContextOptions<MyAppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    // ... all your entities
}
```

### 3. Register DynData in `Program.cs`

```csharp
using a2n.DynData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure your DbContext with DynData
builder.Services.AddDbContext<MyAppDbContext>(options =>
{
    options.UseDynData(new DatabaseServer
    {
        Provider = DatabaseProvider.Sqlite,
        ConnectionString = "Data Source=myapp.db"
    });
});

// Register the DynData API — "db" becomes the controller name
builder.Services.AddDynDataApi<MyAppDbContext>("db");

// Important: use Newtonsoft.Json with member casing
builder.Services.AddRazorPages()
    .AddNewtonsoftJson(x => x.UseMemberCasing());

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.Run();
```

That's it. Every `DbSet<>` in your context is now available as a REST API at `/dyndata/db/{EntityName}/{action}`.

### 4. Include the JavaScript libraries (optional — for UI integration)

Copy `a2n.js` and `a2n-DynData.js` from the [`a2n Javascripts/`](a2n%20Javascripts/) folder into your web project's static files, then reference them along with the required dependencies:

```html
<!-- Required dependencies -->
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
<script src="~/lib/datatables/jquery.dataTables.min.js"></script>
<script src="~/lib/moment/moment.min.js"></script>

<!-- DynData client libraries -->
<script src="~/lib/a2n/a2n.js"></script>
<script src="~/lib/a2n/a2n-DynData.js"></script>
```

---

## Usage

### API Endpoints

Once registered, the following endpoints are automatically available for every entity:

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/dyndata/{controller}/viewNames` | GET/POST | List all available entity names |
| `/dyndata/{controller}/{entity}/datatable` | POST | Server-side DataTables data (paging, sorting, filtering) |
| `/dyndata/{controller}/{entity}/datatable/export` | POST | Export filtered DataTable to CSV or Excel |
| `/dyndata/{controller}/{entity}/list` | POST | Paginated list with expression-based filtering |
| `/dyndata/{controller}/{entity}/dropdown` | GET | Dropdown data with server-side search |
| `/dyndata/{controller}/{entity}/read` | POST | Read a single record by primary key |
| `/dyndata/{controller}/{entity}/create` | POST | Create one or more records |
| `/dyndata/{controller}/{entity}/update` | POST | Update one or more records |
| `/dyndata/{controller}/{entity}/delete` | POST | Delete one or more records |
| `/dyndata/{controller}/{entity}/metadata` | GET/POST | Entity metadata (field names, types, keys) |
| `/dyndata/{controller}/{entity}/metadataQB` | GET/POST | Metadata with jQuery QueryBuilder filter options |

### Database Providers

DynData supports multiple database providers through the `DatabaseServer` configuration:

```csharp
// SQL Server
new DatabaseServer
{
    Provider = DatabaseProvider.SqlServer,
    ConnectionString = "Server=.;Database=MyDb;Trusted_Connection=True;",
    DefaultSchema = "dbo"
}

// PostgreSQL
new DatabaseServer
{
    Provider = DatabaseProvider.Postgres,
    ConnectionString = "Host=localhost;Database=mydb;Username=user;Password=pass"
}

// MySQL / MariaDB
new DatabaseServer
{
    Provider = DatabaseProvider.MySql,
    ConnectionString = "Server=localhost;Database=mydb;User=user;Password=pass"
}

// SQLite
new DatabaseServer
{
    Provider = DatabaseProvider.Sqlite,
    ConnectionString = "Data Source=myapp.db"
}
```

### Custom Query Templates

Beyond raw table access, you can expose custom queries (joins, projections, computed views) as named endpoints using `QueryTemplate<T>`:

```csharp
public class MyQueryTemplate : QueryTemplate<MyAppDbContext>
{
    public MyQueryTemplate()
    {
        AddQuery("vProductCategory", typeof(Product), (db, provider) =>
        {
            return from p in db.Products
                   join c in db.Categories on p.CategoryId equals c.CategoryId
                   join s in db.Suppliers on p.SupplierId equals s.SupplierId
                   select new
                   {
                       p.ProductId,
                       p.CategoryId,
                       c.CategoryName,
                       p.ProductName,
                       p.UnitPrice,
                       p.UnitsInStock,
                       p.SupplierId,
                       SupplierName = s.CompanyName
                   };
        },
        meta =>
        {
            // Customize metadata per field
            if (meta.FieldName == "ProductId")
            {
                meta.IsPrimaryKey = true;
                meta.CustomAttributes = new { Hidden = true };
            }
        });
    }
}
```

Register it alongside your DbContext:

```csharp
builder.Services.AddDynDataApi<MyAppDbContext, MyQueryTemplate>("db");
```

The custom query `vProductCategory` is now accessible at `/dyndata/db/vProductCategory/datatable` (and all other endpoints), alongside your regular entities.

### Authorization

For per-endpoint, per-entity access control, implement `IDynDataAPIAuth`:

```csharp
public class MyApiAuth : IDynDataAPIAuth
{
    public bool IsAllowed(HttpContext context, string controllerName,
        DynDbContext db, DynDataAPIMethod method, string viewName)
    {
        // Your authorization logic here
        var user = context.User;
        if (method == DynDataAPIMethod.Delete && !user.IsInRole("Admin"))
            return false;
        return true;
    }

    public void ApplyRequest(HttpContext context, DynDbContext db,
        string viewName, DataTableJSRequest req)
    {
        // Optionally modify requests based on user context
        // e.g., add row-level security filters
    }

    public void ApplyRequest(HttpContext context, DynDbContext db,
        string viewName, DataTableJSExportRequest req)
    {
        // Same for export requests
    }
}
```

Register with the three-type-parameter overload:

```csharp
builder.Services.AddDynDataApi<MyApiAuth, MyAppDbContext, MyQueryTemplate>("db");

// In the middleware pipeline, register the auth handler
app.RegisterDynDataServiceAPIAuth<MyApiAuth, MyAppDbContext, MyQueryTemplate>("db");
```

### Event Handlers

Intercept CRUD operations and customize behavior by extending `DynDbContextEventHandler`:

```csharp
public class MyEventHandler : DynDbContextEventHandler
{
    public override bool OnBeforeCreate(DynDbContext db, Type valueType, object value)
    {
        // Return false to cancel the create operation
        return true;
    }

    public override bool OnBeforeUpdate(DynDbContext db, Type valueType,
        object originalValue, JObject valueToModified)
    {
        return true;
    }

    public override bool OnBeforeDelete(DynDbContext db, Type valueType, object value)
    {
        return true;
    }

    public override void OnMetaGenerated(Metadata meta)
    {
        // Customize metadata for all entities
    }

    public override void OnExport(string format, string viewName,
        Type valueType, Metadata[] metadataArr,
        IQueryable<dynamic> qry,
        out byte[] buffer, out string mimeType, out string fileName)
    {
        // Custom export logic, or fall back to default
        DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry,
            out buffer, out mimeType, out fileName);
    }
}
```

Pass it when configuring your DbContext:

```csharp
builder.Services.AddDbContext<MyAppDbContext>(options =>
{
    options.UseDynData(dbSettings, new MyEventHandler());
});
```

### JavaScript Client — DataTables Integration

Render a fully interactive, server-side DataTable with a few lines of JavaScript:

```javascript
var dt = new a2n.dyndata.DataTable("myTable", "#tableContainer", "db", "Product", {
    allowCreate: true,
    allowUpdate: true,
    allowDelete: true,
    allowExport: true,
    enableQueryBuilder: true
});
dt.LoadMetadata(function () {
    dt.Render();
});
```

This creates a complete data grid with:
- Server-side pagination and sorting
- Global search
- Advanced filtering via jQuery QueryBuilder
- Inline Create / Edit / Delete modals
- CSV and Excel export buttons

---

## Examples

The repository includes two working example applications:

### SQLite — Northwind Database

A lightweight example using the classic Northwind database with SQLite. Demonstrates basic setup, custom query templates, and the JavaScript DataTable integration.

📂 [`Examples/Northwind.DataAccess/`](Examples/Northwind.DataAccess/) — EF Core entities and DbContext  
📂 [`Examples/Northwind.WebUI/`](Examples/Northwind.WebUI/) — ASP.NET Core web application  
📄 [`Examples/Northwind.WebUI/Program.cs`](Examples/Northwind.WebUI/Program.cs) — Registration and startup  
📄 [`Examples/Northwind.WebUI/Configuration/NorthwindQueryTemplate.cs`](Examples/Northwind.WebUI/Configuration/NorthwindQueryTemplate.cs) — Custom query template example

### SQL Server — AdventureWorks Database

A more comprehensive example using the AdventureWorks database. Demonstrates event handlers, authorization patterns, and advanced configuration.

📂 [`Examples/AdvWorks.DataAccess/`](Examples/AdvWorks.DataAccess/) — EF Core entities and DbContext  
📂 [`Examples/AdvWorks.WebUI/`](Examples/AdvWorks.WebUI/) — ASP.NET Core web application  
📄 [`Examples/AdvWorks.WebUI/Startup.cs`](Examples/AdvWorks.WebUI/Startup.cs) — Registration with event handlers and auth

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Your ASP.NET Core App                   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  AddDynDataApi<TDbContext, TTemplate, TApiAuth>("name")     │
│       │                                                     │
│       ▼                                                     │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  DynDataController (auto-generated generic controller)│  │
│  │  Routes: /dyndata/{name}/{entity}/{action}            │  │
│  └───────────────────────┬───────────────────────────────┘  │
│                          │                                  │
│       ┌──────────────────┼──────────────────┐               │
│       ▼                  ▼                  ▼               │
│  ┌──────────┐   ┌──────────────┐   ┌──────────────┐        │
│  │DynDbContext│   │QueryTemplate │   │IDynDataAPIAuth│       │
│  │(your EF   │   │(custom joins │   │(per-endpoint  │       │
│  │ DbContext) │   │ & views)     │   │ authorization)│       │
│  └─────┬─────┘   └──────────────┘   └──────────────┘       │
│        │                                                    │
│        ▼                                                    │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  ExpressionBuilder + ExternalFilterParser              │  │
│  │  (dynamic LINQ expression trees from filter rules)    │  │
│  └───────────────────────┬───────────────────────────────┘  │
│                          │                                  │
│                          ▼                                  │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Entity Framework Core                                │  │
│  │  SQL Server │ PostgreSQL │ MySQL │ SQLite              │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ▲
                           │ HTTP / JSON
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                     Browser / Client                        │
│                                                             │
│  a2n-DynData.js ──► DataTables.js (server-side processing) │
│  a2n.js ──────────► jQuery (AJAX, forms, dialogs)          │
│                  ──► jQuery QueryBuilder (advanced filters) │
│                  ──► Bootstrap (UI framework)               │
└─────────────────────────────────────────────────────────────┘
```

---

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).

© 2021–2026 Anwar Minarso

---

## Links

- **Repository**: [https://github.com/anwarminarso/DynData](https://github.com/anwarminarso/DynData)

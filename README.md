# DynData for ASP.NET Core Razor Pages

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

DynData is a powerful backend library for ASP.NET Core Razor Pages designed to dramatically accelerate the development of data-driven web applications. It automates the creation of robust, feature-rich data grids and forms by dynamically generating APIs from your Entity Framework Core `DbContext`.

Built for modern .NET, DynData supports **.NET 8, .NET 9, and .NET 10**, providing a seamless bridge between your data models and a dynamic frontend powered by jQuery DataTables.

![DynData Demo](https://raw.githubusercontent.com/anwarminarso/DynData/main/docs/images/dyndata-demo.gif)

## Features

- **Automatic API Generation**: Creates RESTful APIs for CRUD operations (Create, Read, Update, Delete), server-side paging, sorting, and filtering directly from your EF Core models.
- **Rich Data Grids**: Effortlessly renders data grids using the popular [jQuery DataTables](https://datatables.net/) library, with full support for:
    - Server-side processing for handling large datasets efficiently.
    - Global and column-specific searching.
    - Multi-column sorting.
    - An advanced query builder for complex filtering logic.
- **Dynamic Form Generation**: Automatically generates data entry forms for create and edit operations based on your EF model metadata, including support for foreign key relationships (dropdowns).
- **Custom Query Templates**: Define complex, read-only views using LINQ, including joins (inner, left), projections to anonymous types, and custom logic. DynData exposes these as if they were database views.
- **Data Export**: Built-in support for exporting grid data to **CSV** and **Excel** formats. The export functionality is fully extensible.
- **Broad Database Support**: Works seamlessly with major databases:
    - **SQL Server**
    - **PostgreSQL**
    - **MySQL**
    - **SQLite**
- **Flexible & Extensible**:
    - Customize authorization rules for each API endpoint.
    - Override default behavior with custom event handlers.
    - Integrate with your existing Razor Pages projects with minimal configuration.

## How It Works

DynData inspects your `DbContext` and exposes its `DbSet` properties through a dedicated controller. The frontend JavaScript library (`a2n-DynData.js`) communicates with this controller to fetch metadata and data, dynamically building the UI.

1.  **Setup**: Register your `DbContext` and the DynData services in `Program.cs`.
2.  **API Controller**: A generic controller handles all incoming requests for data, metadata, and CRUD operations.
3.  **Razor Page**: In your Razor Page, a few lines of JavaScript are all you need to initialize a data grid for any table or view defined in your `DbContext`.
4.  **Dynamic UI**: The library fetches metadata to construct the table columns and generates forms on-the-fly for editing or creating records.

This approach eliminates the need to write boilerplate code for each data grid, saving you significant development time.

## Getting Started

### Prerequisites

- .NET SDK (8.0, 9.0, or 10.0)
- An existing ASP.NET Core Razor Pages project.
- An Entity Framework Core `DbContext`.

### 1. Installation

Add the DynData library to your project.
*(Assuming it will be packaged as a NuGet package)*
Coming soon: NuGet installation
dotnet add package a2n.DynData


For now, include the `a2n.DynData` project in your solution.

### 2. Configure Services

In your `Program.cs` file, configure the services for DynData.
```
using Northwind.DataAccess;
using Northwind.WebUI.Configuration;
using a2n.DynData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// 1. Add Razor Pages and configure JSON options for DynData
    builder.Services.AddRazorPages()
      .AddNewtonsoftJson(options => { options.UseMemberCasing(); // Required for proper property name mapping });
// 2. Add your DbContext with the UseDynData extension
   builder.Services.AddDbContext<NorthwindDbContext>(o => {
     // Assumes connection string is in appsettings.json
     var dbSetting = builder.Configuration.GetSection("DBConnectionSetting").Get<DatabaseServer>();
     o.UseDynData(dbSetting);
   });
// 3. Register the DynData API controller //    "db" is the route prefix, e.g., /dyndata/db/... builder.Services.AddDynDataApi<NorthwindDbContext, NorthwindQueryTemplate>("db");
var app = builder.Build();
// ... standard middleware configuration ...
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers(); // Important: This maps the DynData API controller
app.Run();
```

### 3. Include Frontend Assets

In your Razor page (e.g., `Index.cshtml`) or layout file (`_Layout.cshtml`), include the required JavaScript and CSS files.
```
@section Scripts {
<!-- Dependencies (jQuery, DataTables, etc.) -->
<script src="~/lib/jquery/dist/jquery.min.js"></script> <script type="text/javascript" src="https://cdn.datatables.net/v/bs4/dt-1.11.3/datatables.min.js">
</script> <script type="text/javascript" src="~/lib/jquery-querybuilder/js/query-builder.standalone.min.js"></script>
<!-- DynData Scripts -->
<script type="text/javascript" src="~/lib/a2n/a2n.js" asp-append-version="true"></script>
<script type="text/javascript" src="~/lib/a2n/a2n-DynData.js" asp-append-version="true"></script>

<script type="text/javascript">
    $(function() {
        // Your grid initialization code here
    });
</script>
}
```
### 4. Initialize the Data Grid

To display a grid for a table (e.g., "Products"), initialize the `a2n.dyndata.DataTable` object.
```
<script type="text/javascript">
$(function() {
    let $el = $('#tblContainer');
    let viewName = 'Products'; // The name of the DbSet in your DbContext
    let dt = new a2n.dyndata.DataTable(viewName, $el, 'db', viewName, { 
        dynOptions: {
            allowCreate: true,
            allowView: true,
            allowUpdate: true,
            allowDelete: true,
            allowExport: true,
            enableQueryBuilder: true
        },
        tableOptions: { 
            responsive: true,
            lengthMenu: [10, 25, 50, 100]
        }
    });
    dt.Render();
});
</script>
```

That's it! A fully functional data grid for your `Products` table will be rendered, complete with server-side paging, sorting, filtering, and CRUD operations.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

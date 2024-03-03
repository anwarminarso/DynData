using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class ProductsByCategory
{
    public string CategoryName { get; set; }

    public string ProductName { get; set; }

    public string QuantityPerUnit { get; set; }

    public int? UnitsInStock { get; set; }

    public string Discontinued { get; set; }
}

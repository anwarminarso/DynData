using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class SalesByCategory
{
    public int? CategoryId { get; set; }

    public string CategoryName { get; set; }

    public string ProductName { get; set; }

    public byte[] ProductSales { get; set; }
}

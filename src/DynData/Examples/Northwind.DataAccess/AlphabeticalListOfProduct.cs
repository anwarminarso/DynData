using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class AlphabeticalListOfProduct
{
    public int? ProductId { get; set; }

    public string ProductName { get; set; }

    public int? SupplierId { get; set; }

    public int? CategoryId { get; set; }

    public string QuantityPerUnit { get; set; }

    public double? UnitPrice { get; set; }

    public int? UnitsInStock { get; set; }

    public int? UnitsOnOrder { get; set; }

    public int? ReorderLevel { get; set; }

    public string Discontinued { get; set; }

    public string CategoryName { get; set; }
}

using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class OrderSubtotal
{
    public int? OrderId { get; set; }

    public double? Subtotal { get; set; }
}

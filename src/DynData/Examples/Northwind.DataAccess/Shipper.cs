using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class Shipper
{
    public int ShipperId { get; set; }

    public string CompanyName { get; set; }

    public string Phone { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

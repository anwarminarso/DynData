using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; }

    public string Description { get; set; }

    public byte[] Picture { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

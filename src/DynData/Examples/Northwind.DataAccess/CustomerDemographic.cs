using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class CustomerDemographic
{
    public string CustomerTypeId { get; set; }

    public string CustomerDesc { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}

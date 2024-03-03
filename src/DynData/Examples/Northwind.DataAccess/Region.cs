using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class Region
{
    public int RegionId { get; set; }

    public string RegionDescription { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}

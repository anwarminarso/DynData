using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Stores (including store addresses) that sell Adventure Works Cycles products to consumers.
    /// </summary>
    public partial class VStoreWithAddress
    {
        public int BusinessEntityId { get; set; }
        public string Name { get; set; } = null!;
        public string AddressType { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string StateProvinceName { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string CountryRegionName { get; set; } = null!;
    }
}

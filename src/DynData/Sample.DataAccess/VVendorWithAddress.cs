﻿using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Vendor (company) names and addresses .
    /// </summary>
    public partial class VVendorWithAddress
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

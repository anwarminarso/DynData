﻿using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Stores (including demographics) that sell Adventure Works Cycles products to consumers.
    /// </summary>
    public partial class VStoreWithDemographic
    {
        public int BusinessEntityId { get; set; }
        public string Name { get; set; } = null!;
        public decimal? AnnualSales { get; set; }
        public decimal? AnnualRevenue { get; set; }
        public string? BankName { get; set; }
        public string? BusinessType { get; set; }
        public int? YearOpened { get; set; }
        public string? Specialty { get; set; }
        public int? SquareFeet { get; set; }
        public string? Brands { get; set; }
        public string? Internet { get; set; }
        public int? NumberEmployees { get; set; }
    }
}

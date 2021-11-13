﻿using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Companies from whom Adventure Works Cycles purchases parts or other goods.
    /// </summary>
    public partial class Vendor
    {
        public Vendor()
        {
            ProductVendors = new HashSet<ProductVendor>();
            PurchaseOrderHeaders = new HashSet<PurchaseOrderHeader>();
        }

        /// <summary>
        /// Primary key for Vendor records.  Foreign key to BusinessEntity.BusinessEntityID
        /// </summary>
        public int BusinessEntityId { get; set; }
        /// <summary>
        /// Vendor account (identification) number.
        /// </summary>
        public string AccountNumber { get; set; } = null!;
        /// <summary>
        /// Company name.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// 1 = Superior, 2 = Excellent, 3 = Above average, 4 = Average, 5 = Below average
        /// </summary>
        public byte CreditRating { get; set; }
        /// <summary>
        /// 0 = Do not use if another vendor is available. 1 = Preferred over other vendors supplying the same product.
        /// </summary>
        public bool? PreferredVendorStatus { get; set; }
        /// <summary>
        /// 0 = Vendor no longer used. 1 = Vendor is actively used.
        /// </summary>
        public bool? ActiveFlag { get; set; }
        /// <summary>
        /// Vendor URL.
        /// </summary>
        public string? PurchasingWebServiceUrl { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual BusinessEntity BusinessEntity { get; set; } = null!;
        public virtual ICollection<ProductVendor> ProductVendors { get; set; }
        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
    }
}

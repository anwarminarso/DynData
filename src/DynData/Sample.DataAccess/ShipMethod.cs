﻿using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Shipping company lookup table.
    /// </summary>
    public partial class ShipMethod
    {
        public ShipMethod()
        {
            PurchaseOrderHeaders = new HashSet<PurchaseOrderHeader>();
            SalesOrderHeaders = new HashSet<SalesOrderHeader>();
        }

        /// <summary>
        /// Primary key for ShipMethod records.
        /// </summary>
        public int ShipMethodId { get; set; }
        /// <summary>
        /// Shipping company name.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Minimum shipping charge.
        /// </summary>
        public decimal ShipBase { get; set; }
        /// <summary>
        /// Shipping charge per pound.
        /// </summary>
        public decimal ShipRate { get; set; }
        /// <summary>
        /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
        /// </summary>
        public Guid Rowguid { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    }
}

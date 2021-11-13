﻿using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Types of addresses stored in the Address table. 
    /// </summary>
    public partial class AddressType
    {
        public AddressType()
        {
            BusinessEntityAddresses = new HashSet<BusinessEntityAddress>();
        }

        /// <summary>
        /// Primary key for AddressType records.
        /// </summary>
        public int AddressTypeId { get; set; }
        /// <summary>
        /// Address type description. For example, Billing, Home, or Shipping.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
        /// </summary>
        public Guid Rowguid { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<BusinessEntityAddress> BusinessEntityAddresses { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// High-level product categorization.
    /// </summary>
    public partial class ProductCategory
    {
        public ProductCategory()
        {
            ProductSubcategories = new HashSet<ProductSubcategory>();
        }

        /// <summary>
        /// Primary key for ProductCategory records.
        /// </summary>
        public int ProductCategoryId { get; set; }
        /// <summary>
        /// Category description.
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

        public virtual ICollection<ProductSubcategory> ProductSubcategories { get; set; }
    }
}

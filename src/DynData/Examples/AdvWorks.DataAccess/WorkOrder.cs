﻿using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Manufacturing work orders.
    /// </summary>
    public partial class WorkOrder
    {
        public WorkOrder()
        {
            WorkOrderRoutings = new HashSet<WorkOrderRouting>();
        }

        /// <summary>
        /// Primary key for WorkOrder records.
        /// </summary>
        public int WorkOrderId { get; set; }
        /// <summary>
        /// Product identification number. Foreign key to Product.ProductID.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Product quantity to build.
        /// </summary>
        public int OrderQty { get; set; }
        /// <summary>
        /// Quantity built and put in inventory.
        /// </summary>
        public int StockedQty { get; set; }
        /// <summary>
        /// Quantity that failed inspection.
        /// </summary>
        public short ScrappedQty { get; set; }
        /// <summary>
        /// Work order start date.
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Work order end date.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Work order due date.
        /// </summary>
        public DateTime DueDate { get; set; }
        /// <summary>
        /// Reason for inspection failure.
        /// </summary>
        public short? ScrapReasonId { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual ScrapReason? ScrapReason { get; set; }
        public virtual ICollection<WorkOrderRouting> WorkOrderRoutings { get; set; }
    }
}

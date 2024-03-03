﻿using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Work shift lookup table.
    /// </summary>
    public partial class Shift
    {
        public Shift()
        {
            EmployeeDepartmentHistories = new HashSet<EmployeeDepartmentHistory>();
        }

        /// <summary>
        /// Primary key for Shift records.
        /// </summary>
        public byte ShiftId { get; set; }
        /// <summary>
        /// Shift description.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Shift start time.
        /// </summary>
        public TimeSpan StartTime { get; set; }
        /// <summary>
        /// Shift end time.
        /// </summary>
        public TimeSpan EndTime { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; }
    }
}

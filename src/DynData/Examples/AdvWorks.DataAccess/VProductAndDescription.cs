using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Product names and descriptions. Product descriptions are provided in multiple languages.
    /// </summary>
    public partial class VProductAndDescription
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ProductModel { get; set; } = null!;
        public string CultureId { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}

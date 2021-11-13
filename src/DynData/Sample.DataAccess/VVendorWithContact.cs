using System;
using System.Collections.Generic;

namespace Sample.DataAccess
{
    /// <summary>
    /// Vendor (company) names  and the names of vendor employees to contact.
    /// </summary>
    public partial class VVendorWithContact
    {
        public int BusinessEntityId { get; set; }
        public string Name { get; set; } = null!;
        public string ContactType { get; set; } = null!;
        public string? Title { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? Suffix { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneNumberType { get; set; }
        public string? EmailAddress { get; set; }
        public int EmailPromotion { get; set; }
    }
}

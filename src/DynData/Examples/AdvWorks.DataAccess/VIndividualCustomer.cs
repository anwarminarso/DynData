﻿using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Individual customers (names and addresses) that purchase Adventure Works Cycles products online.
    /// </summary>
    public partial class VIndividualCustomer
    {
        public int BusinessEntityId { get; set; }
        public string? Title { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public string? Suffix { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneNumberType { get; set; }
        public string? EmailAddress { get; set; }
        public int EmailPromotion { get; set; }
        public string AddressType { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = null!;
        public string StateProvinceName { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string CountryRegionName { get; set; } = null!;
        public string? Demographics { get; set; }
    }
}

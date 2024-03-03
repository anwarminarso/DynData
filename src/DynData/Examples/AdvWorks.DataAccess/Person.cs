﻿using System;
using System.Collections.Generic;

namespace AdvWorks.DataAccess
{
    /// <summary>
    /// Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.
    /// </summary>
    public partial class Person
    {
        public Person()
        {
            BusinessEntityContacts = new HashSet<BusinessEntityContact>();
            Customers = new HashSet<Customer>();
            EmailAddresses = new HashSet<EmailAddress>();
            PersonCreditCards = new HashSet<PersonCreditCard>();
            PersonPhones = new HashSet<PersonPhone>();
        }

        /// <summary>
        /// Primary key for Person records.
        /// </summary>
        public int BusinessEntityId { get; set; }
        /// <summary>
        /// Primary type of person: SC = Store Contact, IN = Individual (retail) customer, SP = Sales person, EM = Employee (non-sales), VC = Vendor contact, GC = General contact
        /// </summary>
        public string PersonType { get; set; } = null!;
        /// <summary>
        /// 0 = The data in FirstName and LastName are stored in western style (first name, last name) order.  1 = Eastern style (last name, first name) order.
        /// </summary>
        public bool NameStyle { get; set; }
        /// <summary>
        /// A courtesy title. For example, Mr. or Ms.
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// First name of the person.
        /// </summary>
        public string FirstName { get; set; } = null!;
        /// <summary>
        /// Middle name or middle initial of the person.
        /// </summary>
        public string? MiddleName { get; set; }
        /// <summary>
        /// Last name of the person.
        /// </summary>
        public string LastName { get; set; } = null!;
        /// <summary>
        /// Surname suffix. For example, Sr. or Jr.
        /// </summary>
        public string? Suffix { get; set; }
        /// <summary>
        /// 0 = Contact does not wish to receive e-mail promotions, 1 = Contact does wish to receive e-mail promotions from AdventureWorks, 2 = Contact does wish to receive e-mail promotions from AdventureWorks and selected partners. 
        /// </summary>
        public int EmailPromotion { get; set; }
        /// <summary>
        /// Additional contact information about the person stored in xml format. 
        /// </summary>
        public string? AdditionalContactInfo { get; set; }
        /// <summary>
        /// Personal information such as hobbies, and income collected from online shoppers. Used for sales analysis.
        /// </summary>
        public string? Demographics { get; set; }
        /// <summary>
        /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
        /// </summary>
        public Guid Rowguid { get; set; }
        /// <summary>
        /// Date and time the record was last updated.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        public virtual BusinessEntity BusinessEntity { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
        public virtual Password Password { get; set; } = null!;
        public virtual ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<EmailAddress> EmailAddresses { get; set; }
        public virtual ICollection<PersonCreditCard> PersonCreditCards { get; set; }
        public virtual ICollection<PersonPhone> PersonPhones { get; set; }
    }
}

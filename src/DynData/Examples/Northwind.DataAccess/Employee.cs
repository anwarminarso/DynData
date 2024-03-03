using System;
using System.Collections.Generic;

namespace Northwind.DataAccess;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string Title { get; set; }

    public string TitleOfCourtesy { get; set; }

    public DateOnly? BirthDate { get; set; }

    public DateOnly? HireDate { get; set; }

    public string Address { get; set; }

    public string City { get; set; }

    public string Region { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public string HomePhone { get; set; }

    public string Extension { get; set; }

    public byte[] Photo { get; set; }

    public string Notes { get; set; }

    public int? ReportsTo { get; set; }

    public string PhotoPath { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Employee> InverseReportsToNavigation { get; set; } = new List<Employee>();

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual Employee ReportsToNavigation { get; set; }

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual ICollection<Territory> Territories { get; set; } = new List<Territory>();
}

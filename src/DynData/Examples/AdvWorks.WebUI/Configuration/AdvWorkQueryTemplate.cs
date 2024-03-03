using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using AdvWorks.DataAccess;
using a2n.DynData;

namespace AdvWorks.WebUI.Configuration
{

    public class AdvWorkQueryTemplate : QueryTemplate<AdvWorksDbContext>
    {
        public AdvWorkQueryTemplate()
        {
            //Register View Query for AdvWorksDbContext

            // contoh dari view vSalesPerson di database, dijadikan linq query
            // dengan default crudnya ke table SalesPerson
            AddQuery("IniQueryTemplate_vSalesPerson",
                typeof(SalesPerson), // CRUD table type
                (db, provider) =>
                {
                    var qry = (from s in db.SalesPeople
                               join e in db.Employees on s.BusinessEntityId equals e.BusinessEntityId // inner join
                               join p in db.People on s.BusinessEntityId equals p.BusinessEntityId // inner join
                               join bea in db.BusinessEntityAddresses on s.BusinessEntityId equals bea.BusinessEntityId // inner join
                               join a in db.Addresses on bea.AddressId equals a.AddressId // inner join
                               join sp in db.StateProvinces on a.StateProvinceId equals sp.StateProvinceId // inner join
                               join cr in db.CountryRegions on sp.CountryRegionCode equals cr.CountryRegionCode // inner join
                               from st in db.SalesTerritories.Where(x => x.TerritoryId == s.TerritoryId).DefaultIfEmpty() // left join
                               from ea in db.EmailAddresses.Where(x => x.BusinessEntityId == p.BusinessEntityId).DefaultIfEmpty()// left join
                               from pp in db.PersonPhones.Where(x => x.BusinessEntityId == p.BusinessEntityId).DefaultIfEmpty()// left join
                               from pnt in db.PhoneNumberTypes.Where(x => x.PhoneNumberTypeId == pp.PhoneNumberTypeId).DefaultIfEmpty()// left join
                               select new
                               {
                                   s.BusinessEntityId,
                                   p.Title,
                                   p.FirstName,
                                   p.MiddleName,
                                   p.LastName,
                                   p.Suffix,
                                   e.JobTitle,
                                   pp.PhoneNumber,
                                   ASPhoneNumberType = pnt.Name,
                                   EmailAddress = ea.EmailAddress1,
                                   p.EmailPromotion,
                                   a.AddressLine1,
                                   a.AddressLine2,
                                   a.City,
                                   StateProvinceName = sp.Name,
                                   a.PostalCode,
                                   CountryRegionName = cr.Name,
                                   TerritoryName = st.Name,
                                   TerritoryGroup = st.Group,
                                   s.SalesQuota,
                                   s.SalesYtd,
                                   s.SalesLastYear
                               });
                    return qry;
                }, // grid View
                meta =>
                {
                    if (meta.FieldName == "BusinessEntityId")
                        meta.IsPrimaryKey = true;
                } // mapping key query template (in this query BusinessEntityId is a PK on table SalesPerson)
            );
        }
    }
}

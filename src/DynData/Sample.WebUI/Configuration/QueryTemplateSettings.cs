using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using Sample.DataAccess;
using a2n.DynData;

namespace Sample.WebUI.Configuration
{
    public class QueryTemplateSettings : BaseQueryTemplateSettings
    {
        public QueryTemplateSettings()
        {
            #region Register View Query for AdventureWorksContext
            var Template = new QueryTemplate<AdventureWorksContext>();


            // dari view vSalesPerson di db dijadikan linq query
            Template.AddQuery("IniQueryTemplate_vSalesPerson", 
                typeof(SalesPerson),
                db =>
                {
                    var qry = (from s in db.SalesPeople
                               join e in db.Employees on s.BusinessEntityId equals e.BusinessEntityId
                               join p in db.People on s.BusinessEntityId equals p.BusinessEntityId
                               join bea in db.BusinessEntityAddresses on s.BusinessEntityId equals bea.BusinessEntityId
                               join a in db.Addresses on bea.AddressId equals a.AddressId
                               join sp in db.StateProvinces on a.StateProvinceId equals sp.StateProvinceId
                               join cr in db.CountryRegions on sp.CountryRegionCode equals cr.CountryRegionCode
                               from st in db.SalesTerritories.Where(x => x.TerritoryId == s.TerritoryId).DefaultIfEmpty()
                               from ea in db.EmailAddresses.Where(x => x.BusinessEntityId == p.BusinessEntityId).DefaultIfEmpty()
                               from pp in db.PersonPhones.Where(x => x.BusinessEntityId == p.BusinessEntityId).DefaultIfEmpty()
                               from pnt in db.PhoneNumberTypes.Where(x => x.PhoneNumberTypeId == pp.PhoneNumberTypeId).DefaultIfEmpty()
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
                },
                meta =>
                {
                    if (meta.FieldName == "BusinessEntityId")
                        meta.IsPrimaryKey = true;
                }
            );


            //Template.AddQuery("UserRoles", db =>
            //{
            //    var qry = (from ur in db.SysUserRoles
            //               join r in db.SysRoles on ur.RoleId equals r.Id
            //               join u in db.SysUsers on ur.UserId equals u.Id
            //               select new
            //               {
            //                   ur.UserId,
            //                   u.UserName,
            //                   u.Email,
            //                   ur.RoleId,
            //                   RoleName = r.Name
            //               });

            //    return qry;
            //},
            //        new Metadata() { FieldName = "UserId", FieldType = "Sting", FieldLabel = "User Id", CustomAttributes = new { Hidden = true } },
            //        new Metadata() { FieldName = "UserName", FieldType = "Sting", FieldLabel = "User Name" },
            //        new Metadata() { FieldName = "Email", FieldType = "Sting", FieldLabel = "Email" },
            //        new Metadata() { FieldName = "RoleId", FieldType = "Sting", FieldLabel = "Role Id", CustomAttributes = new { Hidden = true } },
            //        new Metadata() { FieldName = "RoleName", FieldType = "DateTime", FieldLabel = "Role Name" }
            //    );

            this.AddTemplate(Template);
            #endregion

        }
    }
}

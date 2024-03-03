using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using Northwind.DataAccess;
using a2n.DynData;

namespace Northwind.WebUI.Configuration
{

    public class NorthwindQueryTemplate : QueryTemplate<NorthwindDbContext>
    {
        public NorthwindQueryTemplate()
        {
            //Register View Query for AdvWorksDbContext
            AddQuery("vProductCategory", typeof(Product), (db, provider) =>
            {
                return (from p in db.Products
                        join c in db.Categories on p.CategoryId equals c.CategoryId
                        join s in db.Suppliers on p.SupplierId equals s.SupplierId
                        select new
                        {
                            p.ProductId,
                            p.CategoryId,
                            c.CategoryName,
                            p.ProductName,
                            p.UnitPrice,
                            p.UnitsInStock,
                            p.SupplierId,
                            SupplierName = s.CompanyName,
                            SupplierContact = s.ContactName
                        });
            },
            meta =>
            {
                if (meta.FieldName == "ProductId")
                {
                    meta.IsPrimaryKey = true;
                    meta.CustomAttributes = new { Hidden = true };
                }
                else if (meta.FieldName == "CategoryId" || meta.FieldName == "SupplierId")
                {
                    meta.CustomAttributes = new { Hidden = true };
                }
            });
        }
    }
}

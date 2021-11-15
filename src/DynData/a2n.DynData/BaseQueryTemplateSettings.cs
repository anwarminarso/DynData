using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

#nullable disable

namespace a2n.DynData
{
    public abstract class BaseQueryTemplateSettings
    {
        private Dictionary<Type, object> dicDataView = new Dictionary<Type, object>();

        public IQueryable<dynamic> GetQuery<T>(T db, string QueryName, params ExpressionRule[] rules)
            where T : DbContext, new()
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return null;
            if (dbCtxtView.HasQueryName(QueryName))
            {
                var qry = dbCtxtView.GetQuery(db, QueryName);
                var viewType = dbCtxtView.GetValueType(db, QueryName);
                if (rules != null && rules.Length > 0)
                {
                    foreach (var rule in rules)
                        rule.ValidatePropertyType(viewType);
                    var whereExp = ExpressionBuilder.Build(viewType, rules);
                    return qry.Where(whereExp, viewType);
                }
                else
                    return qry;
            }
            else
                return null;
        }
        public Metadata[] GetMetadata<T>(T db, string QueryName)
            where T : DbContext
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return null;
            if (dbCtxtView.HasQueryName(QueryName))
                return dbCtxtView.GetMetadata(db, QueryName);
            else
                return null;
        }
        public PropertyInfo[] GetProperties<T>(T db, string QueryName)
           where T : DbContext
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return null;
            if (dbCtxtView.HasQueryName(QueryName))
                return dbCtxtView.GetProperties(db, QueryName);
            else
                return null;
        }
        public Type GetValueType<T>(T db, string QueryName)
           where T : DbContext
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return null;
            if (dbCtxtView.HasQueryName(QueryName))
                return dbCtxtView.GetValueType(db, QueryName);
            else
                return null;
        }

        public string[] GetAllQueryTemplateNames<T>()
            where T : DbContext
        {
            var dbCtxtType = typeof(T);
            if (!dicDataView.ContainsKey(dbCtxtType))
                return new string[] { };
            var qryTpl = dicDataView[dbCtxtType] as QueryTemplate<T>;
            return qryTpl.GetQueryTemplateNames();
        }
        public void AddTemplate<T>(QueryTemplate<T> value)
            where T : DbContext
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (dicDataView.ContainsKey(typeof(T)))
                throw new Exception("Duplicate !!!");
            dicDataView.Add(typeof(T), value);
        }

        private QueryTemplate<T> GetQueryTemplate<T>()
            where T : DbContext
        {
            var dbCtxtType = typeof(T);
            if (!dicDataView.ContainsKey(dbCtxtType))
                return null;
            return dicDataView[dbCtxtType] as QueryTemplate<T>;
        }

    }
}

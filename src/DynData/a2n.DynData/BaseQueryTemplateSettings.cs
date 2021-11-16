using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Newtonsoft.Json.Linq;

#nullable disable

namespace a2n.DynData
{
    public abstract class BaseQueryTemplateSettings
    {
        private Dictionary<Type, object> dicDataView = new Dictionary<Type, object>();

        public bool HasQueryName<T>(T db, string QueryName)
            where T : DbContext, new()
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return false;
            return dbCtxtView.HasQueryName(QueryName);
        }
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
        public dynamic FindByKey<T>(T db, string QueryName, string jsonKeyValues)
            where T : DbContext, new()
        {
            var jKey = JObject.Parse(jsonKeyValues);
            return FindByKey(db, QueryName, jKey);
        }
        public dynamic FindByKey<T>(T db, string QueryName, System.Text.Json.JsonElement keyValues)
            where T : DbContext, new()
        {
            var jKey = JObject.Parse(keyValues.ToString());
            return FindByKey(db, QueryName, jKey);
        }
        public dynamic FindByKey<T>(T db, string QueryName, JObject jKey)
            where T : DbContext, new()
        {
            ExpressionRule rootRule = new ExpressionRule() { IsBracket = true, LogicalOperator = ExpressionLogicalOperator.And };
            var valueType = this.GetValueType(db, QueryName);
            var metaArr = this.GetMetadata<T>(db, QueryName);
            var pkValues = metaArr.Where(t => t.IsPrimaryKey && jKey.ContainsKey(t.FieldName)).Select(t => new { Type = t.PropertyInfo.PropertyType, Name = t.FieldName, Value = jKey[t.FieldName] }).ToArray();
            if (pkValues.Length == 0)
                return null;
            var qry = this.GetQuery(db, QueryName);
            foreach (var pk in pkValues)
            {
                rootRule.AddChild(new ExpressionRule()
                {
                    IsBracket = false,
                    LogicalOperator = ExpressionLogicalOperator.And,
                    Operator = ExpressionOperator.Equal,
                    ReferenceFieldName = pk.Name,
                    ReferenceFieldType = pk.Type,
                    CompareFieldValue = pk.Value.ToString()
                });
            }
            var whereExp = ExpressionBuilder.Build(valueType, rootRule);
            if (whereExp != null)
                return qry.Where(whereExp, valueType).FirstOrDefault();
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
        public Type GetCRUDTableType<T>(T db, string QueryName)
           where T : DbContext
        {
            var dbCtxtView = GetQueryTemplate<T>();
            if (dbCtxtView == null)
                return null;
            if (dbCtxtView.HasQueryName(QueryName))
                return dbCtxtView.GetCRUDTableType(QueryName);
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

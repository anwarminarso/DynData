using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#nullable disable

namespace a2n.DynData
{
    public class QueryTemplate<T>
        where T : DbContext
    {
        private Dictionary<string, QueryMeta<T>> dicQueryList;

        public QueryTemplate()
        {
            dicQueryList = new Dictionary<string, QueryMeta<T>>();
        }
        public string[] GetQueryTemplateNames()
        {
            return dicQueryList.Keys.ToArray();
        }
        public bool HasQueryName(string QueryName)
        {
            return dicQueryList.ContainsKey(QueryName);
        }
        public void AddQuery(string QueryName, Func<T, IQueryable<dynamic>> funQuery, params Metadata[] metadata)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, metadata));
        }
        public void SetMetadata(string QueryName, params Metadata[] metadata)
        {
            if (!dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Query name {QueryName} not found");
            dicQueryList[QueryName].Metadata = metadata;
        }
        public void RemoveQuery(string QueryName)
        {
            if (dicQueryList.ContainsKey(QueryName))
                dicQueryList.Remove(QueryName);
        }
        public IQueryable<dynamic> GetQuery(T db, string QueryName)
        {
            return dicQueryList[QueryName].Handler(db);
        }
        public Metadata[] GetMetadata(T db, string QueryName)
        {
            var results = dicQueryList[QueryName].Metadata;
            if (results == null)
            {
                var piArr = dicQueryList[QueryName].GetProperties(db);
                results = (from t in piArr
                           select new Metadata()
                           {
                               FieldName = t.Name,
                               FieldType = t.PropertyType.Name,
                               FieldLabel = t.Name,
                               CustomAttributes = null
                           }).ToArray();
            }
            return results;
        }
        public PropertyInfo[] GetProperties(T db, string QueryName)
        {
            return dicQueryList[QueryName].GetProperties(db);
        }
        public Type GetValueType(T db, string QueryName)
        {
            return dicQueryList[QueryName].GetValueType(db);
        }

    }
    public class QueryMeta<T>
        where T : DbContext
    {
        public Func<T, IQueryable<dynamic>> Handler { get; private set; }
        public Metadata[] Metadata { get; set; }

        private PropertyInfo[] _properties;
        private Type _valueType;
        public Type GetValueType(T db)
        {
            if (_valueType == null)
            {
                var dynamicType = Handler(db).GetType();
                _valueType = dynamicType.GetGenericArguments()[0];
            }
            return _valueType;
        }
        public PropertyInfo[] GetProperties(T db)
        {
            if (_properties == null)
            {
                var argType = GetValueType(db);
                _properties = argType.GetProperties();
            }
            return _properties;
        }
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler)
        {
            this.Handler = Handler;
        }
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler, Metadata[] Metadata)
        {
            this.Handler = Handler;
            if (Metadata != null && Metadata.Length > 0)
                this.Metadata = Metadata;
        }
    }
}

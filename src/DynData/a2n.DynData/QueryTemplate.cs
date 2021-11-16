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
        public void AddQuery(string QueryName, Type CRUDTableType, Func<T, IQueryable<dynamic>> funQuery, params Metadata[] metadata)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, CRUDTableType, metadata));
        }

        public void AddQuery(string QueryName, Func<T, IQueryable<dynamic>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, OnMetadataGenerated));
        }
        public void AddQuery(string QueryName, Type CRUDTableType, Func<T, IQueryable<dynamic>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, CRUDTableType, OnMetadataGenerated));
        }

        public void SetMetadata(string QueryName, params Metadata[] metadata)
        {
            if (!dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Query name {QueryName} not found");
            dicQueryList[QueryName].Metadata = metadata;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="QueryName">Query/View name</param>
        /// <param name="propertyMapping">from view property name to table property name</param>
        /// <exception cref="Exception">Query not found</exception>
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
            var tpl = dicQueryList[QueryName];
            var results = tpl.Metadata;
            if (results == null)
            {
                List<Metadata> metaLst = new List<Metadata>();
                var piArr = dicQueryList[QueryName].GetProperties(db);
                results = (from pi in piArr
                           select new Metadata(pi, tpl.OnMetadataGenerated)).ToArray();
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
        public Type GetCRUDTableType(string QueryName)
        {
            return dicQueryList[QueryName].GetCRUDTableType();
        }

    }
    public class QueryMeta<T>
        where T : DbContext
    {
        private PropertyInfo[] _properties;
        private Type _valueType;
        private Type _CRUDTableType;

        public Func<T, IQueryable<dynamic>> Handler { get; private set; }
        public Action<Metadata> OnMetadataGenerated { get; private set; }
        public Metadata[] Metadata { get; set; }

        public string[] PrimaryKeyNames { get; set; }

        public string CRUDTableName
        {
            get
            {
                if (_CRUDTableType != null)
                    return _CRUDTableType.Name;
                else
                    return null;
            }
        }

        public Type GetValueType(T db)
        {
            if (_valueType == null)
            {
                var dynamicType = Handler(db).GetType();
                _valueType = dynamicType.GenericTypeArguments[0];
            }
            return _valueType;
        }
        public Type GetCRUDTableType()
        {
            return _CRUDTableType;
        }
        public PropertyInfo[] GetProperties(T db)
        {
            if (_properties == null)
            {
                var argType = GetValueType(db);
                _properties = argType.GetProperties().Where(t => t.PropertyType.Namespace == "System").ToArray();
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
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler, Type CRUDTableType)
        {
            this._CRUDTableType = CRUDTableType;
            this.Handler = Handler;
        }
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler, Type CRUDTableType, Metadata[] Metadata)
        {
            this.Handler = Handler;
            if (Metadata != null && Metadata.Length > 0)
                this.Metadata = Metadata;
            this._CRUDTableType = CRUDTableType;
        }
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler, Action<Metadata> OnMetadataGenerated)
        {
            this.Handler = Handler;
            this.OnMetadataGenerated = OnMetadataGenerated;
        }
        public QueryMeta(Func<T, IQueryable<dynamic>> Handler, Type CRUDTableType, Action<Metadata> OnMetadataGenerated)
        {
            this._CRUDTableType = CRUDTableType;
            this.Handler = Handler;
            this.OnMetadataGenerated = OnMetadataGenerated;
        }
    }
}

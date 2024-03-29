﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;


namespace a2n.DynData
{
    public interface IQueryTemplate
    {
        string[] GetQueryTemplateNames();
        bool HasQueryName(string QueryName);
    }
    public abstract class QueryTemplate<T> : IQueryTemplate
        where T : DbContext
    {
        private readonly Dictionary<string, QueryMeta<T>> dicQueryList = new Dictionary<string, QueryMeta<T>>();
        private object lockObject = new object();

        public string[] GetQueryTemplateNames()
        {
            return dicQueryList.Keys.ToArray();
        }
        public bool HasQueryName(string QueryName)
        {
            return dicQueryList.ContainsKey(QueryName);
        }
        public void AddQuery(string QueryName, Func<T, IServiceProvider, IQueryable<dynamic>> funQuery, params Metadata[] metadata)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, metadata));
        }
        public void AddQuery(string QueryName, Type CRUDTableType, Func<T, IServiceProvider, IQueryable<dynamic>> funQuery, params Metadata[] metadata)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, CRUDTableType, metadata));
        }

        public void AddQuery(string QueryName, Func<T, IServiceProvider, IQueryable<dynamic>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, OnMetadataGenerated));
        }
        public void AddQuery(string QueryName, Type CRUDTableType, Func<T, IServiceProvider, IQueryable<dynamic>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery, CRUDTableType, OnMetadataGenerated));
        }

        public void AddQuery<TResult>(Func<T, IServiceProvider, IQueryable<TResult>> funQuery, params Metadata[] metadata)
        {
            string QueryName = typeof(TResult).Name;
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery as Func<T, IServiceProvider, IQueryable<dynamic>>, metadata));
        }
        public void AddQuery<TResult>(Type CRUDTableType, Func<T, IServiceProvider, IQueryable<TResult>> funQuery, params Metadata[] metadata)
        {
            string QueryName = typeof(TResult).Name;
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery as Func<T, IServiceProvider, IQueryable<dynamic>>, CRUDTableType, metadata));
        }
        public void AddQuery<TResult>(Func<T, IServiceProvider, IQueryable<TResult>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            string QueryName = typeof(TResult).Name;
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery as Func<T, IServiceProvider, IQueryable<dynamic>>, OnMetadataGenerated));
        }
        public void AddQuery<TResult>(Type CRUDTableType, Func<T, IServiceProvider, IQueryable<TResult>> funQuery, Action<Metadata> OnMetadataGenerated)
        {
            string QueryName = typeof(TResult).Name;
            if (dicQueryList.ContainsKey(QueryName))
                throw new Exception($"Duplicate query name {QueryName}");
            dicQueryList.Add(QueryName, new QueryMeta<T>(funQuery as Func<T, IServiceProvider, IQueryable<dynamic>>, CRUDTableType, OnMetadataGenerated));
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
        public IQueryable<dynamic> GetQuery(T db, IServiceProvider provider, string QueryName)
        {
            return dicQueryList[QueryName].Handler(db, provider);
        }
        public Metadata[] GetMetadata(T db, IServiceProvider provider, string QueryName)
        {
            var tpl = dicQueryList[QueryName];
            var results = tpl.Metadata;
            if (results == null)
            {
                List<Metadata> metaLst = new List<Metadata>();
                var piArr = dicQueryList[QueryName].GetProperties(db, provider);
                results = (from pi in piArr
                           select new Metadata(pi, tpl.OnMetadataGenerated)).ToArray();
            }
            return results;
        }
        public PropertyInfo[] GetProperties(T db, IServiceProvider provider, string QueryName)
        {
            return dicQueryList[QueryName].GetProperties(db, provider);
        }
        public Type GetValueType(T db, IServiceProvider provider, string QueryName)
        {
            return dicQueryList[QueryName].GetValueType(db, provider);
        }
        public Type GetCRUDTableType(string QueryName)
        {
            return dicQueryList[QueryName].GetCRUDTableType();
        }

        public dynamic FindByKey(T db, IServiceProvider provider, string QueryName, string jsonKeyValues)
        {
            var jKey = JObject.Parse(jsonKeyValues);
            return FindByKey(db, provider, QueryName, jKey);
        }
        public dynamic FindByKey(T db, IServiceProvider provider, string QueryName, System.Text.Json.JsonElement keyValues)
        {
            var jKey = JObject.Parse(keyValues.ToString());
            return FindByKey(db, provider, QueryName, jKey);
        }
        public dynamic FindByKey(T db, IServiceProvider provider, string QueryName, JObject jKey)
        {
            ExpressionRule rootRule = new ExpressionRule() { IsBracket = true, LogicalOperator = ExpressionLogicalOperator.And };
            var valueType = this.GetValueType(db, provider, QueryName);
            var metaArr = this.GetMetadata(db, provider, QueryName);
            var pkValues = metaArr.Where(t => t.IsPrimaryKey && jKey.ContainsKey(t.FieldName)).Select(t => new { Type = t.PropertyInfo.PropertyType, Name = t.FieldName, Value = jKey[t.FieldName] }).ToArray();
            if (pkValues.Length == 0)
                return null;
            var qry = this.GetQuery(db, provider, QueryName);
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

        public IQueryable<dynamic> GetQuery(T db, IServiceProvider provider, string QueryName, params ExpressionRule[] rules)
        {
            if (HasQueryName(QueryName))
            {
                var qry = GetQuery(db, provider, QueryName);
                var viewType = GetValueType(db, provider, QueryName);
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

    }


    public class QueryMeta<T>
        where T : DbContext
    {
        protected PropertyInfo[] _properties;
        protected Type _valueType;
        protected Type _CRUDTableType;

        //public Func<T, IQueryable<dynamic>> Handler { get; protected set; }
        public Func<T, IServiceProvider, IQueryable<dynamic>> Handler { get; protected set; }
        public Action<Metadata> OnMetadataGenerated { get; protected set; }
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

        public Type GetValueType(T db, IServiceProvider provider)
        {
            if (_valueType == null)
            {
                var dynamicType = Handler(db, provider).GetType();
                _valueType = dynamicType.GenericTypeArguments[0];
            }
            return _valueType;
        }
        public Type GetCRUDTableType()
        {
            return _CRUDTableType;
        }
        public PropertyInfo[] GetProperties(T db, IServiceProvider provider)
        {
            if (_properties == null)
            {
                var argType = GetValueType(db, provider);
                _properties = argType.GetProperties().Where(t => t.PropertyType.Namespace == "System").ToArray();
            }
            return _properties;
        }

        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler)
        {
            this.Handler = Handler;
        }
        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler, Metadata[] Metadata)
        {
            this.Handler = Handler;
            if (Metadata != null && Metadata.Length > 0)
                this.Metadata = Metadata;
        }
        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler, Type CRUDTableType)
        {
            this._CRUDTableType = CRUDTableType;
            this.Handler = Handler;
        }
        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler, Type CRUDTableType, Metadata[] Metadata)
        {
            this.Handler = Handler;
            if (Metadata != null && Metadata.Length > 0)
                this.Metadata = Metadata;
            this._CRUDTableType = CRUDTableType;
        }
        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler, Action<Metadata> OnMetadataGenerated)
        {
            this.Handler = Handler;
            this.OnMetadataGenerated = OnMetadataGenerated;
        }
        public QueryMeta(Func<T, IServiceProvider, IQueryable<dynamic>> Handler, Type CRUDTableType, Action<Metadata> OnMetadataGenerated)
        {
            this._CRUDTableType = CRUDTableType;
            this.Handler = Handler;
            this.OnMetadataGenerated = OnMetadataGenerated;
        }
    }

}

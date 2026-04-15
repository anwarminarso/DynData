
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

namespace a2n.DynData
{
    public class DataTableJSRequest
    {
        public string id { get; set; }
        public string viewName { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public int draw { get; set; }

        public bool usePGSQL { get; set; } = false;

        [JsonIgnore]
        public bool EnableSearchIgnoreCase { get; set; } = false;

        public DataTableJSSearch search { get; set; } = new DataTableJSSearch();

        public string externalFilter { get; set; }

        public string jsonQB { get; set; } = null;

        public DataTableJSColumn[] columns { get; set; } = new DataTableJSColumn[] { };
        public DataTableJSOrder[] order { get; set; } = new DataTableJSOrder[] { };

        public DataTableJSRequest()
        {
        }
        public ExpressionRule[] ToRules(Type type)
        {
            return ToRules(type.GetProperties());
        }
        public ExpressionRule[] ToRules(PropertyInfo[] propArr)
        {
            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            ExternalFilterParser.ParseExternalFilter(externalFilter,
                name =>
                {
                    var p = propArr.Where(t => t.Name == name).SingleOrDefault();
                    return p != null ? (p.Name, p.PropertyType) : ((string, Type)?)null;
                }, rootRule);

            ExternalFilterParser.ParseGlobalSearch(search.value, usePGSQL, EnableSearchIgnoreCase,
                () => propArr.Select(p => (p.Name, p.PropertyType)), rootRule);

            ExternalFilterParser.ParseQueryBuilder(jsonQB, propArr, rootRule);

            return new[] { rootRule };
        }
        public ExpressionRule[] ToRules(Metadata[] metaArr)
        {
            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            ExternalFilterParser.ParseExternalFilter(externalFilter,
                name =>
                {
                    var m = metaArr.Where(t => t.FieldName == name).SingleOrDefault();
                    return m != null ? (m.FieldName, m.PropertyInfo.PropertyType) : ((string, Type)?)null;
                }, rootRule);

            ExternalFilterParser.ParseGlobalSearch(search.value, usePGSQL, EnableSearchIgnoreCase,
                () => metaArr.Where(m => m.IsOrderable).Select(m => (m.FieldName, m.PropertyInfo.PropertyType)), rootRule);

            var propArr = metaArr.Select(t => t.PropertyInfo).ToArray();
            ExternalFilterParser.ParseQueryBuilder(jsonQB, propArr, rootRule);

            return new[] { rootRule };
        }
        public object ToWhereExpression(Type type)
        {
            return ToWhereExpression(type, type.GetProperties());
        }
        public object ToWhereExpression(Type type, PropertyInfo[] propArr)
        {
            var rules = ToRules(propArr);
            return ExpressionBuilder.Build(type, rules);
        }
        public object ToWhereExpression(Type type, Metadata[] metaArr)
        {
            var rules = ToRules(metaArr);
            return ExpressionBuilder.Build(type, rules);
        }

        public IQueryable<T> ToQueryable<T>(IQueryable<T> query)
        {
            var valueType = typeof(T);
            var propArr = valueType.GetProperties();
            var whereExp = ToWhereExpression(valueType, propArr) as Expression<Func<T, bool>>;
            var qry = query;
            if (whereExp != null)
                qry = query.Where(whereExp);

            if (this.order != null && this.order.Length > 0)
            {
                var ColOrderBy = columns[this.order[0].column].name;
                bool ascending = true;
                if (!string.IsNullOrEmpty(this.order[0].dir) && this.order[0].dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(ColOrderBy, ascending);
                if (order.Length > 1)
                {
                    for (int i = 1; i < order.Length; i++)
                    {
                        ascending = true;
                        ColOrderBy = columns[order[i].column].name;
                        if (!string.IsNullOrEmpty(order[i].dir) && order[i].dir != "asc")
                            ascending = false;
                        qry = qry.ThenBy(ColOrderBy, ascending);
                    }
                }
            }

            return qry;
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType)
        {
            var propArr = valueType.GetProperties();
            return ToQueryable(query, valueType, propArr);
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType, PropertyInfo[] propArr)
        {
            var qry = query;
            var whereExp = ToWhereExpression(valueType, propArr);
            if (whereExp != null)
                qry = query.Where(whereExp, valueType);

            if (this.order != null && this.order.Length > 0)
            {
                var ColOrderBy = columns[this.order[0].column].name;
                bool ascending = true;
                if (!string.IsNullOrEmpty(this.order[0].dir) && this.order[0].dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(ColOrderBy, valueType, ascending);
                if (order.Length > 1)
                {
                    for (int i = 1; i < order.Length; i++)
                    {
                        ascending = true;
                        ColOrderBy = columns[order[i].column].name;
                        if (!string.IsNullOrEmpty(order[i].dir) && order[i].dir != "asc")
                            ascending = false;
                        qry = qry.ThenBy(ColOrderBy, valueType, ascending);
                    }
                }
            }

            return qry;
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType, Metadata[] metaArr)
        {
            var qry = query;
            var whereExp = ToWhereExpression(valueType, metaArr);
            if (whereExp != null)
                qry = query.Where(whereExp, valueType);

            if (this.order != null && this.order.Length > 0)
            {
                var ColOrderBy = columns[this.order[0].column].name;
                bool ascending = true;
                if (!string.IsNullOrEmpty(this.order[0].dir) && this.order[0].dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(ColOrderBy, valueType, ascending);
                if (order.Length > 1)
                {
                    for (int i = 1; i < order.Length; i++)
                    {
                        ascending = true;
                        ColOrderBy = columns[order[i].column].name;
                        if (!string.IsNullOrEmpty(order[i].dir) && order[i].dir != "asc")
                            ascending = false;
                        qry = qry.ThenBy(ColOrderBy, valueType, ascending);
                    }
                }
            }

            return qry;
        }

        public PagingResult<T> ToPagingResult<T>(IQueryable<T> query)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query);
            if (length == -1)
            {
                var items = qry.ToArray();
                return new PagingResult<T>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return qry.ToPagingResult(length, pageIndex);
        }
        public PagingResult<dynamic> ToPagingResult(IQueryable<dynamic> query, Type valueType)
        {
            var propArr = valueType.GetProperties();
            return ToPagingResult(query, valueType, propArr);
        }
        public PagingResult<dynamic> ToPagingResult(IQueryable<dynamic> query, Type valueType, PropertyInfo[] propArr)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query, valueType, propArr);
            if (length == -1)
            {
                var items = qry.ToArray();
                return new PagingResult<dynamic>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return qry.ToPagingResult(length, pageIndex);
        }
        public PagingResult<dynamic> ToPagingResult(IQueryable<dynamic> query, Type valueType, Metadata[] metaArr)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query, valueType, metaArr);
            if (length == -1)
            {
                var items = qry.ToArray();
                return new PagingResult<dynamic>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return qry.ToPagingResult(length, pageIndex);
        }

        public async Task<PagingResult<T>> ToPagingResultAsync<T>(IQueryable<T> query)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query);
            if (length == -1)
            {
                var items = await qry.ToArrayAsync();
                return new PagingResult<T>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return await qry.ToPagingResultAsync(length, pageIndex);
        }
        public async Task<PagingResult<dynamic>> ToPagingResultAsync(IQueryable<dynamic> query, Type valueType)
        {
            var propArr = valueType.GetProperties();
            return await ToPagingResultAsync(query, valueType, propArr);
        }
        public async Task<PagingResult<dynamic>> ToPagingResultAsync(IQueryable<dynamic> query, Type valueType, PropertyInfo[] propArr)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query, valueType, propArr);
            if (length == -1)
            {
                var items = await qry.ToArrayAsync();
                return new PagingResult<dynamic>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return await qry.ToPagingResultAsync(length, pageIndex);
        }
        public async Task<PagingResult<dynamic>> ToPagingResultAsync(IQueryable<dynamic> query, Type valueType, Metadata[] metaArr)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query, valueType, metaArr);
            if (length == -1)
            {
                var items = await qry.ToArrayAsync();
                return new PagingResult<dynamic>()
                {
                    pageIndex = 0,
                    pageSize = -1,
                    context = null,
                    items = items,
                    totalPages = 1,
                    totalRows = items.Length
                };
            }
            return await qry.ToPagingResultAsync(length, pageIndex);
        }
    }


    public class DataTableJSExportRequest
    {
        public string viewName { get; set; }
        public string globalSearch { get; set; }
        public string orderBy { get; set; }
        public string dir { get; set; }
        public string jsonQB { get; set; } = null;
        public string format { get; set; }

        public string externalFilter { get; set; }

        public bool usePGSQL { get; set; } = false;

        [JsonIgnore]
        public bool EnableSearchIgnoreCase { get; set; } = false;

        public ExpressionRule[] ToRules(Type type)
        {
            return ToRules(type.GetProperties());
        }
        public ExpressionRule[] ToRules(PropertyInfo[] propArr)
        {
            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            ExternalFilterParser.ParseExternalFilter(externalFilter,
                name =>
                {
                    var p = propArr.Where(t => t.Name == name).SingleOrDefault();
                    return p != null ? (p.Name, p.PropertyType) : ((string, Type)?)null;
                }, rootRule);

            ExternalFilterParser.ParseGlobalSearch(globalSearch, usePGSQL, EnableSearchIgnoreCase,
                () => propArr.Select(p => (p.Name, p.PropertyType)), rootRule);

            ExternalFilterParser.ParseQueryBuilder(jsonQB, propArr, rootRule);

            return new[] { rootRule };
        }

        public ExpressionRule[] ToRules(Metadata[] metaArr)
        {
            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            ExternalFilterParser.ParseExternalFilter(externalFilter,
                name =>
                {
                    var m = metaArr.Where(t => t.FieldName == name).SingleOrDefault();
                    return m != null ? (m.FieldName, m.PropertyInfo.PropertyType) : ((string, Type)?)null;
                }, rootRule);

            ExternalFilterParser.ParseGlobalSearch(globalSearch, usePGSQL, EnableSearchIgnoreCase,
                () => metaArr.Where(m => m.IsOrderable && m.IsSearchable).Select(m => (m.FieldName, m.PropertyInfo.PropertyType)), rootRule);

            var propArr = metaArr.Select(t => t.PropertyInfo).ToArray();
            ExternalFilterParser.ParseQueryBuilder(jsonQB, propArr, rootRule);

            return new[] { rootRule };
        }
        public object ToWhereExpression(Type type)
        {
            return ToWhereExpression(type, type.GetProperties());
        }
        public object ToWhereExpression(Type type, PropertyInfo[] propArr)
        {
            var rules = ToRules(propArr);
            return ExpressionBuilder.Build(type, rules);
        }
        public object ToWhereExpression(Type type, Metadata[] metaArr)
        {
            var rules = ToRules(metaArr);
            return ExpressionBuilder.Build(type, rules);
        }
        public IQueryable<T> ToQueryable<T>(IQueryable<T> query)
        {
            var valueType = typeof(T);
            var propArr = valueType.GetProperties();
            var whereExp = ToWhereExpression(valueType, propArr) as Expression<Func<T, bool>>;
            var qry = query;
            if (whereExp != null)
                qry = query.Where(whereExp);
            if (!string.IsNullOrEmpty(orderBy))
            {
                bool ascending = true;
                if (!string.IsNullOrEmpty(dir) && dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(orderBy, ascending) as IQueryable<T>;
            }
            return qry;
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType)
        {
            var propArr = valueType.GetProperties();
            return ToQueryable(query, valueType, propArr);
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType, PropertyInfo[] propArr)
        {
            var qry = query;
            var whereExp = ToWhereExpression(valueType, propArr);
            if (whereExp != null)
                qry = query.Where(whereExp, valueType);
            if (!string.IsNullOrEmpty(orderBy))
            {
                bool ascending = true;
                if (!string.IsNullOrEmpty(dir) && dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(orderBy, valueType, ascending);
            }

            return qry;
        }
        public IQueryable<dynamic> ToQueryable(IQueryable<dynamic> query, Type valueType, Metadata[] metaArr)
        {
            var qry = query;
            var whereExp = ToWhereExpression(valueType, metaArr);
            if (whereExp != null)
                qry = query.Where(whereExp, valueType);
            if (!string.IsNullOrEmpty(orderBy))
            {
                bool ascending = true;
                if (!string.IsNullOrEmpty(dir) && dir != "asc")
                    ascending = false;
                qry = qry.OrderBy(orderBy, valueType, ascending);
            }

            return qry;
        }

    }
    public class DataTableJSSearch
    {
        public bool regex { get; set; }
        public string value { get; set; }
    }
    public class DataTableJSResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public object data { get; set; }
        public string error { get; set; }
        public DataTableJSResponse(DataTableJSRequest req, PagingResult<dynamic> page)
        {
            draw = req.draw;
            recordsTotal = page.totalRows;
            recordsFiltered = page.totalRows;
            data = page.items;
        }
        public DataTableJSResponse()
        {
        }
    }
    public class DataTableJSColumn
    {
        public string name { get; set; }
        //public string data { get; set; }
        //public string type { get; set; }
        //public bool wrap { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public DataTableJSSearch search { get; set; } = new DataTableJSSearch();
    }
    public class DataTableJSOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
}

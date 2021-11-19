
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
#nullable disable
namespace a2n.DynData
{
    public class DataTableJSRequest
    {
        public string id { get; set; }
        public string viewName { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public int draw { get; set; }

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
            List<ExpressionRule> result = new List<ExpressionRule>();

            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };
            if (!string.IsNullOrEmpty(search.value))
            {
                var globalSearchRule = new ExpressionRule()
                {
                    IsBracket = true,
                    LogicalOperator = ExpressionLogicalOperator.And
                };
                foreach (var prop in propArr)
                {
                    if (prop.PropertyType == typeof(String))
                    {
                        var childSearch = new ExpressionRule()
                        {
                            IsBracket = false,
                            LogicalOperator = ExpressionLogicalOperator.Or,
                            Operator = ExpressionOperator.Contains,
                            ReferenceFieldName = prop.Name,
                            ReferenceFieldType = prop.PropertyType,
                            CompareFieldObject = search.value
                        };
                        globalSearchRule.AddChild(childSearch);
                    }
                }
                rootRule.AddChild(globalSearchRule);
            }

            result.Add(rootRule);
            if (!string.IsNullOrEmpty(jsonQB))
            {
                var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(jsonQB);
                if (queryBuilder != null && queryBuilder.ruleData != null)
                {
                    var rules = queryBuilder.ToExpressionRule(propArr, null);
                    foreach (var rule in rules)
                        rootRule.AddChild(rule);
                }
            }
            return result.ToArray();
        }
        public ExpressionRule[] ToRules(Metadata[] metaArr)
        {
            List<ExpressionRule> result = new List<ExpressionRule>();

            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };
            if (!string.IsNullOrEmpty(search.value))
            {
                var globalSearchRule = new ExpressionRule()
                {
                    IsBracket = true,
                    LogicalOperator = ExpressionLogicalOperator.And
                };
                foreach (var meta in metaArr)
                {
                    if (!meta.IsOrderable)
                        continue;
                    if (meta.PropertyInfo.PropertyType == typeof(String))
                    {
                        var childSearch = new ExpressionRule()
                        {
                            IsBracket = false,
                            LogicalOperator = ExpressionLogicalOperator.Or,
                            Operator = ExpressionOperator.Contains,
                            ReferenceFieldName = meta.FieldName,
                            ReferenceFieldType = meta.PropertyInfo.PropertyType,
                            CompareFieldObject = search.value
                        };
                        globalSearchRule.AddChild(childSearch);
                    }
                }
                rootRule.AddChild(globalSearchRule);
            }

            result.Add(rootRule);
            if (!string.IsNullOrEmpty(jsonQB))
            {
                var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(jsonQB);
                var propArr = metaArr.Select(t => t.PropertyInfo).ToArray();
                if (queryBuilder != null && queryBuilder.ruleData != null)
                {
                    var rules = queryBuilder.ToExpressionRule(propArr, null);
                    foreach (var rule in rules)
                        rootRule.AddChild(rule);
                }
            }
            return result.ToArray();
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
                qry = qry.OrderBy(ColOrderBy, ascending) as IQueryable<T>;
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
            return qry.ToPagingResult(length, pageIndex);
        }
        public PagingResult<dynamic> ToPagingResult(IQueryable<dynamic> query, Type valueType, Metadata[] metaArr)
        {
            int pageIndex = 0;
            if (length > 0)
                pageIndex = start / length;
            var qry = ToQueryable(query, valueType, metaArr);
            return qry.ToPagingResult(length, pageIndex);
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

        public string externalfilter { get; set; }

        public ExpressionRule[] ToRules(Type type)
        {
            return ToRules(type.GetProperties());
        }
        public ExpressionRule[] ToRules(PropertyInfo[] propArr)
        {
            List<ExpressionRule> result = new List<ExpressionRule>();

            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };
            if (!string.IsNullOrEmpty(globalSearch))
            {
                var globalSearchRule = new ExpressionRule()
                {
                    IsBracket = true,
                    LogicalOperator = ExpressionLogicalOperator.And
                };
                foreach (var prop in propArr)
                {
                    if (prop.PropertyType == typeof(String))
                    {
                        var childSearch = new ExpressionRule()
                        {
                            IsBracket = false,
                            LogicalOperator = ExpressionLogicalOperator.Or,
                            Operator = ExpressionOperator.Contains,
                            ReferenceFieldName = prop.Name,
                            ReferenceFieldType = prop.PropertyType,
                            CompareFieldObject = globalSearch
                        };
                        globalSearchRule.AddChild(childSearch);
                    }
                }
                rootRule.AddChild(globalSearchRule);
            }

            result.Add(rootRule);
            if (!string.IsNullOrEmpty(jsonQB))
            {
                var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(jsonQB);
                if (queryBuilder != null && queryBuilder.ruleData != null)
                {
                    var rules = queryBuilder.ToExpressionRule(propArr, null);
                    foreach (var rule in rules)
                        rootRule.AddChild(rule);
                }
            }
            return result.ToArray();
        }

        public ExpressionRule[] ToRules(Metadata[] metaArr)
        {
            List<ExpressionRule> result = new List<ExpressionRule>();

            var rootRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };
            if (!string.IsNullOrEmpty(globalSearch))
            {
                var globalSearchRule = new ExpressionRule()
                {
                    IsBracket = true,
                    LogicalOperator = ExpressionLogicalOperator.And
                };
                foreach (var meta in metaArr)
                {
                    if (!meta.IsOrderable)
                        continue;
                    if (meta.PropertyInfo.PropertyType == typeof(String))
                    {
                        var childSearch = new ExpressionRule()
                        {
                            IsBracket = false,
                            LogicalOperator = ExpressionLogicalOperator.Or,
                            Operator = ExpressionOperator.Contains,
                            ReferenceFieldName = meta.FieldName,
                            ReferenceFieldType = meta.PropertyInfo.PropertyType,
                            CompareFieldObject = globalSearch
                        };
                        globalSearchRule.AddChild(childSearch);
                    }
                }
                rootRule.AddChild(globalSearchRule);
            }

            result.Add(rootRule);
            if (!string.IsNullOrEmpty(jsonQB))
            {
                var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(jsonQB);
                var propArr = metaArr.Select(t => t.PropertyInfo).ToArray();
                if (queryBuilder != null && queryBuilder.ruleData != null)
                {
                    var rules = queryBuilder.ToExpressionRule(propArr, null);
                    foreach (var rule in rules)
                        rootRule.AddChild(rule);
                }
            }
            return result.ToArray();
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

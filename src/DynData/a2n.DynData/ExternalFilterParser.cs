using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace a2n.DynData
{
    /// <summary>
    /// Shared helper for parsing external filters, global search, and jQuery QueryBuilder JSON
    /// into ExpressionRule arrays. Eliminates code duplication between DataTableJSRequest and DataTableJSExportRequest.
    /// </summary>
    internal static class ExternalFilterParser
    {
        public static void ParseExternalFilter(string externalFilter, Func<string, (string fieldName, Type fieldType)?> fieldResolver, ExpressionRule parentRule)
        {
            if (string.IsNullOrEmpty(externalFilter))
                return;

            JObject objVal = JsonConvert.DeserializeObject(externalFilter) as JObject;
            var externalFilterRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            foreach (JProperty prop in objVal.Properties())
            {
                var resolved = fieldResolver(prop.Name);
                if (resolved == null)
                    continue;

                var fieldName = resolved.Value.fieldName;
                var fieldType = resolved.Value.fieldType;

                JArray values = null;
                List<ExpressionRule> childFilters = new List<ExpressionRule>();
                bool requireIn = false;

                if (prop.Value.Type == JTokenType.Array)
                {
                    values = prop.Value as JArray;
                    requireIn = true;
                }
                else
                {
                    values = new JArray();
                    values.Add(prop.Value);
                }

                if (requireIn)
                {
                    foreach (JValue value in values)
                    {
                        var val = value.Value.ToString();
                        if (val.StartsWith(">") || val.StartsWith("<") || val.StartsWith("="))
                        {
                            requireIn = false;
                            break;
                        }
                    }
                }

                if (requireIn)
                {
                    var ruleIn = new ExpressionRule()
                    {
                        IsBracket = true,
                        LogicalOperator = ExpressionLogicalOperator.And
                    };
                    foreach (JValue value in values)
                    {
                        var val = value.Value.ToString().Trim();
                        ruleIn.AddChild(new ExpressionRule()
                        {
                            IsBracket = false,
                            Operator = ExpressionOperator.Equal,
                            LogicalOperator = ExpressionLogicalOperator.Or,
                            ReferenceFieldName = fieldName,
                            ReferenceFieldType = fieldType,
                            CompareFieldValue = val
                        });
                    }
                    childFilters.Add(ruleIn);
                }
                else
                {
                    foreach (JValue value in values)
                    {
                        var val = value.Value.ToString();
                        childFilters.Add(ParseSingleFilterValue(val, fieldName, fieldType));
                    }
                }

                foreach (var item in childFilters)
                    externalFilterRule.AddChild(item);
            }

            if (externalFilterRule.Children.Length > 0)
                parentRule.AddChild(externalFilterRule);
        }

        public static void ParseGlobalSearch(string searchValue, bool usePGSQL, bool enableSearchIgnoreCase,
            Func<IEnumerable<(string fieldName, Type fieldType)>> stringFieldsProvider, ExpressionRule parentRule)
        {
            if (string.IsNullOrEmpty(searchValue))
                return;

            var globalSearchRule = new ExpressionRule()
            {
                IsBracket = true,
                LogicalOperator = ExpressionLogicalOperator.And
            };

            foreach (var (fieldName, fieldType) in stringFieldsProvider())
            {
                if (fieldType == typeof(String))
                {
                    globalSearchRule.AddChild(new ExpressionRule()
                    {
                        IsBracket = false,
                        LogicalOperator = ExpressionLogicalOperator.Or,
                        Operator = usePGSQL ? ExpressionOperator.PGSQLContains
                                 : enableSearchIgnoreCase ? ExpressionOperator.ContainsIgnoreCase
                                 : ExpressionOperator.Contains,
                        ReferenceFieldName = fieldName,
                        ReferenceFieldType = fieldType,
                        CompareFieldObject = searchValue
                    });
                }
            }

            parentRule.AddChild(globalSearchRule);
        }

        public static void ParseQueryBuilder(string jsonQB, PropertyInfo[] propArr, ExpressionRule parentRule)
        {
            if (string.IsNullOrEmpty(jsonQB))
                return;

            var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(jsonQB);
            if (queryBuilder != null && queryBuilder.ruleData != null)
            {
                var rules = queryBuilder.ToExpressionRule(propArr, null);
                foreach (var rule in rules)
                    parentRule.AddChild(rule);
            }
        }

        private static ExpressionRule ParseSingleFilterValue(string val, string fieldName, Type fieldType)
        {
            ExpressionOperator opr;
            string parsedVal;

            if (val.StartsWith(">="))
            {
                opr = ExpressionOperator.GreaterThanOrEqual;
                parsedVal = val.Substring(2).Trim();
            }
            else if (val.StartsWith(">"))
            {
                opr = ExpressionOperator.GreaterThan;
                parsedVal = val.Substring(1).Trim();
            }
            else if (val.StartsWith("<="))
            {
                opr = ExpressionOperator.LessThanOrEqual;
                parsedVal = val.Substring(2).Trim();
            }
            else if (val.StartsWith("<"))
            {
                opr = ExpressionOperator.LessThan;
                parsedVal = val.Substring(1).Trim();
            }
            else if (val.StartsWith("="))
            {
                opr = ExpressionOperator.Equal;
                parsedVal = val.Substring(1).Trim();
            }
            else if (val.StartsWith("%") && val.EndsWith("%"))
            {
                opr = ExpressionOperator.Contains;
                parsedVal = val.Substring(1, val.Length - 2).Trim();
            }
            else if (val.StartsWith("%"))
            {
                opr = ExpressionOperator.EndsWith;
                parsedVal = val.Substring(1).Trim();
            }
            else if (val.EndsWith("%"))
            {
                opr = ExpressionOperator.StartsWith;
                parsedVal = val.Substring(0, val.Length - 1).Trim();
            }
            else
            {
                opr = ExpressionOperator.Equal;
                parsedVal = val.Trim();
            }

            return new ExpressionRule()
            {
                IsBracket = false,
                Operator = opr,
                LogicalOperator = ExpressionLogicalOperator.And,
                ReferenceFieldName = fieldName,
                ReferenceFieldType = fieldType,
                CompareFieldValue = parsedVal
            };
        }
    }
}

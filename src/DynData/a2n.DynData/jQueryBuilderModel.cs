
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#nullable disable

namespace a2n.DynData
{
    [JsonConverter(typeof(jQueryBuilderJSONConverter))]
    public abstract class jQueryBuilderBaseRuleItemModel
    {
        public jQueryBuilderUserData data { get; set; }
    }
    public class jQueryBuilderModel
    {
        public int ruleGroupId { get; set; }

        public string referenceType { get; set; }

        public jQueryBuilderRuleSetModel ruleData { get; set; }

        internal ExpressionRule[] ToExpressionRule(PropertyInfo[] piArr, jQueryBuilderRuleSetModel parent = null)
        {
            List<ExpressionRule> rules = new List<ExpressionRule>();
            jQueryBuilderRuleSetModel current = parent;
            if (current == null)
                current = ruleData;
            if (current != null && current.rules != null && current.rules.Count > 0)
            {
                foreach (var item in current.rules)
                {
                    if (item is jQueryBuilderRuleModel)
                    {
                        var rule = item as jQueryBuilderRuleModel;
                        var propoertyInfo = piArr.Where(t => t.Name == rule.id).FirstOrDefault();
                        ExpressionRule exrule = new ExpressionRule()
                        {
                            IsBracket = false,
                            LogicalOperator = jQueryBuilderUtils.ConvertToLogicalOperator(current.condition),
                            ReferenceFieldName = rule.id,
                            ReferenceFieldType = propoertyInfo?.PropertyType,
                            Operator = jQueryBuilderUtils.ConvertToOperator(rule.@operator),
                            CompareFieldValue = JsonConvert.SerializeObject(rule.value)
                        };
                        rules.Add(exrule);
                    }
                    else
                    {
                        var ruleSet = item as jQueryBuilderRuleSetModel;
                        ExpressionRule exrule = new ExpressionRule()
                        {
                            IsBracket = true,
                            LogicalOperator = jQueryBuilderUtils.ConvertToLogicalOperator(current.condition)
                        };

                        var childExRuleArr = ToExpressionRule(piArr, ruleSet);
                        foreach (var childExRule in childExRuleArr)
                            exrule.AddChild(childExRule);
                        rules.Add(exrule);
                    }
                }
            }
            return rules.ToArray();
        }
        public object ToWhereExpression(Type type, PropertyInfo[] piArr = null)
        {
            if (piArr == null)
                piArr = type.GetProperties().Where(t => t.PropertyType.Namespace == "System").ToArray();
            var rules = ToExpressionRule(piArr, null);
           
            return ExpressionBuilder.Build(type, rules);
        }
        public static object GenerateFilterOptions(Type type, Metadata[] metadataArr)
        {
            if (type == null || metadataArr == null || metadataArr.Length == 0)
                return null;
            List<object> objLst = new List<object>();
            var propArr = type.GetProperties();
            foreach (var meta in metadataArr)
            {
                Dictionary<string, object> dicValues = new Dictionary<string, object>();
                var pt = propArr.Where(t => t.Name == meta.FieldName).Select(t => t.PropertyType).FirstOrDefault();
                if (pt == null)
                    continue;
                if (meta.CustomAttributes != null)
                {
                    var obj = meta.CustomAttributes;
                    var propHidden = obj.GetType().GetProperty("Hidden");
                    if(propHidden != null)
                    {
                        var val = propHidden.GetValue(obj);
                        if (val != null && Boolean.TryParse(val.ToString(), out bool hidden))
                        {
                            if (hidden)
                                continue;
                        }
                    }
                }
                if (!meta.IsSearchable)
                    continue;
                dicValues["label"] = meta.FieldLabel;
                dicValues["id"] = meta.FieldName;
                if (pt == typeof(String))
                {
                    dicValues["type"] = "string";
                    dicValues["input"] = "text";
                    dicValues["operators"] = new string[] { "equal", "not_equal", "begins_with", "ends_with", "contains", "is_empty", "is_not_empty" };
                }
                else if (pt == typeof(Int16) || pt == typeof(Int32) || pt == typeof(Int64)
                    || pt == typeof(UInt16) || pt == typeof(UInt16) || pt == typeof(UInt16)
                    || pt == typeof(Int16?) || pt == typeof(Int32?) || pt == typeof(Int64?)
                    || pt == typeof(UInt16?) || pt == typeof(UInt16?) || pt == typeof(UInt16?))
                {
                    dicValues["type"] = "integer";
                    dicValues["input"] = "number";
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal", "is_null", "is_not_null" };
                    }
                    else
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal" };
                    }
                }
                else if (pt == typeof(Single) || pt == typeof(Double) || pt == typeof(Decimal)
                    || pt == typeof(Single?) || pt == typeof(Double?) || pt == typeof(Decimal?))
                {
                    dicValues["type"] = "double";
                    dicValues["input"] = "number";
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal", "is_null", "is_not_null" };
                    }
                    else
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal" };
                    }
                }
                else if (pt == typeof(DateTime) || pt == typeof(DateTime?))
                {
                    //
                    dicValues["type"] = "datetime";
                    dicValues["input"] = "text";
                    //dicValues["input"] = "datetime-local";
                    //dicValues["validation"] = new { format = "YYYY/MM/DD" };
                    //dicValues["plugin"] = "datepicker";
                    //dicValues["plugin_config"] = new
                    //{
                    //    format = "yyyy/mm/dd",
                    //    todayBtn = "linked",
                    //    todayHighlight = true,
                    //    autoclose = true
                    //};


                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal", "is_null", "is_not_null" };
                    }
                    else
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "less", "less_or_equal", "greater", "greater_or_equal" };
                    }
                }
                else if (pt == typeof(Boolean) || pt == typeof(Boolean?))
                {
                    dicValues["type"] = "boolean";
                    dicValues["input"] = "radio";
                    var values = new Dictionary<int, string>();
                    values.Add(1, "Yes");
                    values.Add(0, "No");
                    dicValues["values"] = values;
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal", "is_null", "is_not_null" };
                    }
                    else
                    {
                        dicValues["operators"] = new string[] { "equal", "not_equal" };
                    }

                }
                else
                {
                    dicValues["type"] = "string";
                    dicValues["input"] = "text";
                    dicValues["operators"] = new string[] { "equal", "not_equal", "begins_with", "ends_with", "contains", "is_empty", "is_not_empty" };
                }
                objLst.Add(dicValues);
            }

            return new
            {
                automationRuleType = type.Name,
                filters = objLst.ToArray()
            };
        }
        //public static jQueryBuilderModel[] FromEntity(AutomationContext ctxt, MsAutomationRule[] dbRules)
        //{
        //    List<jQueryBuilderModel> results = new List<jQueryBuilderModel>();
        //    var ruleGroupIds = dbRules.Select(t => t.RuleGroupId).Distinct().ToArray();
        //    var autoRuleTypeArr = ctxt.GetAutomationRuleTypes();

        //    foreach (var ruleGroupId in ruleGroupIds)
        //    {
        //        var dbGroupRules = dbRules.Where(t => t.RuleGroupId == ruleGroupId).OrderBy(t => t.ParentRuleId).ThenBy(t => t.RuleId).ToArray();
        //        var firstRule = dbGroupRules.FirstOrDefault();
        //        var dicRule = new Dictionary<MsAutomationRule, jQueryBuilderBaseRuleItemModel>();
        //        var autoRuleType = autoRuleTypeArr.Where(t => t.Name == firstRule.ReferenceType).First();
        //        var qb = new jQueryBuilderModel()
        //        {
        //            ruleGroupId = ruleGroupId,
        //            referenceType = dbGroupRules.Where(t => !string.IsNullOrEmpty(t.ReferenceType)).Select(t => t.ReferenceType).First()
        //        };


        //        qb.ruleData = new jQueryBuilderRuleSetModel()
        //        {
        //            condition = Enum.Parse<ExpressionLogicalOperator>(firstRule.LogicalOperator).ToQBCondition(),
        //            data = new jQueryBuilderUserData() { referenceId = -ruleGroupId },
        //            rules = new List<jQueryBuilderBaseRuleItemModel>()
        //        };
        //        foreach (var dbGroupRule in dbGroupRules)
        //        {
        //            if (dbGroupRule.IsBracket)
        //            {
        //                dicRule.Add(dbGroupRule, new jQueryBuilderRuleSetModel()
        //                {
        //                    condition = Enum.Parse<ExpressionLogicalOperator>(dbGroupRule.LogicalOperator).ToQBCondition(),
        //                    data = new jQueryBuilderUserData() { referenceId = dbGroupRule.RuleId },
        //                    rules = new List<jQueryBuilderBaseRuleItemModel>()
        //                });
        //            }
        //            else
        //            {
        //                var propType = autoRuleType.RuleProperties.Where(t => t.PropertyInfo.Name == dbGroupRule.ReferenceName).Select(t => t.PropertyInfo.PropertyType).FirstOrDefault();
        //                if (propType == null)
        //                    continue;
        //                var rule = new jQueryBuilderRuleModel()
        //                {
        //                    field = dbGroupRule.ReferenceName,
        //                    id = dbGroupRule.ReferenceName,
        //                    @operator = Enum.Parse<ExpressionOperator>(dbGroupRule.Operator).ToQBOperator(propType),
        //                    input = propType.ToQBInput(),
        //                    type = propType.ToQBType(),
        //                    value = dbGroupRule.CompareValue == null ? null : JsonConvert.DeserializeObject(dbGroupRule.CompareValue, propType),
        //                    data = new jQueryBuilderUserData() { referenceId = dbGroupRule.RuleId }
        //                };
        //                dicRule.Add(dbGroupRule, rule);
        //            }
        //        }
        //        foreach (var dbGroupRule in dbGroupRules)
        //        {
        //            if (!dbGroupRule.ParentRuleId.HasValue)
        //                continue;
        //            var par = dbGroupRules.Where(t => t.RuleId == dbGroupRule.ParentRuleId.Value).FirstOrDefault();
        //            if (par != null)
        //            {
        //                jQueryBuilderRuleSetModel parent = dicRule[par] as jQueryBuilderRuleSetModel;
        //                parent.rules.Add(dicRule[dbGroupRule]);
        //            }
        //        }

        //        qb.ruleData.rules = (from k in dicRule.Keys
        //                             where k.ParentRuleId == null
        //                             select dicRule[k]).ToList();
        //        results.Add(qb);
        //    }
        //    return results.ToArray();
        //}
    }

    public class jQueryBuilderRuleSetModel : jQueryBuilderBaseRuleItemModel
    {
        public string condition { get; set; }
        public List<jQueryBuilderBaseRuleItemModel> rules { get; set; }

    }
    public class jQueryBuilderRuleModel : jQueryBuilderBaseRuleItemModel
    {
        public string id { get; set; }
        public string field { get; set; }
        public string type { get; set; }
        public string input { get; set; }
        public string @operator { get; set; }
        public object value { get; set; }
    }
    public class jQueryBuilderUserData
    {
        public int referenceId { get; set; }
    }

    public class jQueryBuilderJSONContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(jQueryBuilderBaseRuleItemModel).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }
    public class jQueryBuilderJSONConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new jQueryBuilderJSONContractResolver() };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(jQueryBuilderBaseRuleItemModel);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo.ContainsKey("condition"))
            {
                return JsonConvert.DeserializeObject<jQueryBuilderRuleSetModel>(jo.ToString(), SpecifiedSubclassConversion);
            }
            else
            {
                return JsonConvert.DeserializeObject<jQueryBuilderRuleModel>(jo.ToString(), SpecifiedSubclassConversion);
            }
            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }

    public static class jQueryBuilderUtils
    {
        public static string ToQBCondition(this ExpressionLogicalOperator value)
        {
            switch (value)
            {
                case ExpressionLogicalOperator.And:
                    return ExpressionLogicalOperator.And.ToString().ToUpper();
                case ExpressionLogicalOperator.Or:
                    return ExpressionLogicalOperator.Or.ToString().ToUpper();
                default:
                    return null;
            }
        }
        public static string ToQBOperator(this ExpressionOperator value, Type propertyType)
        {
            switch (value)
            { 
                case ExpressionOperator.Equal:
                    return "equal";
                case ExpressionOperator.NotEqual:
                    return "not_equal";
                case ExpressionOperator.GreaterThan:
                    return "greater";
                case ExpressionOperator.GreaterThanOrEqual:
                    return "greater_or_equal";
                case ExpressionOperator.LessThan:
                    return "less";
                case ExpressionOperator.LessThanOrEqual:
                    return "less_or_equal";
                case ExpressionOperator.Contains:
                    return "contains";
                case ExpressionOperator.StartsWith:
                    return "begins_with";
                case ExpressionOperator.EndsWith:
                    return "ends_with";
                case ExpressionOperator.In:
                    return "in";
                case ExpressionOperator.NotIn:
                    return "not_in";
                case ExpressionOperator.IsNullOrEmpty:
                    if (propertyType == typeof(string))
                        return "is_empty";
                    else
                        return "is_null";
                case ExpressionOperator.IsNotNullOrEmpty:
                    if (propertyType == typeof(string))
                        return "is_not_empty";
                    else
                        return "is_not_null";
                default:
                    return null;
            }
        }
        public static string ToQBType(this Type propertyType)
        {
            string result = string.Empty;
            if (propertyType == typeof(string))
            {
                result = "string";
            }
            else if (propertyType == typeof(Int16) || propertyType == typeof(Int32) || propertyType == typeof(Int64)
                || propertyType == typeof(UInt16) || propertyType == typeof(UInt16) || propertyType == typeof(UInt16)
                || propertyType == typeof(Int16?) || propertyType == typeof(Int32?) || propertyType == typeof(Int64?)
                || propertyType == typeof(UInt16?) || propertyType == typeof(UInt16?) || propertyType == typeof(UInt16?))
            {
                result = "integer";
            }
            else if (propertyType == typeof(Single) || propertyType == typeof(Double) || propertyType == typeof(Decimal)
                || propertyType == typeof(Single?) || propertyType == typeof(Double?) || propertyType == typeof(Decimal?))
            {
                result = "double";
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
            }
            else if (propertyType == typeof(Boolean) || propertyType == typeof(Boolean?))
            {
                result = "boolean";
            }
            else
            {
                result = "string";
            }
            return result;
        }
        public static string ToQBInput(this Type propertyType)
        {
            string result = string.Empty;
            if (propertyType == typeof(string))
            {
                result = "text";
            }
            else if (propertyType == typeof(Int16) || propertyType == typeof(Int32) || propertyType == typeof(Int64)
                || propertyType == typeof(UInt16) || propertyType == typeof(UInt16) || propertyType == typeof(UInt16)
                || propertyType == typeof(Int16?) || propertyType == typeof(Int32?) || propertyType == typeof(Int64?)
                || propertyType == typeof(UInt16?) || propertyType == typeof(UInt16?) || propertyType == typeof(UInt16?))
            {
                result = "number";
            }
            else if (propertyType == typeof(Single) || propertyType == typeof(Double) || propertyType == typeof(Decimal)
                || propertyType == typeof(Single?) || propertyType == typeof(Double?) || propertyType == typeof(Decimal?))
            {
                result = "number";
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                //result = "date";
                result = "text";
            }
            else if (propertyType == typeof(Boolean) || propertyType == typeof(Boolean?))
            {
                result = "radio";
            }
            else
            {
                result = "text";
            }
            return result;
        }
        public static ExpressionOperator ConvertToOperator(string @operator)
        {

            //equal: { type: 'equal',            nb_inputs: 1, multiple: false, apply_to:['string', 'number', 'datetime', 'boolean'] },
            //not_equal: { type: 'not_equal',        nb_inputs: 1, multiple: false, apply_to:['string', 'number', 'datetime', 'boolean'] },
            //in:               { type: 'in',               nb_inputs: 1, multiple: true,  apply_to:['string', 'number', 'datetime'] },
            //not_in: { type: 'not_in',           nb_inputs: 1, multiple: true,  apply_to:['string', 'number', 'datetime'] },
            //less: { type: 'less',             nb_inputs: 1, multiple: false, apply_to:['number', 'datetime'] },
            //less_or_equal: { type: 'less_or_equal',    nb_inputs: 1, multiple: false, apply_to:['number', 'datetime'] },
            //greater: { type: 'greater',          nb_inputs: 1, multiple: false, apply_to:['number', 'datetime'] },
            //greater_or_equal: { type: 'greater_or_equal', nb_inputs: 1, multiple: false, apply_to:['number', 'datetime'] },
            //between: { type: 'between',          nb_inputs: 2, multiple: false, apply_to:['number', 'datetime'] },
            //not_between: { type: 'not_between',      nb_inputs: 2, multiple: false, apply_to:['number', 'datetime'] },
            //begins_with: { type: 'begins_with',      nb_inputs: 1, multiple: false, apply_to:['string'] },
            //not_begins_with: { type: 'not_begins_with',  nb_inputs: 1, multiple: false, apply_to:['string'] },
            //contains: { type: 'contains',         nb_inputs: 1, multiple: false, apply_to:['string'] },
            //not_contains: { type: 'not_contains',     nb_inputs: 1, multiple: false, apply_to:['string'] },
            //ends_with: { type: 'ends_with',        nb_inputs: 1, multiple: false, apply_to:['string'] },
            //not_ends_with: { type: 'not_ends_with',    nb_inputs: 1, multiple: false, apply_to:['string'] },
            //is_empty: { type: 'is_empty',         nb_inputs: 0, multiple: false, apply_to:['string'] },
            //is_not_empty: { type: 'is_not_empty',     nb_inputs: 0, multiple: false, apply_to:['string'] },
            //is_null: { type: 'is_null',          nb_inputs: 0, multiple: false, apply_to:['string', 'number', 'datetime', 'boolean'] },
            //is_not_null: { type: 'is_not_null',      nb_inputs: 0, multiple: false, apply_to:['string', 'number', 'datetime', 'boolean'] }
            ExpressionOperator result = ExpressionOperator.Equal;
            switch (@operator.ToLower())
            {
                case "equal":
                    result = ExpressionOperator.Equal;
                    break;
                case "not_equal":
                    result = ExpressionOperator.NotEqual;
                    break;
                case "in":
                    result = ExpressionOperator.In;
                    break;
                case "not_in":
                    result = ExpressionOperator.NotIn;
                    break;
                case "less":
                    result = ExpressionOperator.LessThan;
                    break;
                case "less_or_equal":
                    result = ExpressionOperator.LessThanOrEqual;
                    break;
                case "greater":
                    result = ExpressionOperator.GreaterThan;
                    break;
                case "greater_or_equal":
                    result = ExpressionOperator.GreaterThanOrEqual;
                    break;
                case "begins_with":
                    result = ExpressionOperator.StartsWith;
                    break;
                case "contains":
                    result = ExpressionOperator.Contains;
                    break;
                case "not_contains":
                    result = ExpressionOperator.NotContains;
                    break;
                case "ends_with":
                    result = ExpressionOperator.EndsWith;
                    break;
                case "is_empty":
                case "is_null":
                    result = ExpressionOperator.IsNullOrEmpty;
                    break;
                case "is_not_empty":
                case "is_not_null":
                    result = ExpressionOperator.IsNotNullOrEmpty;
                    break;
                default:
                    break;
            }
            return result;
        }
        public static ExpressionLogicalOperator ConvertToLogicalOperator(string condition)
        {
            ExpressionLogicalOperator result = ExpressionLogicalOperator.And;
            switch (condition.ToLower())
            {
                case "and":
                    result = ExpressionLogicalOperator.And;
                    break;
                case "or":
                    result = ExpressionLogicalOperator.Or;
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}

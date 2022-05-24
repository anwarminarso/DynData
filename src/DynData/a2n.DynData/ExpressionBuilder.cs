using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SysJsonSerial = System.Text.Json.Serialization;

#nullable disable

namespace a2n.DynData
{
    public class ExpressionBuilder
    {
        private static readonly MethodInfo MethodStringContainsIgnoreCase = typeof(String).GetMethod(nameof(String.Contains), new Type[] { typeof(String), typeof(StringComparison) });
        private static readonly MethodInfo MethodStringContains = typeof(String).GetMethod(nameof(String.Contains), new Type[] { typeof(String) });
        private static readonly MethodInfo MethodStringStartsWith = typeof(String).GetMethod(nameof(String.StartsWith), new Type[] { typeof(String) });
        private static readonly MethodInfo MethodStringEndsWith = typeof(String).GetMethod(nameof(String.EndsWith), new Type[] { typeof(String) });
        private static Expression GenerateExpression(ExpressionRule currentRule, ParameterExpression itemExpression)
        {
            Expression result = null;
            if (!currentRule.IsBracket)
            {
                var prop = Expression.Property(itemExpression, currentRule.ReferenceFieldName);
                object value = null;
                if (currentRule.CompareFieldObject == null)
                    value = currentRule.ConvertToObjectValue();
                else
                    value = currentRule.CompareFieldObject;
                switch (currentRule.Operator)
                {
                    case ExpressionOperator.Equal:
                    case ExpressionOperator.NotEqual:
                    case ExpressionOperator.GreaterThan:
                    case ExpressionOperator.GreaterThanOrEqual:
                    case ExpressionOperator.LessThan:
                    case ExpressionOperator.LessThanOrEqual:
                        result = GenerateConstantExpression(prop, currentRule.Operator, value);
                        break;
                    case ExpressionOperator.StartsWith:
                        result = Expression.Call(prop, MethodStringStartsWith, Expression.Constant(value, typeof(string)));
                        break;
                    case ExpressionOperator.EndsWith:
                        result = Expression.Call(prop, MethodStringEndsWith, Expression.Constant(value, typeof(string)));
                        break;
                    case ExpressionOperator.Contains:
                        result = Expression.Call(prop, MethodStringContains, Expression.Constant(value, typeof(string)));
                        break;
                    case ExpressionOperator.ContainsIgnoreCase:
                        result = Expression.Call(prop, MethodStringContainsIgnoreCase, Expression.Constant(value, typeof(string)), Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));
                        break;
                    case ExpressionOperator.NotContains:
                        {
                            var exp = Expression.Call(prop, MethodStringContains, Expression.Constant(value, typeof(string)));
                            result = Expression.Not(exp);
                        }
                        break;
                    case ExpressionOperator.In:
                        {
                            var mtd = typeof(List<>).MakeGenericType(currentRule.ReferenceFieldType).GetMethod(nameof(String.Contains));
                            result = Expression.Call(Expression.Constant(value), mtd, prop);
                        }
                        break;
                    case ExpressionOperator.NotIn:
                        {
                            var mtd = typeof(List<>).MakeGenericType(currentRule.ReferenceFieldType).GetMethod(nameof(String.Contains));
                            var expIn = Expression.Call(Expression.Constant(value), mtd, prop);
                            result = Expression.Not(expIn);
                        }
                        break;
                    case ExpressionOperator.IsNullOrEmpty:
                        {
                            if (prop.Type == typeof(string))
                            {
                                var exp1 = GenerateConstantExpression(prop, ExpressionOperator.Equal, null);
                                var exp2 = GenerateConstantExpression(prop, ExpressionOperator.Equal, string.Empty);
                                result = Expression.OrElse(exp1, exp2);
                            }
                            else
                            {
                                result = GenerateConstantExpression(prop, ExpressionOperator.Equal, null);
                            }
                        }
                        break;
                    case ExpressionOperator.IsNotNullOrEmpty:
                        {
                            if (prop.Type == typeof(string))
                            {
                                var exp1 = GenerateConstantExpression(prop, ExpressionOperator.Equal, null);
                                var exp2 = GenerateConstantExpression(prop, ExpressionOperator.Equal, string.Empty);
                                var exp3 = Expression.OrElse(exp1, exp2);
                                result = Expression.Not(exp3);
                            }
                            else
                            {
                                result = GenerateConstantExpression(prop, ExpressionOperator.NotEqual, null);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }


            if (currentRule.Children.Length > 0)
            {
                List<Expression> expChild = new List<Expression>();
                Expression childExpression = null;
                foreach (var child in currentRule.Children)
                {
                    if (childExpression == null)
                        childExpression = GenerateExpression(child, itemExpression);
                    else
                    {
                        switch (child.LogicalOperator)
                        {
                            case ExpressionLogicalOperator.And:
                                childExpression = Expression.AndAlso(childExpression, GenerateExpression(child, itemExpression));
                                break;
                            case ExpressionLogicalOperator.Or:
                                childExpression = Expression.OrElse(childExpression, GenerateExpression(child, itemExpression));
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (currentRule.IsBracket)
                {
                    result = childExpression;
                }
                else
                {
                    switch (currentRule.Children[0].LogicalOperator)
                    {
                        case ExpressionLogicalOperator.And:
                            result = Expression.AndAlso(result, childExpression);
                            break;
                        case ExpressionLogicalOperator.Or:
                            result = Expression.OrElse(result, childExpression);
                            break;
                        default:
                            break;
                    }
                }
            }
            return result;
        }
        private static Expression GenerateConstantExpression(Expression prop, ExpressionOperator opr, object value)
        {
            Expression result = null;
            Expression expValue = Expression.Constant(value);
            if (IsNullableType(prop.Type) && !IsNullableType(expValue.Type))
                expValue = Expression.Convert(expValue, prop.Type);
            else if (!IsNullableType(prop.Type) && IsNullableType(expValue.Type))
                prop = Expression.Convert(prop, expValue.Type);
            switch (opr)
            {
                case ExpressionOperator.Equal:
                    result = Expression.Equal(prop, expValue);
                    break;
                case ExpressionOperator.NotEqual:
                    result = Expression.NotEqual(prop, expValue);
                    break;
                case ExpressionOperator.GreaterThan:
                    result = Expression.GreaterThan(prop, expValue);
                    break;
                case ExpressionOperator.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(prop, expValue);
                    break;
                case ExpressionOperator.LessThan:
                    result = Expression.LessThan(prop, expValue);
                    break;
                case ExpressionOperator.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(prop, expValue);
                    break;
                case ExpressionOperator.IsNullOrEmpty:
                case ExpressionOperator.IsNotNullOrEmpty:
                case ExpressionOperator.In:
                case ExpressionOperator.NotIn:
                case ExpressionOperator.Contains:
                case ExpressionOperator.StartsWith:
                case ExpressionOperator.EndsWith:
                default:
                    throw new NotSupportedException();
            }
            return result;
        }
        private static bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        public static Expression<Func<T, bool>> Build<T>(params ExpressionRule[] rules)
        {
            if (rules == null)
                throw new ArgumentException(nameof(rules));

            ParameterExpression itemExpression = Expression.Parameter(typeof(T), "q");
            foreach (var rule in rules)
                rule.UpdateParent();
            var rootRules = rules.Where(t => t.Parent == null).OrderBy(t => t.OrderId).ToArray();
            Expression conditions = null;
            foreach (var root in rootRules)
            {
                if (conditions == null)
                    conditions = GenerateExpression(root, itemExpression);
                else
                {
                    switch (root.LogicalOperator)
                    {
                        case ExpressionLogicalOperator.And:
                            conditions = Expression.AndAlso(conditions, GenerateExpression(root, itemExpression));
                            break;
                        case ExpressionLogicalOperator.Or:
                            conditions = Expression.OrElse(conditions, GenerateExpression(root, itemExpression));
                            break;
                        default:
                            break;
                    }
                }
            }
            if (conditions != null)
            {
                if (conditions.CanReduce)
                    conditions = conditions.ReduceAndCheck();
                var query = Expression.Lambda<Func<T, bool>>(conditions, itemExpression);
                return query;
            }
            else
                return null;
        }
        public static object Build(Type type, params ExpressionRule[] rules)
        {
            var mtd = typeof(ExpressionBuilder).GetMethod("Build", new Type[] { typeof(ExpressionRule[]) }).MakeGenericMethod(type);
            return mtd.Invoke(null, new object[] { rules });
        }
    }
    public class ExpressionRule
    {
        private List<ExpressionRule> _children = new List<ExpressionRule>();

        [JsonConverter(typeof(StringEnumConverter))]
        [SysJsonSerial.JsonConverter(typeof(SysJsonSerial.JsonStringEnumConverter))]
        [EnumDataType(typeof(ExpressionLogicalOperator))]
        public ExpressionLogicalOperator LogicalOperator { get; set; }

        [JsonIgnore]
        public ExpressionRule Parent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ExpressionRule[] Children
        {
            get
            {
                return _children.ToArray();
            } 
            set
            {
                if (value != null)
                    _children = new List<ExpressionRule>(value);
                else
                    _children = new List<ExpressionRule>();
            }
        } 

        public int OrderId { get; set; }
        public bool IsBracket { get; set; }
        public string ReferenceFieldName { get; set; }

        [JsonIgnore]
        [SysJsonSerial.JsonIgnore()]
        public Type ReferenceFieldType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [SysJsonSerial.JsonConverter(typeof(SysJsonSerial.JsonStringEnumConverter))]
        [EnumDataType(typeof(ExpressionOperator))]
        public ExpressionOperator Operator { get; set; }

        public void AddChild(ExpressionRule value)
        {
            //value.Parent = this;
            _children.Add(value);
        }
        public void RemoveChild(ExpressionRule value)
        {
            if (_children.Contains(value))
            {
                _children.Remove(value);
                value.Parent = null;
            }
        }

        public string CompareFieldValue { get; set; }

        //private object _CompareFieldObject;
        //public object CompareFieldObject
        //{
        //    get
        //    {
        //        if (_CompareFieldObject == null)
        //            _CompareFieldObject = ConvertToObjectValue();
        //        return _CompareFieldObject;
        //    }
        //    set { _CompareFieldObject = value; }
        //}
        //private object _CompareFieldObject;
        public object CompareFieldObject { get; set; }
        internal object ConvertToObjectValue()
        {
            object result = null;
            switch (this.Operator)
            {
                case ExpressionOperator.Equal:
                case ExpressionOperator.NotEqual:
                case ExpressionOperator.GreaterThan:
                case ExpressionOperator.GreaterThanOrEqual:
                case ExpressionOperator.LessThan:
                case ExpressionOperator.LessThanOrEqual:
                case ExpressionOperator.Contains:
                case ExpressionOperator.StartsWith:
                case ExpressionOperator.EndsWith:
                    {
                        if (ReferenceFieldType == typeof(string))
                        {
                            if (!string.IsNullOrEmpty(CompareFieldValue) && CompareFieldValue.StartsWith("\"") && CompareFieldValue.EndsWith("\""))
                                result = JsonConvert.DeserializeObject(CompareFieldValue, ReferenceFieldType);
                            else
                                result = CompareFieldValue;
                        }
                        else if (ReferenceFieldType == typeof(DateTime) || ReferenceFieldType == typeof(DateTime?))
                        {
                            if (!string.IsNullOrEmpty(CompareFieldValue) && CompareFieldValue.StartsWith("\"") && CompareFieldValue.EndsWith("\""))
                                result = JsonConvert.DeserializeObject(CompareFieldValue, ReferenceFieldType);
                            else if (!string.IsNullOrEmpty(CompareFieldValue))
                                result = Convert.ToDateTime(CompareFieldValue);
                            else if (ReferenceFieldType == typeof(DateTime?))
                                result = default(DateTime?);
                            else
                                throw new Exception("Failed to cast datetime");
                        }
                        else
                            result = JsonConvert.DeserializeObject(CompareFieldValue, ReferenceFieldType);
                    }
                    break;
                case ExpressionOperator.In:
                case ExpressionOperator.NotIn:
                    {
                        result = JsonConvert.DeserializeObject(CompareFieldValue, typeof(List<>).MakeGenericType(ReferenceFieldType));
                    }
                    break;
                default:
                    break;
            }
            return result;
        }

        public ExpressionRule[] GetAllChildren()
        {
            List<ExpressionRule> rules = new List<ExpressionRule>();
            if (this.Children.Length > 0)
            {
                foreach (var child in this.Children)
                {
                    rules.Add(child);
                    rules.AddRange(child.GetAllChildren());
                }
            }
            return rules.ToArray();
        }


        public void UpdateParent()
        {
            foreach (var child in this._children)
            {
                child.Parent = this;
                child.UpdateParent();
            }
        }


        public void ValidatePropertyType(Type baseType)
        {
            var propArr = baseType.GetProperties();
            ValidatePropertyType(propArr);
        }
        public void ValidatePropertyType(PropertyInfo[] propArr)
        {
            ValidatePropertyType(this, propArr);
        }
        private void ValidatePropertyType(ExpressionRule rule, PropertyInfo[] propArr)
        {
            if (!rule.IsBracket && rule.ReferenceFieldType == null)
            {
                var prop = propArr.Where(t => t.Name == rule.ReferenceFieldName).FirstOrDefault();
                if (prop == null)
                    throw new Exception($"Property {rule.ReferenceFieldName} not found");
                rule.ReferenceFieldType = prop.PropertyType;
            }
            foreach (var child in rule.Children)
                ValidatePropertyType(child, propArr);
        }
    }
    public enum ExpressionLogicalOperator
    {
        And,
        Or
    }
    public enum ExpressionOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Contains,
        ContainsIgnoreCase,
        NotContains,
        StartsWith,
        EndsWith,
        In,
        NotIn,
        IsNullOrEmpty,
        IsNotNullOrEmpty
    }
}

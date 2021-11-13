
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
        public object ToWhereExpression(Type type, PropertyInfo[] propArr)
        {
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
            return ExpressionBuilder.Build(type, rootRule);
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

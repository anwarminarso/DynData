using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

using Sample.WebUI.Configuration;
using Sample.DataAccess;
using a2n.DynData;
using Newtonsoft.Json.Linq;

#nullable disable

namespace Sample.WebUI
{
    [ApiController]
    public class DynDataController : ControllerBase
    {
        private readonly ILogger<DynDataController> logger;
        private readonly AdventureWorksContext db;
        private readonly QueryTemplateSettings qryTpl;

        public DynDataController(ILogger<DynDataController> logger, AdventureWorksContext db, QueryTemplateSettings qryTpl)
        {
            this.logger = logger;
            this.db = db;
            this.qryTpl = qryTpl;
        }

        [Route("/api/dyndata/{viewName}/datatable")]
        [HttpPost]
        public Task<DataTableJSResponse> GetDataTable(string viewName, [FromForm] DataTableJSRequest req)
        {
            return Task.Run(() =>
            {
                var qry = qryTpl.GetQuery(db, viewName);
                Type valueType = null;
                PropertyInfo[] propArr = null;
                if (qry != null)
                {
                    valueType = qryTpl.GetValueType(db, viewName);
                    propArr = qryTpl.GetProperties(db, viewName);
                }
                else
                {
                    valueType = db.GetTableType(viewName);
                    if (valueType != null)
                    {
                        propArr = db.GetProperties(viewName);
                        qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
                    }
                }
                if (qry != null)
                {
                    var page = req.ToPagingResult(qry, valueType, propArr);
                    var resp = new DataTableJSResponse(req, page);
                    return resp;
                }
                else
                    return new DataTableJSResponse(req, new PagingResult<dynamic>());
            });
        }


        [Route("/api/dyndata/{viewName}/list")]
        [HttpPost]
        public Task<PagingResult<dynamic>> GetList(string viewName, PagingRequest req)
        {
            return Task.Run(() =>
            {
                var qry = qryTpl.GetQuery(db, viewName, req.rules);
                if (qry == null)
                    qry = db.Query(viewName, req.rules);
                if (qry != null)
                {
                    if (!string.IsNullOrEmpty(req.orderBy))
                    {
                        var asc = req.ascending.HasValue ? req.ascending.Value : true;
                        qry = qry.OrderBy(req.orderBy, asc);
                    }
                    return qry.ToPagingResult(req.pageSize, req.pageIndex);
                }
                else
                    return new PagingResult<dynamic>();
            });
        }


        [Route("/api/dyndata/{viewName}/read")]
        [HttpPost]
        public object ReadRecord(string viewName, System.Text.Json.JsonElement data)
        {
            JObject jObj = JObject.Parse(data.ToString());
            if (jObj.Properties().Count() == 0)
                return null;

            var qry = qryTpl.GetQuery(db, viewName);
            if (qry != null)
            {
                Type valueType = null;
                Metadata[] metaArr = null;
                ExpressionRule rootRule = new ExpressionRule() { IsBracket = true, LogicalOperator = ExpressionLogicalOperator.And };
                valueType = qryTpl.GetValueType(db, viewName);
                metaArr = qryTpl.GetMetadata(db, viewName);
                var pkValues = metaArr.Where(t => jObj.ContainsKey(t.FieldName)).Select(t => new { Type = t.PropertyInfo.PropertyType, Name = t.FieldName, Value = jObj[t.FieldName] }).ToArray();

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
            else
            {
                return db.FindByKey(viewName, jObj);
            }
        }

        [Route("/api/dyndata/{viewName}/create")]
        [HttpPost]
        public object[] CreateRecord(string viewName, System.Text.Json.JsonElement value)
        {
            var dataArr = db.Create(viewName, value);
            db.SaveChanges();
            return dataArr;
        }

        [Route("/api/dyndata/{viewName}/update")]
        [HttpPost]
        public object[] UpdateRecord(string viewName, System.Text.Json.JsonElement value)
        {
            var results = db.Update(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("/api/dyndata/{viewName}/delete")]
        [HttpPost]
        public object[] DeleteRecord(string viewName, System.Text.Json.JsonElement value)
        {
            var results = db.Delete(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("/api/dyndata/{viewName}/metadata")]
        [HttpPost]
        [HttpGet]
        public Metadata[] GetMetadata(string viewName)
        {
            Metadata[] results = new Metadata[0];
            results = qryTpl.GetMetadata(db, viewName);
            if (results == null)
            {
                var tableType = db.GetTableType(viewName);
                if (tableType != null)
                {
                    results = db.GetMetadata(viewName);
                }
                else
                    results = new Metadata[0];
            }

            return results;
        }

        [Route("/api/dyndata/{viewName}/metadataQB")]
        [HttpPost]
        [HttpGet]
        public object GetMetadataQB(string viewName)
        {
            Type tableType = null;
            Metadata[] results = new Metadata[0];
            results = qryTpl.GetMetadata(db, viewName);
            if (results != null)
            {
                tableType = qryTpl.GetValueType(db, viewName);
            }
            else
            {
                tableType = db.GetTableType(viewName);
                if (tableType != null)
                {
                    results = db.GetMetadata(viewName);
                }
                else
                    results = new Metadata[0];
            }

            return new
            {
                metaData = results,
                isTable = results.Where(t => t.IsPrimaryKey).FirstOrDefault(),
                queryBuilderOptions = jQueryBuilderModel.GenerateFilterOptions(tableType, results)
            };
        }

    }
}

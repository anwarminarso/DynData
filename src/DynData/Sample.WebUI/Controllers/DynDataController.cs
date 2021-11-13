using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using Newtonsoft.Json;

using Sample.WebUI.Configuration;
using Sample.DataAccess;
using a2n.DynData;

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
                        var pkNames = db.GetPKNames(valueType);
                        qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
                    }
                }
                if (qry != null)
                {
                    var whereExp = req.ToWhereExpression(valueType, propArr);
                    int pageIndex = 0;
                    if (req.length > 0)
                        pageIndex = req.start / req.length;
                    if (whereExp != null)
                        qry = qry.Where(whereExp, valueType);
                    if (!string.IsNullOrEmpty(req.jsonQB))
                    {
                        var queryBuilder = JsonConvert.DeserializeObject<jQueryBuilderModel>(req.jsonQB);
                        if (queryBuilder != null && queryBuilder.ruleData != null)
                            qry = qry.Where(queryBuilder.ToWhereExpression(valueType, propArr), valueType);
                    }
                    if (req.order != null && req.order.Length > 0)
                    {
                        var ColOrderBy = req.columns[req.order[0].column].name;
                        bool ascending = true;
                        if (!string.IsNullOrEmpty(req.order[0].dir) && req.order[0].dir != "asc")
                            ascending = false;
                        qry = qry.OrderBy(ColOrderBy, valueType, ascending);
                        if (req.order.Length > 1)
                        {
                            for (int i = 1; i < req.order.Length; i++)
                            {
                                ascending = true;
                                ColOrderBy = req.columns[req.order[i].column].name;
                                if (!string.IsNullOrEmpty(req.order[i].dir) && req.order[i].dir != "asc")
                                    ascending = false;
                                qry = qry.ThenBy(ColOrderBy, valueType, ascending);
                            }
                        }
                    }
                    var page = qry.GetPagingResult(req.length, pageIndex);
                    var resp = new DataTableJSResponse(req, page);
                    return resp;
                }
                else
                    return new DataTableJSResponse(req, new PagingResult<dynamic>());
            });
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
                    var piArr = db.GetProperties(viewName);
                    results = db.GetMetadata(viewName);
                }
                else
                    results = new Metadata[0];
            }

            return new
            {
                metaData = results,
                queryBuilderOptions = jQueryBuilderModel.GenerateFilterOptions(tableType, results)
            };
        }
    }
}

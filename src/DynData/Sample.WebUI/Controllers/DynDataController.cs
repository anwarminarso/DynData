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
                Metadata[] metaArr = null;
                if (qry != null)
                {
                    valueType = qryTpl.GetValueType(db, viewName);
                    metaArr = qryTpl.GetMetadata(db, viewName);
                }
                else
                {
                    valueType = db.GetTableType(viewName);
                    if (valueType != null)
                    {
                        metaArr = db.GetMetadata(viewName);
                        qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
                    }
                }
                if (qry != null)
                {
                    var page = req.ToPagingResult(qry, valueType, metaArr);
                    var resp = new DataTableJSResponse(req, page);
                    return resp;
                }
                else
                    return new DataTableJSResponse(req, new PagingResult<dynamic>());
            });
        }



        [Route("/api/dyndata/{viewName}/datatable/export")]
        [HttpPost]
        public IActionResult ExportDataTable(string viewName, [FromForm] DataTableJSExportRequest req)
        {
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format.ToLower();
            var qry = qryTpl.GetQuery(db, viewName);
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;
            if (qry != null)
            {
                valueType = qryTpl.GetValueType(db, viewName);
                propArr = qryTpl.GetProperties(db, viewName);
                metadataArr = qryTpl.GetMetadata(db, viewName);
            }
            else
            {
                valueType = db.GetTableType(viewName);
                if (valueType != null)
                {
                    qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
                    propArr = db.GetProperties(viewName);
                    metadataArr = db.GetMetadata(viewName);
                }
            }
            if (qry != null)
            {
                qry = req.ToQueryable(qry, valueType, metadataArr);
                byte[] buffer = null;
                switch (format)
                {
                    case "xlsx":
                        {
                            using (var ms = new MemoryStream())
                            {
                                qry.ExportToExcel(valueType, metadataArr, ms);
                                buffer = ms.ToArray();
                            }

                            return File(buffer, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{viewName.ToHumanReadable()}.xlsx");
                        }
                    case "pdf":
                        {
                            //not implemented
                            //coming soon
                            return NotFound();
                        }
                    case "csv":
                    default:
                        {
                            using (var ms = new MemoryStream())
                            {
                                using (var sr = new StreamWriter(ms))
                                {
                                    qry.ExportToCSV(metadataArr, sr);
                                }
                                buffer = ms.ToArray();
                            }
                            return File(buffer, "text/csv", $"{viewName.ToHumanReadable()}.csv");
                        }
                }
            }
            return NotFound();
        }

        [Route("/api/dyndata/{viewName}/list")]
        [HttpPost]
        public Task<PagingResult<dynamic>> GetList(string viewName, PagingRequest req)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
                if (qryTpl.HasQueryName(db, viewName))
                    qry = qryTpl.GetQuery(db, viewName, req.rules);
                else
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


        [Route("/api/dyndata/{viewName}/dropdown")]
        [HttpGet]
        public Task<PagingResult<dynamic>> GetDropDown(string viewName, string keyField, string labelField, string search, int pageIndex, int pageSize)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
                Type valueType = null;
                ExpressionRule rule = null;
                if (!string.IsNullOrEmpty(search))
                {
                    rule = new ExpressionRule()
                    {
                        IsBracket = false,
                        ReferenceFieldName = labelField,
                        Operator = ExpressionOperator.Contains,
                        CompareFieldValue = search
                    };
                }
                if (qryTpl.HasQueryName(db, viewName))
                {
                    valueType = qryTpl.GetValueType(db, viewName);
                    qry = qryTpl.GetQuery(db, viewName, rule);
                }
                else
                {
                    valueType = db.GetTableType(viewName);
                    qry = db.Query(viewName, rule);
                }
                if (qry != null)
                {
                    //return qry.Select(valueType, keyField, labelField).ToPagingResult(pageSize, pageIndex);
                    return qry.ToPagingResult(pageSize, pageIndex);
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
            if (qryTpl.HasQueryName(db, viewName))
            {
                return qryTpl.FindByKey(db, viewName, jObj);
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
        public MetadataInfo GetMetadata(string viewName)
        {
            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            results = qryTpl.GetMetadata(db, viewName);
            if (results != null)
            {
                var crudTableType = qryTpl.GetCRUDTableType(db, viewName);
                if (crudTableType != null)
                    crudTableName = crudTableType.Name;
            }
            else
            {
                var tableType = db.GetTableType(viewName);
                if (tableType != null)
                {
                    results = db.GetMetadata(viewName);
                    crudTableName = tableType.Name;
                }
                else
                    results = new Metadata[0];
            }

            return new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName
            };
        }

        [Route("/api/dyndata/{viewName}/metadataQB")]
        [HttpPost]
        [HttpGet]
        public MetadataInfo GetMetadataQB(string viewName)
        {
            Type tableType = null;
            Metadata[] results = new Metadata[0];
            results = qryTpl.GetMetadata(db, viewName);
            string crudTableName = null;
            if (results != null)
            {
                tableType = qryTpl.GetValueType(db, viewName);
                var crudTableType = qryTpl.GetCRUDTableType(db, viewName);
                if (crudTableType != null)
                    crudTableName = crudTableType.Name;
            }
            else
            {
                tableType = db.GetTableType(viewName);
                if (tableType != null)
                {
                    results = db.GetMetadata(viewName);
                    crudTableName = tableType.Name;
                }
                else
                    results = new Metadata[0];
            }

            return new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName,
                queryBuilderOptions = jQueryBuilderModel.GenerateFilterOptions(tableType, results)
            };
        }

    }
}

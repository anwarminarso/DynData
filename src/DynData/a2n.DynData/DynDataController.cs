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
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;

#nullable disable

namespace a2n.DynData
{
    [ApiController]
    [Authorize]
    //[EnableCors("DynData")]
    [Route("/dyndata/[controller]")]
    public class DynDataController<TDbContext> : ControllerBase
        where TDbContext : DynDbContext, new()
    {
        private readonly ILogger<DynDataController<TDbContext>> logger;
        private readonly TDbContext db;
        private readonly IServiceProvider provider;
        public DynDataController(ILogger<DynDataController<TDbContext>> logger, TDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }
        [Route("viewNames")]
        [HttpPost]
        [HttpGet]
        public virtual Task<string[]> GetAllViewNames()
        {
            return Task.Run(() =>
            {
                return db.GetAllTableViewNames().OrderBy(t => t).ToArray();
            });
        }


        [Route("{viewName}/datatable")]
        [HttpPost]
        public virtual Task<DataTableJSResponse> GetDataTable(string viewName, [FromForm] DataTableJSRequest req)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
                Type valueType = valueType = db.GetTableType(viewName);
                Metadata[] metaArr = null;


                if (valueType != null)
                {
                    metaArr = db.GetMetadata(viewName);
                    qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
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



        [Route("{viewName}/datatable/export")]
        [HttpPost]
        public virtual IActionResult ExportDataTable(string viewName, [FromForm] DataTableJSExportRequest req)
        {
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            valueType = db.GetTableType(viewName);
            if (valueType != null)
            {
                qry = db.GetQueryable(valueType) as IQueryable<dynamic>;
                propArr = db.GetProperties(viewName);
                metadataArr = db.GetMetadata(viewName);
            }
            if (qry != null)
            {
                qry = req.ToQueryable(qry, valueType, metadataArr);
                byte[] buffer = null;
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();
        }

        [Route("{viewName}/list")]
        [HttpPost]
        public virtual Task<PagingResult<dynamic>> GetList(string viewName, PagingRequest req)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
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

        [Route("{viewName}/export")]
        [HttpPost]
        public virtual IActionResult Export(string viewName, ExportRequest req)
        {
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            valueType = db.GetTableType(viewName);
            if (valueType != null)
            {
                qry = db.Query(viewName, req.rules) as IQueryable<dynamic>;
                propArr = db.GetProperties(viewName);
                metadataArr = db.GetMetadata(viewName);
            }
            if (qry != null)
            {
                if (!string.IsNullOrEmpty(req.orderBy))
                {
                    var asc = req.ascending.HasValue ? req.ascending.Value : true;
                    qry = qry.OrderBy(req.orderBy, asc);
                }
                byte[] buffer = null;
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();
        }

        [Route("{viewName}/dropdown")]
        [HttpGet]
        public virtual Task<PagingResult<dynamic>> GetDropDown(string viewName, string keyField, string labelField, string search, int pageIndex, int pageSize)
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
                valueType = db.GetTableType(viewName);
                qry = db.Query(viewName, rule);
                if (qry != null)
                {
                    //return qry.Select(valueType, keyField, labelField).ToPagingResult(pageSize, pageIndex);
                    return qry.ToPagingResult(pageSize, pageIndex);
                }
                else
                    return new PagingResult<dynamic>();
            });
        }


        [Route("{viewName}/read")]
        [HttpPost]
        public virtual object ReadRecord(string viewName, JToken data)
        {
            JObject jObj = JObject.Parse(data.ToString());
            if (jObj.Properties().Count() == 0)
                return null;
            return db.FindByKey(viewName, jObj);
        }

        [Route("{viewName}/create")]
        [HttpPost]
        public virtual object[] CreateRecord(string viewName, JToken value)
        {
            var dataArr = db.Create(viewName, value);
            db.SaveChanges();
            return dataArr;
        }

        [Route("{viewName}/update")]
        [HttpPost]
        public virtual object[] UpdateRecord(string viewName, JToken value)
        {
            var results = db.Update(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("{viewName}/delete")]
        [HttpPost]
        public virtual object[] DeleteRecord(string viewName, JToken value)
        {
            var results = db.Delete(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("{viewName}/metadata")]
        [HttpPost]
        [HttpGet]
        public virtual MetadataInfo GetMetadata(string viewName)
        {
            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            var tableType = db.GetTableType(viewName);
            if (tableType != null)
            {
                results = db.GetMetadata(viewName);
                crudTableName = tableType.Name;
            }
            else
                results = new Metadata[0];

            return new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName
            };
        }

        [Route("{viewName}/metadataQB")]
        [HttpPost]
        [HttpGet]
        public virtual MetadataInfo GetMetadataQB(string viewName)
        {
            Type tableType = null;
            Metadata[] results = new Metadata[0];
            string crudTableName = null;

            tableType = db.GetTableType(viewName);
            if (tableType != null)
            {
                results = db.GetMetadata(viewName);
                crudTableName = tableType.Name;
            }
            else
                results = new Metadata[0];
            //if (results != null && results.Length > 0)
            //{
            //    List<Metadata> resultLst = new List<Metadata>();
            //    foreach (var item in results)
            //    {
            //        if (item.CustomAttributes != null)
            //        {
            //            Type t = item.CustomAttributes.GetType();
            //            var hiddenProp = t.GetProperties().Where(t => t.Name == "Hidden").SingleOrDefault();
            //            if (hiddenProp != null)
            //            {
            //                var obj = hiddenProp.GetValue(item);
            //                if (obj != null && Boolean.TryParse(obj.ToString(), out bool hidden)) continue;
            //                continue;
            //            }

            //        }
            //        resultLst.Add(item);
            //    }
            //    results = resultLst.ToArray();
            //}

            return new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName,
                queryBuilderOptions = jQueryBuilderModel.GenerateFilterOptions(tableType, results)
            };
        }

    }



    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDbContext">DynDbContext</typeparam>
    /// <typeparam name="TTemplate">BaseQueryTemplateSettings</typeparam>
    [ApiController]
    [Authorize]
    //[EnableCors("DynData")]
    [Route("/dyndata/[controller]")]
    public class DynDataController<TDbContext, TTemplate> : ControllerBase
        where TDbContext : DynDbContext, new()
        where TTemplate : QueryTemplate<TDbContext>
    {
        private readonly ILogger<DynDataController<TDbContext>> logger;
        private readonly TDbContext db;
        private readonly TTemplate qryTpl;

        public DynDataController(ILogger<DynDataController<TDbContext>> logger, TDbContext db, TTemplate qryTpl)
        {
            this.logger = logger;
            this.db = db;
            this.qryTpl = qryTpl;

        }
        [Route("viewNames")]
        [HttpPost]
        [HttpGet]
        public virtual Task<string[]> GetAllViewNames()
        {
            return Task.Run(() =>
            {
                var tableNames = db.GetAllTableViewNames();
                var tplNames = qryTpl.GetQueryTemplateNames();
                return tableNames.Union(tplNames).OrderBy(t => t).ToArray();
            });
        }

        [Route("{viewName}/datatable")]
        [HttpPost]
        public virtual Task<DataTableJSResponse> GetDataTable(string viewName, [FromForm] DataTableJSRequest req)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
                Type valueType = null;
                Metadata[] metaArr = null;
                if (qryTpl.HasQueryName(viewName))
                {
                    qry = qryTpl.GetQuery(db, viewName);
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



        [Route("{viewName}/datatable/export")]
        [HttpPost]
        public virtual IActionResult ExportDataTable(string viewName, [FromForm] DataTableJSExportRequest req)
        {
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            if (qryTpl.HasQueryName(viewName))
            {
                qry = qryTpl.GetQuery(db, viewName);
                valueType = qryTpl.GetValueType(db, viewName);
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
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();

        }

        [Route("{viewName}/list")]
        [HttpPost]
        public virtual Task<PagingResult<dynamic>> GetList(string viewName, PagingRequest req)
        {
            return Task.Run(() =>
            {
                IQueryable<dynamic> qry = null;
                if (qryTpl.HasQueryName(viewName))
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


        [Route("{viewName}/export")]
        [HttpPost]
        public virtual IActionResult Export(string viewName, ExportRequest req)
        {
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            if (qryTpl.HasQueryName(viewName))
            {
                qry = qryTpl.GetQuery(db, viewName, req.rules);
                valueType = qryTpl.GetValueType(db, viewName);
                metadataArr = qryTpl.GetMetadata(db, viewName);
            }
            else
            {
                valueType = db.GetTableType(viewName);
                if (valueType != null)
                {
                    qry = db.Query(viewName, req.rules) as IQueryable<dynamic>;
                    propArr = db.GetProperties(viewName);
                    metadataArr = db.GetMetadata(viewName);
                }
            }
            if (qry != null)
            {
                if (!string.IsNullOrEmpty(req.orderBy))
                {
                    var asc = req.ascending.HasValue ? req.ascending.Value : true;
                    qry = qry.OrderBy(req.orderBy, asc);
                }
                byte[] buffer = null;
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();
        }
        [Route("{viewName}/dropdown")]
        [HttpGet]
        public virtual Task<PagingResult<dynamic>> GetDropDown(string viewName, string keyField, string labelField, string search, int pageIndex, int pageSize)
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
                if (qryTpl.HasQueryName(viewName))
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


        [Route("{viewName}/read")]
        [HttpPost]
        public virtual object ReadRecord(string viewName, JToken data)
        {
            JObject jObj = JObject.Parse(data.ToString());
            if (jObj.Properties().Count() == 0)
                return null;
            if (qryTpl.HasQueryName(viewName))
            {
                return qryTpl.FindByKey(db, viewName, jObj);
            }
            else
            {
                return db.FindByKey(viewName, jObj);
            }
        }

        [Route("{viewName}/create")]
        [HttpPost]
        public virtual object[] CreateRecord(string viewName, JToken value)
        {
            var dataArr = db.Create(viewName, value);
            db.SaveChanges();
            return dataArr;
        }

        [Route("{viewName}/update")]
        [HttpPost]
        public virtual object[] UpdateRecord(string viewName, JToken value)
        {
            var results = db.Update(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("{viewName}/delete")]
        [HttpPost]
        public virtual object[] DeleteRecord(string viewName, JToken value)
        {
            var results = db.Delete(viewName, value);
            db.SaveChanges();
            return results;
        }

        [Route("{viewName}/metadata")]
        [HttpPost]
        [HttpGet]
        public virtual MetadataInfo GetMetadata(string viewName)
        {
            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            if (qryTpl.HasQueryName(viewName))
            {
                results = qryTpl.GetMetadata(db, viewName);
                var crudTableType = qryTpl.GetCRUDTableType(viewName);
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

        [Route("{viewName}/metadataQB")]
        [HttpPost]
        [HttpGet]
        public virtual MetadataInfo GetMetadataQB(string viewName)
        {
            Type tableType = null;
            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            if (qryTpl.HasQueryName(viewName))
            {
                results = qryTpl.GetMetadata(db, viewName);
                tableType = qryTpl.GetValueType(db, viewName);
                var crudTableType = qryTpl.GetCRUDTableType(viewName);
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


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDbContext">DynDbContext</typeparam>
    /// <typeparam name="TTemplate">BaseQueryTemplateSettings</typeparam>
    [ApiController]
    [Authorize]
    //[EnableCors("DynData")]
    [Route("/dyndata/[controller]")]
    public class DynDataController<TDbContext, TTemplate, TApiAuth> : ControllerBase
        where TDbContext : DynDbContext, new()
        where TTemplate : QueryTemplate<TDbContext>
        where TApiAuth : IDynDataAPIAuth
    {
        private readonly ILogger<DynDataController<TDbContext>> logger;
        private readonly TDbContext db;
        private readonly TTemplate qryTpl;
        private readonly IServiceProvider provider;
        private readonly IDynDataAPIAuth auth;

        public DynDataController(IServiceProvider provider, ILogger<DynDataController<TDbContext>> logger, TDbContext db, TTemplate qryTpl, TApiAuth auth)
        {
            this.logger = logger;
            this.db = db;
            this.qryTpl = qryTpl;
            this.provider = provider;
            this.auth = auth;
        }

        private bool IsAllowed(DynDataAPIMethod methodName, string viewName)
        {
            var controllerName = this.ControllerContext.ActionDescriptor.ControllerName;
            if (auth != null && !auth.IsAllowed(this.HttpContext, controllerName, db, methodName, viewName))
            {
                //if (string.IsNullOrEmpty(viewName))
                //    throw new UnauthorizedAccessException(string.Format("User: {0}, not authorize to access method {1}",
                //        this.HttpContext.User.Identity.Name, methodName.ToString()));
                //else
                //    throw new UnauthorizedAccessException(string.Format("User: {0}, not authorize to access method {1}, viewName: {2}",
                //        this.HttpContext.User.Identity.Name, methodName.ToString(), viewName));
                return false;
            }
            return true;
        }
        [Route("viewNames")]
        [HttpPost]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetAllViewNames()
        {
            if (!IsAllowed(DynDataAPIMethod.ViewNames, null))
                return new UnauthorizedResult();

            var tableNames = db.GetAllTableViewNames();
            var tplNames = qryTpl.GetQueryTemplateNames();
            return Ok(tableNames.Union(tplNames).OrderBy(t => t).ToArray());
        }

        [Route("{viewName}/datatable")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DataTableJSResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetDataTable(string viewName, [FromForm] DataTableJSRequest req)
        {
            if (!IsAllowed(DynDataAPIMethod.DataTable, viewName))
                return new UnauthorizedResult();
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metaArr = null;
            if (qryTpl.HasQueryName(viewName))
            {
                qry = qryTpl.GetQuery(db, viewName);
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
                return Ok(resp);
            }
            else
                return Ok(new DataTableJSResponse(req, new PagingResult<dynamic>()));
        }



        [Route("{viewName}/datatable/export")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult ExportDataTable(string viewName, [FromForm] DataTableJSExportRequest req)
        {
            if (!IsAllowed(DynDataAPIMethod.DataTableExport, viewName))
                return new UnauthorizedResult();
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            if (qryTpl.HasQueryName(viewName))
            {
                qry = qryTpl.GetQuery(db, viewName);
                valueType = qryTpl.GetValueType(db, viewName);
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
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();

        }

        [Route("{viewName}/list")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagingResult<dynamic>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetList(string viewName, PagingRequest req)
        {
            if (!IsAllowed(DynDataAPIMethod.List, viewName))
                return new UnauthorizedResult();
            IQueryable<dynamic> qry = null;
            if (qryTpl.HasQueryName(viewName))
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
                return Ok(qry.ToPagingResult(req.pageSize, req.pageIndex));
            }
            else
                return Ok(new PagingResult<dynamic>());
        }


        [Route("{viewName}/export")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult Export(string viewName, ExportRequest req)
        {
            if (!IsAllowed(DynDataAPIMethod.Export, viewName))
                return new UnauthorizedResult();
            string format = string.Empty;
            if (string.IsNullOrEmpty(req.format))
                format = "csv";
            else
                format = req.format;
            IQueryable<dynamic> qry = null;
            Type valueType = null;
            Metadata[] metadataArr = null;
            PropertyInfo[] propArr = null;

            if (qryTpl.HasQueryName(viewName))
            {
                qry = qryTpl.GetQuery(db, viewName, req.rules);
                valueType = qryTpl.GetValueType(db, viewName);
                metadataArr = qryTpl.GetMetadata(db, viewName);
            }
            else
            {
                valueType = db.GetTableType(viewName);
                if (valueType != null)
                {
                    qry = db.Query(viewName, req.rules) as IQueryable<dynamic>;
                    propArr = db.GetProperties(viewName);
                    metadataArr = db.GetMetadata(viewName);
                }
            }
            if (qry != null)
            {
                if (!string.IsNullOrEmpty(req.orderBy))
                {
                    var asc = req.ascending.HasValue ? req.ascending.Value : true;
                    qry = qry.OrderBy(req.orderBy, asc);
                }
                byte[] buffer = null;
                string fileName = string.Empty;
                string mimeType = string.Empty;
                format = format.ToLower();
                if (db.Handler != null)
                    db.Handler.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                else
                    DefaultExport.OnExport(format, viewName, valueType, metadataArr, qry, out buffer, out mimeType, out fileName);
                if (buffer != null)
                    return File(buffer, mimeType, fileName);
                else
                    return NotFound();
            }
            return NotFound();
        }


        [Route("{viewName}/dropdown")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagingResult<dynamic>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetDropDown(string viewName, string keyField, string labelField, string search, int pageIndex, int pageSize)
        {
            if (!IsAllowed(DynDataAPIMethod.DropDown, viewName))
                return new UnauthorizedResult();

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
            if (qryTpl.HasQueryName(viewName))
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
                return Ok(qry.ToPagingResult(pageSize, pageIndex));
            }
            else
                return Ok(new PagingResult<dynamic>());
        }


        [Route("{viewName}/read")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult ReadRecord(string viewName, JToken data)
        {
            if (!IsAllowed(DynDataAPIMethod.Read, viewName))
                return new UnauthorizedResult();

            JObject jObj = JObject.Parse(data.ToString());
            if (jObj.Properties().Count() == 0)
                return null;
            if (qryTpl.HasQueryName(viewName))
            {
                var obj = qryTpl.FindByKey(db, viewName, jObj);
                return Ok(obj);
            }
            else
            {
                var obj = db.FindByKey(viewName, jObj);
                return Ok(obj);
            }
        }

        [Route("{viewName}/create")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult CreateRecord(string viewName, JToken value)
        {
            if (!IsAllowed(DynDataAPIMethod.Create, viewName))
                return new UnauthorizedResult();

            var dataArr = db.Create(viewName, value);
            db.SaveChanges();
            return Ok(dataArr);
        }

        [Route("{viewName}/update")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult UpdateRecord(string viewName, JToken value)
        {
            if (!IsAllowed(DynDataAPIMethod.Update, viewName))
                return new UnauthorizedResult();

            var results = db.Update(viewName, value);
            db.SaveChanges();
            return Ok(results);
        }

        [Route("{viewName}/delete")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult DeleteRecord(string viewName, JToken value)
        {
            if (!IsAllowed(DynDataAPIMethod.Delete, viewName))
                return new UnauthorizedResult();

            var results = db.Delete(viewName, value);
            db.SaveChanges();
            return Ok(results);
        }

        [Route("{viewName}/metadata")]
        [HttpPost]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetadataInfo))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetMetadata(string viewName)
        {
            if (!IsAllowed(DynDataAPIMethod.Metadata, viewName))
                return new UnauthorizedResult();

            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            if (qryTpl.HasQueryName(viewName))
            {
                results = qryTpl.GetMetadata(db, viewName);
                var crudTableType = qryTpl.GetCRUDTableType(viewName);
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

            return Ok(new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName
            });
        }

        [Route("{viewName}/metadataQB")]
        [HttpPost]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MetadataInfo))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public virtual IActionResult GetMetadataQB(string viewName)
        {
            if (!IsAllowed(DynDataAPIMethod.MetadataQB, viewName))
                return new UnauthorizedResult();

            Type tableType = null;
            Metadata[] results = new Metadata[0];
            string crudTableName = null;
            if (qryTpl.HasQueryName(viewName))
            {
                results = qryTpl.GetMetadata(db, viewName);
                tableType = qryTpl.GetValueType(db, viewName);
                var crudTableType = qryTpl.GetCRUDTableType(viewName);
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
            return Ok(new MetadataInfo()
            {
                viewName = viewName,
                metaData = results,
                crudTableName = crudTableName,
                queryBuilderOptions = jQueryBuilderModel.GenerateFilterOptions(tableType, results)
            });
        }
    }
}

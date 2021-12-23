/// <reference path="../jquery/dist/jquery.js" />
/// <reference path="../bootstrap/dist/js/bootstrap.bundle.js" />
/// <reference path="a2n.js" />



// a2n.dyndata
// version 1.0.0
// © Anwar Minarso, 2021
// https://github.com/anwarminarso
//
// GitHub page:     https://github.com/anwarminarso/DynData
//
// Released under MIT licence:
// =====================================================================================================================

a2n.dyndata = a2n.dyndata || {};
a2n.dyndata.Configuration = {
    API_METADATA: "/dyndata/${controller}/${viewName}/metadata",
    API_METADATAQB: "/dyndata/${controller}/${viewName}/metadataQB",
    API_DATATABLE: "/dyndata/${controller}/${viewName}/datatable",
    API_DATATABLE_EXPORT: "/dyndata/${controller}/${viewName}/datatable/export",
    API_LIST: "/dyndata/${controller}/${viewName}/list",
    API_DROPDOWN: "/dyndata/${controller}/${viewName}/dropdown",
    API_CREATE: "/dyndata/${controller}/${viewName}/create",
    API_READ: "/dyndata/${controller}/${viewName}/read",
    API_UPDATE: "/dyndata/${controller}/${viewName}/update",
    API_DELETE: "/dyndata/${controller}/${viewName}/delete",
    getApiMetadata: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_METADATA + "`");
    },
    getApiMetadataQB: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_METADATAQB + "`");
    },
    getApiDataTable: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DATATABLE + "`");
    },
    getApiDataTableExport: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DATATABLE_EXPORT + "`");
    },
    getApiList: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_LIST + "`");
    },
    getApiDropDown: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DROPDOWN + "`");
    },
    getApiCreate: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_CREATE + "`");
    },
    getApiRead: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_READ + "`");
    },
    getApiUpdate: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_UPDATE + "`");
    },
    getApiDelete: function (controller, viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DELETE + "`");
    }
};

a2n.dyndata.Utils = {
    dtInstances: {},
    QBInstance: null,
    FormInstance: null
}
a2n.dyndata.DataTable = function (tableId, parentElement, controller, viewName, options) {
    this.ID = tableId;
    this.$el = $(parentElement);
    this.controller = controller;
    this.viewName = viewName;
    this.dt = null;
    this.qbRuleSet = null;
    this.externalFilter = null;
    this.dynOptions = {
        allowCreate: false,
        allowUpdate: false,
        allowDelete: false,
        allowView: false,
        allowExport: false,
        rowCommandButtons: [],
        onRowCommand: function (commandName, rowIndex, rowData) { },
        enableQueryBuilder: true,
        metaData: [],
        queryBuilderOptions: null,
        crudTableName: null,
        hasPK: false,
        minGlobalSearchCharLength: 3,
        useBuiltInSearchMode: false
    };
    if (options && options.dynOptions) {
        this.dynOptions = $.extend(this.dynOptions, options.dynOptions);
    }

    if (this.dynOptions.allowView)
        this.dynOptions.rowCommandButtons.push({ commandName: 'View', title: 'View', btnCls:'mr-2', iconCls: 'fa fa-search text-info' });
    if (this.dynOptions.allowUpdate)
        this.dynOptions.rowCommandButtons.push({ commandName: 'Edit', title: 'Edit', btnCls: 'mr-2', iconCls: 'fa fa-edit text-warning' });
    if (this.dynOptions.allowDelete)
        this.dynOptions.rowCommandButtons.push({ commandName: 'Delete', title: 'Delete', btnCls: 'mr-2', iconCls: 'fa fa-trash-alt text-danger' });

    let ajaxUrl = a2n.dyndata.Configuration.getApiDataTable(this.controller,this.viewName);
    a2n.dyndata.Utils.dtInstances[tableId] = this;

    let buttons = [];
    if (this.dynOptions.enableQueryBuilder) {
        if (!a2n.dyndata.Utils.QBInstance) {
            a2n.dyndata.Utils.QBInstance = new a2n.dyndata.QueryBuilder();
        }
        buttons.push({
            text: '<i class="fas fa-filter mr-1"></i>Filter',
            className: 'btn btn-warning btn-sm text-white',
            action: function (e, dt, node, config) {
                let obj = a2n.dyndata.Utils.dtInstances[viewName];
                a2n.dyndata.Utils.QBInstance.Show(viewName, obj.qbRuleSet, obj.dynOptions.queryBuilderOptions, function (ruleSet) {
                    obj.qbRuleSet = ruleSet;
                    obj.dt.ajax.reload();
                });
            }
        });
    }
    if (this.dynOptions.allowExport) {
        buttons.push({
            extend: 'collection',
            text: '<i class="fas fa-file-export mr-1"></i>Export',
            className: 'btn btn-primary btn-sm text-white',
            autoClose: true,
            buttons: [
                {
                    text: '<i class="fa fa-file-csv mr-2" style="color: blue"></i> Export to CSV',
                    action: function (e, dt, node, config) {
                        let obj = a2n.dyndata.Utils.dtInstances[viewName];
                        obj.Export('csv');
                    }
                },
                {
                    text: '<i class="fa fa-file-excel mr-2" style="color: green"></i> Export to Excel',
                    action: function (e, dt, node, config) {
                        let obj = a2n.dyndata.Utils.dtInstances[viewName];
                        obj.Export('xlsx');
                    }
                },
                {
                    text: '<i class="fa fa-file-pdf mr-2" style="color: red"></i> Export to PDF',
                    action: function (e, dt, node, config) {
                        //let obj = a2n.dyndata.Utils.dtInstances[viewName];
                        //obj.Export('pdf');
                        alert('Not implemented.. coming soon');
                    }
                }]
        })
    }
    buttons.push({
        text: '<i class="fa fa-sync mr-1"></i>Reload',
        className: 'btn btn-info btn-sm',
        action: function (e, dt, node, config) {
            dt.ajax.reload();
        }
    });

    this.tableOptions = {
        order: [],
        responsive: false,
        processing: false,
        serverSide: true,
        select: true,
        ajax: {
            "url": ajaxUrl,
            "type": "POST",
            "data": function (r) {
                let obj = a2n.dyndata.Utils.dtInstances[tableId];
                r.viewName = viewName;
                r.id = tableId;
                if (obj.externalFilter)
                    r.externalFilter = obj.externalFilter;
                if (obj.qbRuleSet) {
                    r.jsonQB = JSON.stringify({
                        referenceType: obj.viewName,
                        ruleData: obj.qbRuleSet
                    });
                }
            }
        }
    };

    if (options && options.tableOptions)
        this.tableOptions = $.extend(this.tableOptions, options.tableOptions);
    if (!this.tableOptions.dom) {
        if (!this.tableOptions.responsive) {
            this.tableOptions.dom = "<'row mb-2'<'col-sm-12 col-md-6 d-flex align-items-center justify-content-start'f><'col-sm-12 col-md-6 d-flex align-items-center justify-content-end'B>>" +
                //"<'row'<'col-sm-12'tr>>" +
                "<'div'tr>" +
                "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>"
        }
        else {
            this.tableOptions.dom = "<'row mb-2'<'col-sm-12 col-md-6 d-flex align-items-center justify-content-start'f><'col-sm-12 col-md-6 d-flex align-items-center justify-content-end'B>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-12 col-md-3 mt-2'i><'col-sm-12 col-md-2 mt-1'l><'col-sm-12 col-md-7'p>>";
        }
    }
    if (!this.tableOptions.buttons)
        this.tableOptions.buttons = buttons;
    else {
        for (let i = 0; i < buttons.length; i++) {
            this.tableOptions.buttons.push(buttons[i]);
        }
    }
}
a2n.dyndata.DataTable.prototype = {
    _IsRendered: false,
    LoadMetadata: function (render) {
        let _this = this;
        let _render = render;
        let _apiMetaUrl = a2n.dyndata.Configuration.getApiMetadataQB(_this.controller, _this.viewName);
        $.getJSON(_apiMetaUrl, function (result) {
            _this.dynOptions.metaData = result.metaData;
            _this.dynOptions.queryBuilderOptions = result.queryBuilderOptions;
            _this.dynOptions.crudTableName = result.crudTableName;
            _this.dynOptions.hasPK = false;
            for (let i = 0; i < result.metaData.length; i++) {
                let meta = result.metaData[i];
                if (meta.IsPrimaryKey) {
                    _this.dynOptions.hasPK = true;
                    break;
                }
            }
            if (!_this.dynOptions.hasPK) {
                _this.dynOptions.allowCreate = false;
                _this.dynOptions.allowView = false;
                _this.dynOptions.allowUpdate = false;
                _this.dynOptions.allowDelete = false;
            }
            if (_render)
                _this.Render();
        });
    },
    Render: function () {
        let _this = this;
        if (_this._IsRendered)
            return;
        if (!_this.dynOptions.metaData || _this.dynOptions.metaData.length == 0) {
            _this.LoadMetadata(true);
            return;
        }
        let tabletpl = `
<table id="${_this.ID}" class="table table-bordered table-hover table-striped w-100">
    <thead>
        <tr>
        </tr>
    </thead>
</table>`;
        let $tbl = $(tabletpl);
        let $row = $tbl.find('tr');
        let columns = [];
        let columnDefs = [];
        for (let i = 0; i < _this.dynOptions.metaData.length; i++) {
            let data = _this.dynOptions.metaData[i];
            if (data.CustomAttributes) {
                if (data.CustomAttributes.Hidden)
                    $row.append(`<th data-visible='false' data-name="${data.FieldName}" ${!data.IsOrderable ? 'data-sortable="false"' : ""}  ${!data.IsSearchable ? 'data-searchable="false"' : ""}>${data.FieldLabel}</th>`);
                else
                    $row.append(`<th data-name="${data.FieldName}" ${!data.IsOrderable ? 'data-sortable="false"' : ""}  ${!data.IsSearchable ? 'data-searchable="false"' : ""}>${data.FieldLabel}</th>`);
            }
            else
                $row.append(`<th data-name="${data.FieldName}" ${!data.IsOrderable ? 'data-sortable="false"' : ""}  ${!data.IsSearchable ? 'data-searchable="false"' : ""}>${data.FieldLabel}</th>`);
            columns.push({ data: data.FieldName, name: data.FieldName, title: data.FieldLabel });
        }
        if (_this.dynOptions.hasPK && _this.dynOptions.rowCommandButtons && _this.dynOptions.rowCommandButtons.length > 0) {
            $row.append('<th data-searchable="false" data-sortable="false">Action</th>');
            let actionRenderer = "";
            for (let i = 0; i < _this.dynOptions.rowCommandButtons.length; i++) {
                let btn = _this.dynOptions.rowCommandButtons[i];
                if (btn.renderTemplate) {
                    actionRenderer += btn.renderTemplate;
                    continue;
                }
                let btnCls = 'btn btn-primary rounded-circle';
                if (btn.btnCls)
                    btnCls = btn.btnCls;
                let btnEl = `<a class="${btnCls}" href="javascript:;" onclick="a2n.dyndata.Utils.dtInstances.${_this.ID}.RowCommand('${btn.commandName}', METAROWCODE)" title="${btn.title}" ${btn.visibleHandler !== undefined ? "VISIBLEHANDLER" : ''}><i class="${btn.iconCls}"></i></a>`;

                btnEl = btnEl.replace(new RegExp("METAROWCODE", 'g'), "${meta.row}");
                if (btn.visibleHandler) {
                    btnEl = btnEl.replace(new RegExp("VISIBLEHANDLER", 'g'), "${" + `${btn.visibleHandler}` + " ? '' : 'style=\"display: none;\"'}");
                }
                actionRenderer += btnEl;
            }
            columnDefs.push({
                targets: columns.length,
                data: null,
                render: function (data, type, row, meta) {
                        return eval("`" + actionRenderer + "`");
                    }
                });
        }

        if (_this.dynOptions.allowCreate && _this.dynOptions.crudTableName) {
            _this.tableOptions.buttons.unshift({
                text: '<i class="fas fa-plus mr-1"></i>New',
                className: 'btn btn-success btn-sm',
                action: function (e, dt, node, config) {
                    if (!a2n.dyndata.Utils.FormInstance) {
                        a2n.dyndata.Utils.FormInstance = new a2n.dyndata.Form();
                    }
                    let metaData = null;
                    if (_this.dynOptions.crudTableName == _this.viewName)
                        metaData = _this.dynOptions.metaData
                    a2n.dyndata.Utils.FormInstance.Show(_this.controller, _this.dynOptions.crudTableName, "New", metaData, null, function (data) {
                        let _apiCreateUrl = a2n.dyndata.Configuration.getApiCreate(_this.controller, _this.dynOptions.crudTableName);
                        a2n.submitAjaxJsonPost(_apiCreateUrl, JSON.stringify(data), function () {
                            _this.dt.ajax.reload();
                        });
                    });
                }
            });
        }

        _this.$el.html("");
        _this.$el.append($tbl);
        let _tableOptions = $.extend({}, _this.tableOptions);
        if (!_tableOptions.columns)
            _tableOptions.columns = columns;
        if (!_tableOptions.columnDefs)
            _tableOptions.columnDefs = columnDefs;
        let dt = $tbl.DataTable(_tableOptions);
        _this.dt = dt;
        if (!_this.dynOptions.useBuiltInSearchMode) {
            let $inputSearch = _this.$el.find('input[type=search]');
            $inputSearch.unbind();
            $inputSearch.on('keypress keyup',
                function (e) {
                    var key;
                    if (e && e.which && !e.keyCode) {
                        key = e.which;
                    } else {
                        if (!e && window.event)
                            e = window.event;
                        if (e && e.keyCode) {
                            key = e.keyCode;
                        }
                    }
                    if (key === 13) {
                        if (!this.value)
                            this.value = "";
                        var val = this.value;
                        if (val.length > 0 && val.length < _this.dynOptions.minGlobalSearchCharLength)
                            return;
                        dt.search(val).draw();
                    }
                }).on('blur', function (e) {
                    if (!this.value)
                        this.value = "";
                    var val = this.value;
                    if (val.length > 0 && val.length < _this.dynOptions.minGlobalSearchCharLength)
                        return;
                    dt.search(val).draw();
                });
        }
        _this._IsRendered = true;
    },
    RowCommand: function (commandName, rowIndex) {
        let _this = this;
        let rowData = _this.dt.row(rowIndex).data();
        if (!a2n.dyndata.Utils.FormInstance) {
            a2n.dyndata.Utils.FormInstance = new a2n.dyndata.Form();
        }
        let metaData = null;
        if (_this.dynOptions.crudTableName == _this.viewName)
            metaData = _this.dynOptions.metaData
        switch (commandName) {
            case "Edit":
                {
                    a2n.dyndata.Utils.FormInstance.Show(_this.controller,_this.dynOptions.crudTableName, "Edit", metaData, rowData, function (data) {
                        let _apiUpdateUrl = a2n.dyndata.Configuration.getApiUpdate(_this.controller, _this.dynOptions.crudTableName);
                        a2n.submitAjaxJsonPost(_apiUpdateUrl, JSON.stringify(data), function () {
                            _this.dt.ajax.reload();
                        });
                    });
                }
                break;
            case "Delete":
                {
                    a2n.showConfirmDelete(
                        function (result) {
                            if (result) {
                                let _apiDeleteUrl = a2n.dyndata.Configuration.getApiDelete(_this.controller, _this.dynOptions.crudTableName);
                                a2n.submitAjaxJsonPost(_apiDeleteUrl, JSON.stringify(rowData), function () {
                                    _this.dt.ajax.reload();
                                });
                            }
                        }
                    );
                }
                break;
            case "View":
                {
                    a2n.dyndata.Utils.FormInstance.Show(_this.controller, _this.viewName, "View", _this.dynOptions.metaData, rowData);
                }
                break;
            default:
                if (this.dynOptions.onRowCommand) {
                    this.dynOptions.onRowCommand(commandName, rowIndex, rowData);
                }
                break;
        }
    },
    LoadData: function () {
        if (!this._IsRendered)
            this.Render();
        this.dt.ajax.reload();
    },
    Export: function (format) {
        let req = { viewName: this.viewName, externalfilter: this.externalFilter };
        let apiExportUrl = a2n.dyndata.Configuration.getApiDataTableExport(this.controller, this.viewName);
        req.globalSearch = this.dt.search();
        req.format = format;
        let order = this.dt.order();
        if (order && order.length > 0) {
            req.orderBy = this.dynOptions.metaData[order[0][0]].FieldName;
            req.dir = order[0][1];
        }
        if (this.qbRuleSet) {
            req.jsonQB = JSON.stringify({
                referenceType: this.viewName,
                ruleData: this.qbRuleSet
            });
        }
        a2n.submitPost(apiExportUrl, req);
    },
    Destroy: function () {

        if (a2n.dyndata.Utils.dtInstances[this.ID].dt) {
            a2n.dyndata.Utils.dtInstances[this.ID].dt.destroy();
        }
        this.$el.html('');
        delete a2n.dyndata.Utils.dtInstances[this.ID];
    }
}


a2n.dyndata.QueryBuilder = function (options) {
    this.ID = new Date().getTime().toString();
    this.$el = null;
    this.qb = null;
    this.key = null;
    let defaultOptions = {
        OnApply: function (ruleSet) {

        }
    }
    this.options = $.extend({}, defaultOptions, options)
}
a2n.dyndata.QueryBuilder.prototype = {
    _IsRendered: false,
    Render: function () {
        if (this._IsRendered)
            return;
        let tpl = `<div class="modal fade" id="mdl${this.ID}" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Advanced Filter</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true"><i class="fa fa-times"></i></span>
                </button>
            </div>
            <div class="modal-body">
                <form id="frm${this.ID}">
                    <div class="row">
                        <div class="col-12">
                            <div id="container${this.ID}"></div>
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" id="btn${this.ID}Close" class="btn btn-secondary" data-dismiss="modal">Close</button>
                <button type="button" id="btn${this.ID}Apply" class="btn btn-success">Apply</button>
            </div>
        </div>
    </div>
</div>`
        let $tpl = $(tpl);
        $('body').append($tpl);

        $(`#btn${this.ID}Apply`).click(this, function (evt) {
            let _this = evt.data;
            _this.Apply();
        });

        this._IsRendered = true;

    },
    Apply: function () {
        let ruleSet = $(`#container${this.ID}`).queryBuilder('getRules');
        if (this.options.OnApply)
            this.options.OnApply(ruleSet);
        $(`#mdl${this.ID}`).modal('hide');
    },
    Show: function (key, ruleSet, filterOptions, OnApply) {
        this.Render();
        let $qb = $(`#container${this.ID}`);
        if (this.key && this.key != key && this.qb) {
            $qb.queryBuilder("destroy");
            delete this.qb;
        }
        this.key = key;
        if (!this.qb) {
            this.qb = $qb.queryBuilder(filterOptions);
        }
        if (ruleSet)
            $qb.queryBuilder('setRules', ruleSet);

        this.options.OnApply = OnApply;
        $(`#mdl${this.ID}`).modal('show');
    },
    Destroy: function () {
        if (this._IsRendered) {
            if (this.qb) {
                $(`#container${this.ID}`).queryBuilder("destroy");
                delete this.qb;
            }
            if (this.$el)
                $('body').remove($el);
        }
        if (a2n.dyndata.Utils.QBInstance == this) {
            delete a2n.dyndata.Utils.QBInstance;
        }
        delete this;
    }
}


a2n.dyndata.Form = function (options) {
    this.ID = new Date().getTime().toString();;
    this.$el = null;
    this.dynOptions = {
        metaData: [],
        hasPK: false
    };
    if (options && options.dynOptions) {
        this.dynOptions = $.extend(this.dynOptions, options.dynOptions);
    }

}
a2n.dyndata.Form.prototype = {
    controller: null,
    viewName: null,
    _IsRendered: false,
    _IsFormGenerated: false,
    _OnSubmit: null,
    _FormMode: 'View',
    _GenerateForm: function (formMode) {
        let $frm = $(`#frm${this.ID}`);

        //clean up component
        if ($frm.find('select').filter("[data-principal]").length > 0)
            $frm.find('select').filter("[data-principal]").select2('destroy');
        $frm.html('');
        $(`#mdl${this.ID} h5.modal-title`).html(`${this.viewName} ${formMode} Form`);

        
        for (let i = 0; i < this.dynOptions.metaData.length; i++) {
            let meta = this.dynOptions.metaData[i];
            let $tpl = null;
            if (meta.IsForeignKey) {
                let tpl = `<div class="form-group">
    <label class="form-label" for="cb${meta.FieldName}">${meta.PrincipalLabel}</label>
    <select class="form-control" id="cb${meta.FieldName}" data-principal="${meta.PrincipalName}" data-keyfield="${meta.PrincipalFieldName}" data-labelfield="${meta.PrincipalDisplayFieldName}" data-live-search="true" name="${meta.FieldName}" ${!meta.IsNullable ? "required" : ""}></select>
</div>`;
                $tpl = $(tpl);
                if (meta.IsPrimaryKey) {
                    $tpl.find('select').attr('data-pk', 'true');
                }
                let keys = { keyField: meta.PrincipalFieldName, labelField: meta.PrincipalDisplayFieldName }
                $frm.append($tpl);
                $tpl.find('select').select2({
                    theme: "bootstrap4",
                    allowClear: true,
                    placeholder: `--- Select a ${meta.PrincipalLabel} ---`,
                    minimumInputLength: 2,
                    ajax: {
                        url: a2n.dyndata.Configuration.getApiDropDown(this.controller, meta.PrincipalName),
                        type: "GET",
                        contentType: 'application/json',
                        data: function (params) {
                            let query = {
                                search: params.term,
                                keyField: keys.keyField,
                                labelField: keys.labelField,
                                pageIndex: params.nextPageIndex || 0,
                                pageSize: params.pageSize || 20
                            }
                            return query;
                        },
                        processResults: function (data, params) {
                            params.pageIndex = data.pageIndex || 0;
                            params.pageSize = data.pageSize || 20;
                            params.nextPageIndex = params.pageIndex;
                            let more = data.pageSize ? ((data.pageIndex + 1) * data.pageSize) < data.totalRows : false;
                            if (more)
                                params.nextPageIndex++;
                            let results = [];
                            for (var i = 0; i < data.items.length; i++) {
                                let item = data.items[i];
                                results.push({ id: item[keys.keyField], text: item[keys.labelField] });
                            }
                            return {
                                results: results,
                                pagination: {
                                    more: more
                                }
                            };
                        },
                        cache: false
                    }
                });
            }
            else {
                switch (meta.FieldType) {
                    case 'Int16':
                    case 'Int32':
                    case 'Int64':
                        {
                            let tpl = `
<div class="form-group">
    <label class="form-label" for="nm${meta.FieldName}">${meta.FieldLabel}</label>
    <input id="nm${meta.FieldName}" type="number" class="form-control" name="${meta.FieldName}" ${!meta.IsNullable ? "required" : ""} />
</div>`;
                            $tpl = $(tpl);
                            if (meta.IsAutoGenerated) {
                                $tpl.find('input').attr('readonly', 'readonly');
                                $tpl.find('input').attr('data-autogen', 'true');
                            }
                            if (meta.IsPrimaryKey) {
                                $tpl.find('input').attr('data-pk', 'true');
                            }
                        }
                        break;
                    case 'UInt16':
                    case 'UInt32':
                    case 'UInt64':
                        {
                            let tpl = `
<div class="form-group">
    <label class="form-label" for="nm${meta.FieldName}">${meta.FieldLabel}</label>
    <input id="nm${meta.FieldName}" type="number" class="form-control" min="0" name="${meta.FieldName}" ${!meta.IsNullable ? "required" : ""} />
</div>`;
                            $tpl = $(tpl);
                            if (meta.IsAutoGenerated) {
                                $tpl.find('input').attr('readonly', 'readonly');
                                $tpl.find('input').attr('data-autogen', 'true');
                            }
                            if (meta.IsPrimaryKey) {
                                $tpl.find('input').attr('data-pk', 'true');
                            }
                        }
                        break;
                    case "Single":
                    case "Double":
                    case "Decimal":
                        {
                            let tpl = `
<div class="form-group">
    <label class="form-label" for="nm${meta.FieldName}">${meta.FieldLabel}</label>
    <input id="nm${meta.FieldName}" type="number" class="form-control" min="0" name="${meta.FieldName}" step="0.25" ${!meta.IsNullable ? "required" : ""} />
</div>`;
                            $tpl = $(tpl);
                            if (meta.IsPrimaryKey) {
                                $tpl.find('input').attr('data-pk', 'true');
                            }
                        }
                        break;
                    case "DateTime":
                        {
                            let tpl = `
<div class="form-group">
    <label class="form-label" for="dt${meta.FieldName}">${meta.FieldLabel}</label>
    <input id="dt${meta.FieldName}" type="datetime-local" class="form-control" name="${meta.FieldName}"  ${!meta.IsNullable ? "required" : ""} />
</div>`;
                            $tpl = $(tpl);
                            if (meta.IsPrimaryKey) {
                                $tpl.find('input').attr('data-pk', 'true');
                            }
                        }
                        break;
                    case "Boolean":
                        {
                            let tpl = `
<div class="form-group">
    <div class="form-check">
        <input id="cb${meta.FieldName}" type="checkbox" class="form-check-input"  name="${meta.FieldName}">
        <label class="form-check-label" for="cb${meta.FieldName}">
            ${meta.FieldLabel}
        </label>
    </div>
</div>`;
                            $tpl = $(tpl);
                        }
                        break;
                    case "Byte":
                        break;
                    case 'Guid':
                    case 'String':
                        {
                            let tpl = `
<div class="form-group">
    <label class="form-label" for="tb${meta.FieldName}">${meta.FieldLabel}</label>
    <input id="tb${meta.FieldName}" type="text" class="form-control" name="${meta.FieldName}" ${!meta.IsNullable ? "required" : ""} />
</div>`;
                            $tpl = $(tpl);
                            if (meta.IsPrimaryKey) {
                                $tpl.find('input').attr('data-pk', 'true');
                            }
                        }
                    default:
                        break;
                }
                if ($tpl)
                    $frm.append($tpl);
            }
        }
        this._IsFormGenerated = true;
    },
    LoadMetadata: function (controller, viewName, callback) {
        let _this = this;
        _this.controller = controller;
        _this.viewName = viewName;
        let _apiMetaUrl = a2n.dyndata.Configuration.getApiMetadata(_this.controller, _this.viewName);
        $.getJSON(_apiMetaUrl, function (result) {
            _this.dynOptions.metaData = result.metaData;
            _this.dynOptions.hasPK = false;
            for (let i = 0; i < result.length; i++) {
                if (result[i].IsPrimaryKey) {
                    _this.dynOptions.hasPK = true;
                    break;
                }
            }
            _this._IsFormGenerated = false;
            if (callback)
                callback(_this);
        });
    },
    Render: function () {
        if (this._IsRendered)
            return;
        let tpl = `<div class="modal fade" id="mdl${this.ID}" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Form</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true"><i class="fa fa-times"></i></span>
                </button>
            </div>
            <div class="modal-body">
                <form id="frm${this.ID}">
                    <div class="row">
                        <div class="col-12">
                            <div id="container${this.ID}"></div>
                        </div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" id="btn${this.ID}Close" class="btn btn-secondary" data-dismiss="modal">Close</button>
                <button type="button" id="btn${this.ID}Submit" class="btn btn-success">Submit</button>
            </div>
        </div>
    </div>
</div>`
        let $tpl = $(tpl);
        $('body').append($tpl);
        this.$el = $tpl;

        $(`#btn${this.ID}Submit`).click(this, function (evt) {
            let $frm = $(`#frm${evt.data.ID}`);

            if ($frm[0].checkValidity() === false) {
                $frm.addClass('was-validated');
                event.preventDefault();
                event.stopPropagation();
                return;
            }
            evt.data.Submit();
        });
        this._IsRendered = true;
    },
    Submit: function () {
        let $frm = $(`#frm${this.ID}`);
        let data = a2n.createObjectFromFormInputName($frm);
        if (this._FormMode == "New") {
            for (let i = 0; i < this.dynOptions.metaData.length; i++) {
                let meta = this.dynOptions.metaData[i];
                if (meta.IsAutoGenerated) {
                    delete data[meta.FieldName];
                }
            }
        }
        if (this._OnSubmit)
            this._OnSubmit(data, this.dynOptions.viewName);
        $(`#mdl${this.ID}`).modal('hide');
    },
    Show: function (controller, viewName, formMode, metaData, data, OnSubmit) {
        let _this = this;
        _this.Render();
        let reloadMetadata = false;
        let $container = $(`#container${_this.ID}`);
        let $frm = $(`#frm${_this.ID}`);
        if (_this.viewName != viewName || _this.controller != controller) {
            _this._IsFormGenerated = false;
            if (metaData)
                _this.dynOptions.metaData = metaData;
            else
                reloadMetadata = true;
        }
        else {
            if (!_this.dynOptions.metaData && !metaData) {
                _this._IsFormGenerated = false;
                reloadMetadata = true;
            }
        }
        if (reloadMetadata) {
            let _viewName = viewName;
            let _controller = controller;
            let _formMode = formMode;
            let _metaData = metaData;
            let _data = data;
            let _OnSubmit = OnSubmit;

            _this.LoadMetadata(_controller, _viewName, function (cb) {
                _this.Show(_controller, _viewName, _formMode, _metaData, _data, _OnSubmit);
            });
            return;
        }
        _this.viewName = viewName;
        _this.controller = controller;
        _this._OnSubmit = OnSubmit;
        _this._FormMode = formMode;
        if (!_this._IsFormGenerated)
            _this._GenerateForm(formMode);
        switch (formMode) {
            case "Edit":
                $frm.find('input').removeAttr('readonly');
                $frm.find('input[data-pk=true]').attr('readonly', 'readonly');
                $frm.find('input[data-autogen=true]').attr('readonly', 'readonly');
                $frm.find(`select`).removeAttr('disabled');
                $frm.find(`select[data-pk=true]`).attr('disabled', 'disabled');
                _this.$el.find(`#btn${this.ID}Submit`).removeAttr('disabled');
                _this.$el.find(`#btn${this.ID}Submit`).removeClass('d-none');
                break;
            case "New":
                $frm.find('input').removeAttr('readonly');
                $frm.find(`select`).filter("[data-principal]").removeAttr('disabled');
                $frm.find('input[data-autogen=true]').attr('readonly', 'readonly');
                _this.$el.find(`#btn${this.ID}Submit`).removeAttr('disabled');
                _this.$el.find(`#btn${this.ID}Submit`).removeClass('d-none');
                break;
            case "View":
            default:
                $frm.find('input').attr('readonly', 'readonly');
                $frm.find(`select`).attr('disabled', 'disabled');
                _this.$el.find(`#btn${this.ID}Submit`).attr('disabled', 'disabled');
                _this.$el.find(`#btn${this.ID}Submit`).addClass('d-none');
                break;
        }
        $frm.removeClass('was-validated');
        $frm[0].reset();
        let $select2Arr = $frm.find('select').filter("[data-principal]");
        if (data) {
            a2n.setFormValue($frm, data);
            for (let i = 0; i < $select2Arr.length; i++) {
                let $select2 = $($select2Arr[i]);

                let val = data[$select2.attr('name')];
                let principal = $select2.attr('data-principal');
                let keyField = $select2.attr('data-keyfield');
                let labelfield = $select2.attr('data-labelfield');
                let apiReadUrl = a2n.dyndata.Configuration.getApiRead(_this.controller, principal);
                let keyData = {};
                keyData[keyField] = val;
                a2n.submitAjaxJsonPost(apiReadUrl, JSON.stringify(keyData), function (result) {
                    let resultData = { id: result[keyField], text: result[labelfield] };
                    a2n.select2SetValue($select2, resultData, true);
                });
            }
        }
        else {
            a2n.select2SetValue($select2Arr, null, true);
        }
        $(`#mdl${_this.ID}`).modal('show');
    }
}
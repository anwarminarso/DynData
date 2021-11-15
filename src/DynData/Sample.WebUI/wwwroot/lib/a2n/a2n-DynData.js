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
    API_METADATA_TEMPLATE: "/api/dyndata/${viewName}/metadata",
    API_METADATAQB_TEMPLATE: "/api/dyndata/${viewName}/metadataQB",
    API_DATATABLE_TEMPLATE: "/api/dyndata/${viewName}/datatable",
    API_CREATE: "/api/dyndata/${viewName}/create",
    API_READ: "/api/dyndata/${viewName}/read",
    API_UPDATE: "/api/dyndata/${viewName}/update",
    API_DELETE: "/api/dyndata/${viewName}/delete",
    getApiMetadata: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_METADATAQB_TEMPLATE + "`");
    },
    getApiDataTable: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DATATABLE_TEMPLATE + "`");
    },
    getApiCreate: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_CREATE + "`");
    },
    getApiRead: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_READ + "`");
    },
    getApiUpdate: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_UPDATE + "`");
    },
    getApiDelete: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DELETE + "`");
    }
};

a2n.dyndata.Utils = {
    dtInstances: {},
    QBInstance: null
}
a2n.dyndata.DataTable = function (tableId, element, viewName, options) {
    this.ID = tableId;
    this.$el = $(element);
    this.viewName = viewName;
    this.dt = null;
    this.qbRuleSet = null;
    this.externalFilter = null;
    this.dynOptions = {
        allowCreate: false,
        allowUpdate: false,
        allowDelete: false,
        allowView: false,
        rowCommandButtons: [],
        onRowCommand: function (commandName, rowIndex, rowDate) { },
        enableQueryBuilder: true,
        metaData: [],
        queryBuilderOptions: null,
        minGlobalSearchCharLength: 3,
        useBuiltInSearchMode: false
    };
    if (options && options.dynOptions) {
        this.dynOptions = $.extend(this.dynOptions, options.dynOptions);
    }

    if (this.dynOptions.allowView)
        this.dynOptions.rowCommandButtons.push({ commandName: 'View', title: 'View', iconCls: 'fa fa-search' });
    if (this.dynOptions.allowUpdate)
        this.dynOptions.rowCommandButtons.push({ commandName: 'Edit', title: 'Edit', iconCls: 'fa fa-edit' });
    if (this.dynOptions.allowDelete)
        this.dynOptions.rowCommandButtons.push({ commandName: 'Delete', title: 'Delete', iconCls: 'fa fa-trash-alt' });

    let ajaxUrl = a2n.dyndata.Configuration.getApiDataTable(this.viewName);
    a2n.dyndata.Utils.dtInstances[tableId] = this;

    let buttons = [];
    if (this.dynOptions.allowCreate) {
        buttons.push({
            text: '<i class="fas fa-plus mr-1"></i>New',
            className: 'btn btn-success btn-sm',
            action: function (e, dt, node, config) {

            }
        });
    }
    if (this.dynOptions.enableQueryBuilder) {
        if (!a2n.dyndata.Utils.QBInstance) {
            a2n.dyndata.Utils.QBInstance = new a2n.dyndata.QueryBuilder();
        }
        buttons.push({
            text: '<i class="fas fa-filter mr-1"></i>Advanced Search',
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
        let _apiMetaUrl = a2n.dyndata.Configuration.getApiMetadata(_this.viewName);
        $.getJSON(_apiMetaUrl, function (result) {
            _this.dynOptions.metaData = result.metaData;
            _this.dynOptions.queryBuilderOptions = result.queryBuilderOptions;
            _this.dynOptions.isTable = result.isTable;
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
                    $row.append(`<th data-visible='false' data-name="${data.FieldName}">${data.FieldLabel}</th>`);
                else
                    $row.append(`<th data-name="${data.FieldName}">${data.FieldLabel}</th>`);
            }
            else
                $row.append(`<th data-name="${data.FieldName}">${data.FieldLabel}</th>`);
            columns.push({ data: data.FieldName, name: data.FieldName, title: data.FieldLabel });
        }
        if (_this.dynOptions.rowCommandButtons && _this.dynOptions.rowCommandButtons.length > 0) {
            $row.append('<th data-searchable="false" data-sortable="false">Action</th>');
            let actionRenderer = "";
            for (let i = 0; i < _this.dynOptions.rowCommandButtons.length; i++) {
                let btn = _this.dynOptions.rowCommandButtons[i];
                let btnEl = `<a class="btn btn-outline-primary btn-icon rounded-circle" onclick="a2n.dyndata.Utils.dtInstances.${_this.ID}.RowCommand('${btn.commandName}', METAROWCODE)" title="${btn.title}"><i class="${btn.iconCls}"></i></a>`;
                actionRenderer += btnEl;
            }
            actionRenderer = actionRenderer.replace(new RegExp("METAROWCODE", 'g'), "${meta.row}");
            columnDefs.push({
                targets: columns.length,
                data: null,
                render: function (data, type, row, meta) {
                        return eval("`" + actionRenderer + "`");
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
        let rowData = this.dt.row(rowIndex).data();
        switch (commandName) {
            case "Edit":
                break;
            case "Delete":
                break;
            case "Update":
                break;
            default:
                if (this.onRowCommand) {
                    this.onRowCommand(commandName, rowIndex, rowData);
                }
                break;
        }
    },
    LoadData: function () {
        if (!this._IsRendered)
            this.Render();
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
    }
}
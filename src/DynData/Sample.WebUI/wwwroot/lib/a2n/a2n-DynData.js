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
    getApiMetadata: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_METADATAQB_TEMPLATE + "`");
    },
    getApiDataTable: function (viewName) {
        return eval("`" + a2n.dyndata.Configuration.API_DATATABLE_TEMPLATE + "`");
    }
};

a2n.dyndata.Utils = {
    dataTableInstances: {}
}
a2n.dyndata.DataTable = function (tableId, element, viewName, options) {
    this.ID = tableId;
    this.$el = $(element);
    this.viewName = viewName;
    let ajaxUrl = a2n.dyndata.Configuration.getApiDataTable(this.viewName);
    this.options = {
        metaData: [],
        queryBuilderOptions: null,
        minGlobalSearchCharLength: 3,
        useBuiltInSearchMode: false,
    };
    if (options) {
        this.options = $.extend({}, {

        }, options);
    }
    a2n.dyndata.Utils.dataTableInstances[tableId] = {
        instance: this,
        dt: null,
        externalFilter: null,
        qbRuleSet: null
    };
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
                let obj = a2n.dyndata.Utils.dataTableInstances[tableId];
                r.viewName = viewName;
                r.id = tableId;
                if (obj.externalFilter)
                    r.externalFilter = obj.externalFilter;
                if (obj.qbRuleSet) {
                    r.jsonQB = JSON.stringify({
                        referenceType: obj.instance.viewName,
                        ruleData: obj.qbRuleSet
                    });
                }
            }
        }
    };

    if (this.options && this.options.tableOptions)
        this.tableOptions = $.extend(this.tableOptions, this.options.tableOptions);
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

}
a2n.dyndata.DataTable.prototype = {
    _IsRendered: false,
    LoadMetadata: function (render) {
        let _this = this;
        let _render = render;
        let _apiMetaUrl = a2n.dyndata.Configuration.getApiMetadata(_this.viewName);
        $.getJSON(_apiMetaUrl, function (result) {
            _this.options.metaData = result.metaData;
            _this.options.queryBuilderOptions = result.queryBuilderOptions;
            if (_render)
                _this.Render();
        });
    },
    Render: function () {
        let _this = this;
        if (_this._IsRendered)
            return;
        if (!_this.options.metaData || _this.options.metaData.length == 0) {
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
        for (var i = 0; i < _this.options.metaData.length; i++) {
            let data = _this.options.metaData[i];
            if (data.CustomAttributes) {
                if (data.CustomAttributes.Hidden)
                    $row.append(`<th data-visible='false' data-name="${data.FieldName}">${data.FieldLabel}</th>`);
                else
                    $row.append(`<th data-name="${data.FieldName}">${data.FieldLabel}</th>`);
            }
            columns.push({ data: data.FieldName, name: data.FieldName, title: data.FieldLabel });
        }
        _this.$el.html("");
        _this.$el.append($tbl);
        _this.tableOptions.columns = columns;
        _this.tableOptions.columnDefs = columnDefs;
        let dt = $tbl.DataTable(_this.tableOptions);
        a2n.dyndata.Utils.dataTableInstances[_this.ID].dt = dt;
        //let $inputSearch = $(`#${_this.ID} input[type=search]`);
        if (!_this.options.useBuiltInSearchMode) {
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
                        if (val.length > 0 && val.length < _this.options.minGlobalSearchCharLength)
                            return;
                        dt.search(val).draw();
                    }
                }).on('blur', function (e) {
                    if (!this.value)
                        this.value = "";
                    var val = this.value;
                    if (val.length > 0 && val.length < _this.options.minGlobalSearchCharLength)
                        return;
                    dt.search(val).draw();
                });
        }
        _this._IsRendered = true;
    },
    LoadData: function () {
        if (!this._IsRendered)
            this.Render();
    },

    Destroy: function () {

        if (a2n.dyndata.Utils.dataTableInstances[this.ID].dt) {
            a2n.dyndata.Utils.dataTableInstances[this.ID].dt.destroy();
        }
        this.$el.html('');
        delete a2n.dyndata.Utils.dataTableInstances[this.ID];
    }
}
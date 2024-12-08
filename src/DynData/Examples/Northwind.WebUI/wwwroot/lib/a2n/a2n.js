/// <reference path="../jquery/dist/jquery.js" />
/// <reference path="../bootstrap/dist/js/bootstrap.bundle.js" />


// a2n
// version 3.0.0
// © Anwar Minarso, 2021-2024
// https://github.com/anwarminarso
//
// GitHub page:     https://github.com/anwarminarso/DynData
//
// Released under MIT licence:
// =====================================================================================================================

(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports) :
        typeof define === 'function' && define.amd ? define(['exports'], factory) :
            (global = global || self, factory(global.a2n = global.a2n || {}));
}(this, function (exports) {
    'use strict';
    a2n = {
        _dummyFormId: `tmpForm${Date.now()}`,
        submitAjaxPost: function (url, data, onSuccess, onError) {
            $.ajax({
                type: "POST",
                headers: {
                    //for ASP.NET Validation @Html.AntiForgeryToken() if declared on Page
                    "RequestVerificationToken": $('input:hidden[name="__RequestVerificationToken"]').first().val()
                },
                url: url,
                data: data,
                success: function (data, status, xhr) {
                    if (onSuccess)
                        onSuccess(data, status, xhr);
                },
                error: function (req, status, error) {
                    if (onError)
                        onError(req, status, error);
                }
            });
        },
        submitAjaxJsonPost: function (url, jsonData, onSuccess, onError) {
            // submit raw json data
            $.ajax({
                type: "POST",
                headers: {
                    "RequestVerificationToken": $('input:hidden[name="__RequestVerificationToken"]').first().val()
                },
                url: url,
                processData: false,
                contentType: 'application/json',
                data: jsonData,
                success: function (data, status, xhr) {
                    if (onSuccess)
                        onSuccess(data, status, xhr);
                },
                error: function (req, status, error) {
                    if (onError)
                        onError(req, status, error);
                }
            });
        },
        submitPost: function (url, data, frmId) {
            var _frmId = "";
            var keys = data !== undefined ? Object.keys(data) : {};
            var values = data !== undefined ? Object.values(data) : [];
            if (frmId) {
                _frmId = frmId;
                if (_frmId.indexOf("#") < 0)
                    _frmId = '#' + frmId;
            }
            else {
                _frmId = '#' + a2n._dummyFormId;
                $(_frmId).remove();
            }

            if ($(_frmId).length === 0) {
                var frmHtml = `<form method='post' id='${_frmId.replace('#', '')}'>`;
                for (var i = 0; i < keys.length; i++) {
                    frmHtml += `<input type='hidden' name='${keys[i]}' />`;
                }
                frmHtml += "</form>";
                $('body').append(frmHtml);
            }

            //for ASP.NET Validation @Html.AntiForgeryToken() if declared on Page
            $(_frmId).find('input:hidden[name!="__RequestVerificationToken"]').val("");
            var token = $('input:hidden[name="__RequestVerificationToken"]').first().val();
            for (var j = 0; j < keys.length; j++) {
                $(_frmId).find(`input:hidden[name=${keys[j]}]`).val(values[j]);
            }
            if (token) {
                if ($(_frmId).find('input:hidden[name="__RequestVerificationToken"]').length === 0) {
                    $(_frmId).append("<input type='hidden' name='__RequestVerificationToken' />");
                }
                $(_frmId).find('input:hidden[name="__RequestVerificationToken"]').val(token);
            }
            if (url)
                $(_frmId).attr('action', url);
            else
                $(_frmId).removeAttr('action');
            $(_frmId).submit();
        },
        submitAjaxForm: function ($frm, url, onSuccess, onError) {
            var opt = {
                type: "POST",
                headers: {
                    "RequestVerificationToken": $('input:hidden[name="__RequestVerificationToken"]').first().val()
                },
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: $frm.serialize(),
                success: function (data, status, xhr) {
                    if (onSuccess)
                        onSuccess(data, status, xhr);
                },
                error: function (req, status, error) {
                    if (onError)
                        onError(req, status, error);
                }
            };
            if (url)
                opt.url = url;

            $.ajax(opt);
        },
        
        createObjectFromFormInputName: function ($formSelector) {
            $formSelector.find('[disabled]').addClass('tempDisabled');
            $formSelector.find('[disabled]').removeAttr('disabled');
            var nameValueArr = $formSelector.serializeArray();
            var objVal = new Object();
            jQuery.each(nameValueArr, function (i, field) {
                objVal[field.name] = field.value;
            });
            let $cbs = $formSelector.find('input:checkbox[name]');
            if ($cbs.length > 0) {
                let cbNames = [];
                jQuery.each($cbs, function (i, el) {
                    let $el = $(el);
                    let name = $el.attr('name');
                    if (cbNames.indexOf(name) < 0)
                        cbNames.push(name)
                    objVal[$el.attr('name')] = $el.prop('checked');
                });
                jQuery.each(cbNames, function (i, name) {
                    let $cbNames = $formSelector.find(`input:checkbox[name=${name}]`);
                    if ($cbNames.length > 1) {
                        objVal[name] = [];
                        jQuery.each($cbNames, function (j, el) {
                            let $el = $(el);
                            if (!$el.attr('value'))
                                objVal[name].push($el.prop('checked'));
                            else if ($el.prop('checked'))
                                objVal[name].push($el.attr('value'));
                        });
                    }
                    else {
                        if (!$cbNames.attr('value'))
                            objVal[name] = $cbNames.prop('checked');
                        else if ($cbNames.prop('checked'))
                            objVal[name] = $cbNames.attr('value');
                        else
                            objVal[name] = null;
                    }
                });
            }
            $formSelector.find('.tempDisabled').attr('disabled', 'disabled');
            $formSelector.find('[disabled]').removeClass('tempDisabled');
            return objVal;
        },
        copyFields: function (from, to) {
            var keys = Object.keys(to);
            for (var i = 0; i < keys.length; i++) {
                let k = keys[i];
                if (to[k] instanceof Function)
                    continue;
                if (from.hasOwnProperty(k))
                    to[k] = from[k];
                else if (from[k] !== undefined)
                    to[k] = from[k];
            }
        },
        setFormValue: function ($formSelector, data) {
            let propNames = Object.keys(data);
            for (var i = 0; i < propNames.length; i++) {
                let propName = propNames[i];
                let $input = $formSelector.find(`[name=${propName}]`);

                if ($input.is('input')) {
                    if ($input.attr('type') !== 'checkbox' && $input.attr('type') !== 'radio' && $input.attr('type') !== 'date' && $input.attr('type') !== 'time') {
                        $input.val(data[propName]);
                    }
                    else {
                        switch($input.attr('type')) {
                            case 'checkbox':
                                {
                                    if ($input.length > 1) {
                                        if (Array.isArray(data[propName]))
                                            $input.val(data[propName]);
                                        else
                                            $input.val([data[propName]]);
                                    }
                                    else {
                                        if ($input.attr('value')) {
                                            if (data[propName] && $input.attr('value') == data[propName])
                                                $input.prop('checked', 'checked');
                                            else
                                                $input.prop('checked', '');
                                        }
                                        else if (data[propName])
                                            $input.prop('checked', 'checked');
                                        else
                                            $input.prop('checked', '');
                                    }
                                    $input.change();
                                }
                                break;
                            case 'radio':
                                {
                                    if (data[propName]) {
                                        if (Array.isArray(data[propName]))
                                            $input.val(data[propName]);
                                        else
                                            $input.val([data[propName]]);
                                    }
                                    else
                                        $input.prop('checked', '');
                                }
                                $input.change();
                                break;
                            case 'date':
                                {
                                    let val = data[propName];
                                    let dt = null;
                                    if (val) {
                                        if (val instanceof Date)
                                            dt = val;
                                        else {
                                            try {
                                                dt = new Date(val);
                                            } catch (e) {
                                                dt = null;
                                            }
                                        }
                                    }
                                    if (dt && dt instanceof Date && !isNaN(dt)) {
                                        let dtStr = `${dt.getFullYear()}-${a2n.padZero(dt.getMonth() + 1, 2)}-${a2n.padZero(dt.getDate(), 2)}`;
                                        //$input.val(dt.toISOString().substr(0, 10));
                                        $input.val(dtStr);
                                    }
                                    else {
                                        $input.val(null);
                                    }
                                    $input.change();
                                }
                                break;
                            case 'time':
                                {
                                    let val = data[propName];
                                    let dt = null;
                                    if (val) {
                                        if (val instanceof Date)
                                            dt = val;
                                        else {
                                            try {
                                                dt = new Date(val);
                                            } catch (e) {
                                                dt = null;
                                            }
                                        }
                                    }
                                    if (dt && dt instanceof Date && !isNaN(dt)) {
                                        let dtStr = `${a2n.padZero(dt.getHours(), 2)}-${a2n.padZero(dt.getMinutes(), 2)}`;
                                        //$input.val(dt.toISOString().substr(0, 10));
                                        $input.val(dtStr);
                                    }
                                    else if (val) {
                                        $input.val(val);
                                    }
                                    else {
                                        $input.val(null);
                                    }
                                    $input.change();
                                }
                                break;
                            default:
                                $input.val(data[propName]);
                                $input.change();
                                break;
                        }
                    }
                }
                else if ($input.is('textarea')) {
                    $input.val(data[propName]);
                }
                else if ($input.is('select')) {
                    $input.val(data[propName]);
                    $input.change();
                }
            }
        },
        getObjectValueByPath: function (data, path) {
            if (!path)
                return data;
            var obj = data;
            var paths = path.split('.');
            for (var i = 0; i < paths.length; i++) {
                obj = obj[paths[i]];
            }
            return obj;
        },
        applyBindingPath: function (data, bindingPath, throwError) {
            bindingPath = bindingPath.replace(/\[(\w+)\]/g, '.$1'); // convert indexes to properties
            bindingPath = bindingPath.replace(/^\./, '');           // strip a leading dot
            var a = bindingPath.split('.');
            for (var i = 0, n = a.length; i < n; ++i) {
                var k = a[i];
                if (k in data) {
                    data = data[k];
                } else {
                    if (throwError)
                        throw 'Error';
                    return;
                }
            }
            return data;
        },
        //select2 for set value
        select2SetValue: function ($selector, data, ajax) {
            if (!data) {
                $selector.val(null).trigger('change');
                return;
            }
            else {
                if ($selector.find("option[value='" + data.id + "']").length) {
                    $selector.val(data.id).trigger('change');
                } else {
                    var newOption = new Option(data.text, data.id, true, true);
                    $selector.append(newOption).trigger('change');
                }
            }
            if (ajax) {
                $selector.trigger({
                    type: 'select2:select',
                    params: {
                        data: data
                    }
                });
            }
        },
        playSound: function (path, sound) {
            let $audioElement = $('#appAudio');
            if ($audioElement.length === 0) {
                $audioElement = $(`<audio id="appAudio"></audio>`);
                $('body').append($audioElement);
            }
            if (navigator.userAgent.match('Firefox/'))
                $audioElement.attr('src', path + "/" + sound + '.ogg');
            else
                $audioElement.attr('src', path + "/" + sound + '.mp3');

            $audioElement.on("load", function () {
                $audioElement[0].play();
            });
            $audioElement[0].pause();
            $audioElement[0].play();
        },
        showConfirm: function (fun, options) {
            var opt = {
                title: "<i class='fal fa-times-circle text-danger mr-2'></i> Do you wish to delete this item?",
                message: "<span><strong>Warning:</strong> This action cannot be undone!</span>",
                centerVertical: true,
                swapButtonOrder: false,
                buttons: {
                    confirm: {
                        label: 'Yes',
                        className: 'btn-danger'
                    },
                    cancel: {
                        label: 'No',
                        className: 'btn-default mr-3'
                    }
                },
                className: "modal-alert",
                closeButton: false,
                callback: fun
            };
            if (options)
                $.extend(opt, options);
            a2n.playSound('/media/sound', 'bigbox');
            bootbox.confirm(opt);
        },
        showNotificationDialog: function (message, fun, options) {
            var opt = {
                title: "<i class='fal fa-info text-info mr-2'></i> Notification",
                message: `<span class="ml-4">${message}</span>`,
                centerVertical: true,
                backdrop: true,
                swapButtonOrder: false,
                buttons: {
                    ok: {
                        label: 'Ok',
                        className: 'btn-success'
                    }
                },
                className: "modal-alert",
                closeButton: false,
                callback: fun
            };
            if (options)
                $.extend(opt, options);
            a2n.playSound('/media/sound', 'voice_on');
            bootbox.alert(opt);
        },
        reformatJSON: function (value) {
            // Replace ":" with "@colon@" if it's between double-quotes
            var newValue = value.replace(/:\s*"([^"]*)"/g, function (match, p1) {
            return ':"' + p1.replace(/:/g, '@colon@') + '"';
            })
            // Replace ":" with "@colon@" if it's between single-quotes
            .replace(/:\s*'([^']*)'/g, function (match, p1) {
                return ':"' + p1.replace(/:/g, '@colon@') + '"';
            })
            // Add double-quotes around any tokens before the remaining ":"
            .replace(/(['"])?([a-z0-9A-Z_]+)(['"])?\s*:/g, '"$2": ')
            // Turn "@colon@" back into ":"
            .replace(/@colon@/g, ':');
            return newValue;
        },
        getFieldType: function (value) {
            if (value === undefined)
                return null;
            if (value instanceof Date)
                return "Date";
            switch (typeof value) {
                case "string":
                    {
                        let dt = new Date(value);
                        if (isNaN(dt))
                            return "String";
                        else
                            return "Date";
                    }
                case "boolean":
                    return "Boolean";
                case "number":
                    {
                        if (Number.isInteger(value)) {
                            if (value.toString().indexOf(".") >= 0)
                                return "Float";
                            else
                                return "Integer";
                        }
                        else {
                            return "Float";
                        }
                    }
                default:
                    if (Array.isArray(value))
                        return "Array";
                    else
                        return "Object";
            }
        },
        JSONParse: function (value) {
            let data = null;
            try {
                data = JSON.parse(value);
            }
            catch (e) {
                let value2 = a2n.reformatJSON(value);
                data = JSON.parse(value2);
            }
            return data;
        },
        padZero(str, len) {
            len = len || 2;
            var zeros = new Array(len).join('0');
            return (zeros + str).slice(-len);
        },
        createIconPicker: function ($selector, options) {
            let defaultOpt = {
                templateResult: a2n.Utils.generateIconOption,
                templateSelection: a2n.Utils.generateIconOption,
                allowClear: true,
                placeholder: "Select an icon",
                minimumInputLength: 3,
                iconTypes: "light,nextgen"
            };
            let opt = null;
            if (options)
                opt = $.extend({}, defaultOpt, options);
            else
                opt = defaultOpt;
            $selector.attr('data-iconTypes-filter', opt.iconTypes);
            let $el = $($selector);
            opt.ajax = {
                url: '/api/icon/search',
                type: "GET",
                dataType: 'json',
                data: function (params) {
                    var query = {
                        search: params.term,
                        pageIndex: params.nextPageIndex || 0,
                        pageSize: params.pageSize || 20,
                        iconTypes: $el.attr('data-iconTypes-filter')
                    }
                    return query;
                },
                processResults: function (data, params) {
                    params.pageIndex = data.pageIndex || 0;
                    params.pageSize = data.pageSize || 20;
                    params.nextPageIndex = params.pageIndex;
                    var more = data.pageSize ? ((data.pageIndex + 1) * data.pageSize) < data.totalRows : false;
                    if (more)
                        params.nextPageIndex++;
                    return {
                        results: data.items,
                        pagination: {
                            more: more
                        }
                    };
                },
                cache: false
            }
            $selector.select2(opt);
        },
        Utils: {
            generateIconOption: function (data) {
                var result = `<div><i class="${data.id} mr-4"></i><span>${data.text}</span></div>`;
                return $(result);
            }
        }
    }
}));
String.prototype.interpolate = function (params) {
    const names = Object.keys(params);
    const vals = Object.values(params);
    return new Function(...names, `return \`${this}\`;`)(...vals);
}
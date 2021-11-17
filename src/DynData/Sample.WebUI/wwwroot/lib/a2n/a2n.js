/// <reference path="../jquery/dist/jquery.js" />
/// <reference path="../bootstrap/dist/js/bootstrap.bundle.js" />


// myRaspNet
// version 1.0.0
// © Anwar Minarso, 2021
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
                _frmId = '#' + myRaspNet._dummyFormId;
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
                $(_frmId).find(String.format("input:hidden[name='{0}']", keys[j])).val(values[j]);
            }
            if (token) {
                if ($(_frmId).find('input:hidden[name="__RequestVerificationToken"]').length === 0) {
                    $(_frmId).append("<input type='hidden' name='__RequestVerificationToken' />");
                }
                $(_frmId).find('input:hidden[name="__RequestVerificationToken"]').val(token);
            }

            $(_frmId).attr('action', url);
            $(_frmId).submit();
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
            jQuery.each($cbs, function (i, el) {
                let $el = $(el);
                objVal[$el.attr('name')] = $el.prop('checked');
            });
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
                    if ($input.attr('type') !== 'checkbox' && $input.attr('type') !== 'radio') {
                        $input.val(data[propName]);
                    }
                    else {
                        if (data[propName])
                            $input.prop('checked', 'checked');
                        else
                            $input.prop('checked', '');
                        $input.change();
                    }
                }
                else if ($input.is('select')) {
                    $input.val(data[propName]);
                    $input.change();
                }
            }
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
        showConfirmDelete: function (fun, options) {
            var opt = {
                title: "<i class='fa fa-times-circle text-danger mr-2'></i> Do you wish to delete this item?",
                message: "<br /><br /><span><strong>Warning:</strong> This action cannot be undone!</span>",
                centerVertical: true,
                swapButtonOrder: false,
                buttons: {
                    confirm: {
                        label: 'Yes',
                        className: 'btn-danger'
                    },
                    cancel: {
                        label: 'No',
                        className: 'btn-primary mr-3'
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
        }
    }
}));
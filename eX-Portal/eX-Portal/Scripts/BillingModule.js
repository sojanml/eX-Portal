$(document).ready(function () {
  BillingModule.Initilize();
});

var BillingModule = function () {
  var RulesTables = [];
  var RulesTableFields = {};

  var _initilize = function () {
    $.ajax({
      type: "GET",
      url: '/Billing/Init',
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: _PopulateListItems
    });

    $('#CalculateOn').on("change", _OnCalcuateOnChange);
    $('#CalculateField').on("change", _OnCalculateFieldChange);

  };

  var _PopulateListItems = function (JsonData) {
    var Data = JsonData.ListItems;
    for (var i = 0; i < Data.length; i++) {
      RulesTables.push(Data[i].TableName);
      RulesTableFields[Data[i].TableName.Value] = Data[i].Fields;
    }
    _FillListBox('CalculateOn', RulesTables, _Form.CalculateOn);
    console.log(Data);
  }

  var _FillListBox = function (ListBoxID, ListBoxData, SelectedValue) {
    var ListBox = $('#' + ListBoxID);
    var ListItems = '<option value=""' +
      (SelectedValue == "" ? ' selected' : '') +
      '>Please Select...</option>\n';
    for (var i = 0; i < ListBoxData.length; i++) {
      ListItems += '<option value="' + ListBoxData[i].Value + '"' +
        (SelectedValue == ListBoxData[i].Value ? ' selected' : '') + '>' + ListBoxData[i].Name + '</option >\n';
    }
    ListBox.html(ListItems);
    if (SelectedValue !== '') ListBox.trigger("change");
  }

  var _OnCalcuateOnChange = function (e) {
    var ListBox = $(this);
    var Value = ListBox.val();
    if (Value != "") {
      _FillListBox('CalculateField', RulesTableFields[Value], _Form.CalculateField)
    } else {
      _FillListBox('CalculateField', [], _Form.CalculateField)
    }
    _SetCustomLayerDisplay(RulesTableFields[Value], _Form.CalculateField);
    
  }

  var _SetCustomLayerDisplay = function (CheckArray, CheckVal) {
    var isFound = false;
    if (CheckVal == "") {
      isFound = true;
    } else {
      for (var i = 0; i < CheckArray.length; i++) {
        if (CheckArray[i].Value == CheckVal) {
          isFound = true;
          break;
        }
      }
    }
    if (isFound) {
      $('#CalculateFieldCustomLayer').css({ 'display': 'none' });
      $('#CalculateFieldCustom').val('');
    } else {
      $('#CalculateFieldCustomLayer').css({ 'display': 'block' });
      $('#CalculateFieldCustom').val(CheckVal);
      $('#CalculateField').val("Custom");
    }

  };

  var _OnCalculateFieldChange = function (e) {
    var ListBox = $(this);
    var Value = ListBox.find(':selected').text();
    $('#CalculateFieldCustomLayer').css({ 'display': (Value == 'Custom' ? 'block' : 'none') });
  }

  return {
    Initilize: _initilize
  };
}();
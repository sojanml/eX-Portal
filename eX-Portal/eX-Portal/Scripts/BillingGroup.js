$(document).ready(function () {
  BillingGroup.InitSort();
  BillingGroup.Initilize();
});

var BillingGroup = function () {
  var _sorter = {};

  var _initSort = function () {
    _sorter = $('#BillingRules').rowSorter({
      handler: 'td.sorter',
      stickTopRows: 1,
      onDragStart: function (tbody, row, index) {
      },
      onDrop: _ReindexRules
    });
  };

  var _ReindexRules = function (tbody, row, new_index, old_index) {
    var Index = '';
    $('input.RuleID').each(function (index) {
      if (Index != '') Index += ',';
      Index += $(this).val();
    })
    $('#RulesIndex').val(Index);
  };

  var _initilize = function () {
    $('input.NumberInput').on("change", function (e) {
      var v = $(this).val();
      var i = parseFloat(v);
      if (isNaN(i)) i = 0;
      $(this).val(i.toFixed(2));
    });
  };



  return {
    Initilize: _initilize,
    InitSort: _initSort
  };
}();
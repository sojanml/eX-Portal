using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel {

  public partial class BlackBoxCostCalucation : BlackBoxCost {
    private bool _isSelected = false;
    public bool isSelected {
      get { return _isSelected; }
      set { _isSelected = value; }
    }
    public Double CalcuatedCost { get; set; }
    public int CalcuatedDays { get; set; }
    public int SectionDays { get; set; }

    public Double getItemCost(int NumOfDays) {
      CalcuatedDays = NumOfDays;
      if (RentAmount == null) RentAmount = 0;
      int TheSecion = NumOfDays / RentDays ;
      if (NumOfDays % RentDays > 0) TheSecion++;
      if (TheSecion < 1) TheSecion = 1;
      CalcuatedCost = TheSecion * (Double)RentAmount;
      SectionDays = TheSecion;
      return CalcuatedCost;
    }

  }





  public class BlackBoxViewModel {
    public IEnumerable<SelectListItem> BlackBoxList { get; set; }
    public BlackBoxTransaction BBTransaction { get; set; }
    public IEnumerable<SelectListItem> CollectionMode { get; set; }

    public List<List<BlackBoxCostCalucation>> BlackBoxCostList { get; set; }
  }


}
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class CheckListView
    {
        public MSTR_DroneCheckList CheckHeader { get; set; }
        public List<MSTR_DroneCheckListItems> CheckList{get;set;}
        public bool IsSelected { get; set; }

    }

    public class CheckItemView
    {
        public MSTR_DroneCheckListItems CheckItem { get; set; }
        public bool IsSelected { get; set; }
        public CheckItemView()
        {
            CheckItem = new MSTR_DroneCheckListItems();
            IsSelected = false;
        }
    }

}
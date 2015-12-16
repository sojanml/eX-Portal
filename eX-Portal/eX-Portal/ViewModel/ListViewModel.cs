using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using System.ComponentModel.DataAnnotations;
namespace eX_Portal.ViewModel
{
    public class ListViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public Nullable<int> TypeId { get; set; }
        public string BinaryCode { get; set; }
      
        public string Name { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public Nullable<int> RecordType { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public IEnumerable<SelectListItem> Typelist { get; set; }
        public IEnumerable<eX_Portal.Models.LUP_Drone> NameList { get; set; }      
        public string TypeCopy { get; set; }
    }
}
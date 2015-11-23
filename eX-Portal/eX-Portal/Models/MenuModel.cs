using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.Models
{
    public class MenuModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
    }


   
}
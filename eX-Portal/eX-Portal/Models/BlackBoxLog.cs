using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.Models
{
   
        using System;
        using System.Collections.Generic;

        public partial class BlackBoxLog
        {
            public int LogId { get; set; }
            public string FileName { get; set; }
            public Nullable<bool> IsProcessed { get; set; }
            public Nullable<System.DateTime> CreatedDate { get; set; }
        }
    

}
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class GCAAFlightApproval
    {
        public ApproalDetail ApprovalDetails { get; set; }
        public GCA_Approval GcaApproval { get; set; }
    }
}
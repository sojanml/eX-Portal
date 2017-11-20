using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eX_Portal.Models {

  [MetadataType(typeof(MSTR_NOC_DataAnnotationsHelper))]
  public partial class MSTR_NOC { }

  public class MSTR_NOC_DataAnnotationsHelper {
    [Required]
    [Display(Name ="Flight Type")]
    public string FlightType { get; set; }
    [Required]
    [Display(Name = "Flight For (Client Name)")]
    public string FlightFor { get; set; }
    [Required]
    public int AccountID { get; set; }
    [Required]
    public int CreateBy { get; set; }
    [Required]
    public System.DateTime CreatedOn { get; set; }
  }//NOC_DataAnnotationsHelper

}
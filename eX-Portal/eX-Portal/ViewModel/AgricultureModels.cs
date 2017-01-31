using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class AgricultureFilter {
    public String CustomerReference { get; set; } = String.Empty;
    public String AccountNumber { get; set; } = String.Empty;
    public int PrincipalAmountFrom { get; set; } = 0;
    public int PrincipalAmountTo { get; set; } = 0;
    public DateTime? DisbursementDateFrom { get; set; }
    public DateTime? DisbursementDateTo { get; set; }
    public String BranchID { get; set; } = String.Empty;
    public String LoanOfficer { get; set; } = "-";
    public int Tenor { get; set; } = 0;
    public String Images { get; set; }

    public IQueryable<Object> SetFilter(Models.ExponentPortalEntities db) {
      var List = db.AgriTraxManagements.Select(e => new {
        AgriTraxID = e.AgriTraxID,
        CustomerReference = e.CustomerReference,
        DisburementDate = e.DisburementDate,
        Tenor = e.Tenor,
        PrincipalAmount = e.PrincipalAmount,
        BranchID = e.BranchID,
        LoanOfficer = e.LoanOfficer,
        LandAddress = e.LandAddress,
        LandSize = e.LandSize,
        Lat = e.Lat,
        Lng = e.Lng,
        AccountNumber = e.AccountNumber,
        DisbursementDate = e.DisburementDate,
        InspectionOfficer = e.InspectionOfficer,
        NextSiteVisitDate = e.NextSiteVisitDate,
        SiteVisitDate = e.SiteVisitDate,
        InspectionNote = e.InspectionNote,
        Images = e.Images
      });

      if (!String.IsNullOrWhiteSpace(CustomerReference))
        List = List.Where(e => e.CustomerReference.Contains(CustomerReference));
      if(!String.IsNullOrWhiteSpace(AccountNumber)) 
        List = List.Where(e => e.AccountNumber.Contains(CustomerReference));
      if(PrincipalAmountFrom > 0)
        List = List.Where(e => e.PrincipalAmount >= PrincipalAmountFrom * 1000);
      if (PrincipalAmountTo > 0)
        List = List.Where(e => e.PrincipalAmount <= PrincipalAmountTo * 1000);
      if(DisbursementDateFrom.HasValue)
        List = List.Where(e => e.DisbursementDate >= DisbursementDateFrom);
      if (DisbursementDateTo.HasValue)
        List = List.Where(e => e.DisbursementDate <= DisbursementDateTo);
      if (!String.IsNullOrWhiteSpace(BranchID))
        List = List.Where(e => e.BranchID.Contains(BranchID));
      if (!String.IsNullOrWhiteSpace(LoanOfficer) && LoanOfficer != "-")
        List = List.Where(e => e.LoanOfficer  == LoanOfficer);
      if (Tenor > 0)
        List = List.Where(e => e.Tenor == Tenor);

      return List.OrderBy(e => e.DisburementDate);
    }

  }
}
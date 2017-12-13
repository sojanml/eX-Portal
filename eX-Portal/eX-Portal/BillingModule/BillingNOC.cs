using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace eX_Portal.BillingModule {



  public class BillingNOC: Models.NOC_Details {

    public async Task GenerateFields() {
      Dictionary<String, Decimal> FieldValues = new Dictionary<String, Decimal>();
      String SQL = $@"  
      SELECT
        dbo.PeakMinutes(
          '{this.StartDate.ToString("yyyy-MM-dd")}', '{this.EndDate.ToString("yyyy-MM-dd")}',
          '{this.StartTime.ToString()}', '{this.EndTime.ToString()}'
        ) as BillingPeakMinutes,
        geography::STGeomFromText(
        	'POLYGON((' + dbo.ToogleGeo('{this.Coordinates}') + '))', 4326
        ).STArea() as TotalArea";

      using (var ctx = new ExponentPortalEntities()) {
        using (var cmd =  ctx.Database.Connection.CreateCommand()) {
          await ctx.Database.Connection.OpenAsync();
          cmd.CommandText = SQL;
          using (var reader = await cmd.ExecuteReaderAsync()) {
            if (await reader.ReadAsync()) {
              for (int i = 0; i < reader.FieldCount; i++) {
                FieldValues.Add(reader.GetName(i), Decimal.Parse(reader.GetValue(i).ToString()));
              }//for
            }//if
          }//using reader
        }//using ctx.Database.Connection.CreateCommand
      }//using ExponentPortalEntities

      this.BillingArea = FieldValues["TotalArea"];
      this.BillingPeakMinutes = (int)FieldValues["BillingPeakMinutes"];
      this.BillingDays = (int) (this.EndDate - this.StartDate).TotalDays + 1;
      this.BillingTotalMinutes = (int)(this.EndTime - this.StartTime).TotalMinutes * this.BillingDays;
      this.BillingOffPeakMinutes = this.BillingTotalMinutes - this.BillingPeakMinutes;
      this.BillingVolume = this.BillingArea * this.MaxAltitude;

    }//public async Task GenerateFields
  }//public class BillingNOC: Models.NOC_Details
}//namespace eX_Portal.BillingModule
using eX_Portal.exLogic;
using eX_Portal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace eX_Portal.BillingModule {



  public class BillingNOC: Models.NOC_Details {

    public async Task<Models.DroneFlight> LoadNocForFlight(int FlightID) {
      var ctx = new ExponentPortalEntities();
      var query = ctx.DroneFlight.Where(w => w.ID == FlightID);
      var flight = await query.FirstOrDefaultAsync();
      if (flight == null) return null;
      int ApplicationID = flight.NocApplicationID;
      var noc = await ctx.NOC_Details.Where(w => w.NocID == ApplicationID).FirstOrDefaultAsync();
      if (noc == null)
        return null;
      Initilize(noc);
      
      return flight;
    }

    public void Initilize(Models.NOC_Details noc) {
      this.NocID = noc.NocID;
      this.NocApplicationID = noc.NocApplicationID;
      this.PilotID = noc.PilotID;
      this.DroneID = noc.DroneID;
      this.StartDate = noc.StartDate;
      this.EndDate = noc.EndDate;
      this.StartTime = noc.StartTime;
      this.EndTime = noc.EndTime;
      this.MinAltitude = noc.MinAltitude;
      this.MaxAltitude = noc.MaxAltitude;
      this.Coordinates = noc.Coordinates;
      this.OuterCoordinates = noc.OuterCoordinates;
      this.LOS = noc.LOS;
      this.IsUseCamara = noc.IsUseCamara;
      this.Status = noc.Status;
      this.StatusChangedOn = noc.StatusChangedOn;
      this.StatusChangedBy = noc.StatusChangedBy;
      this.NocBuffer = noc.NocBuffer;
      this.BillingDays = noc.BillingDays;
      this.BillingTotalMinutes = noc.BillingTotalMinutes;
      this.BillingPeakMinutes = noc.BillingPeakMinutes;
      this.BillingOffPeakMinutes = noc.BillingOffPeakMinutes;
      this.BillingArea = noc.BillingArea;
      this.BillingVolume = noc.BillingVolume;
    }

    public Models.NOC_Details GetBase() {
      var o = JsonConvert.DeserializeObject<Models.NOC_Details>(JsonConvert.SerializeObject(this));
      return o;
    }

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class latlng {
    public double lat { get; set; }
    public double lng { get; set; }
  }

  public class DynamicZone {
    public int ID { get; set; }
    public List<latlng> Path {get; set;}

    public void setPath(String Coordinates) {
      Path = new List<latlng>();
      String[] arrayLatLng = Coordinates.Split(',');
      foreach(String sLatLng in arrayLatLng ) {
        String[] aLatLng = sLatLng.Split(' ');
        if(aLatLng.Length == 2) {
          Path.Add(new latlng() {
            lat = Double.Parse(aLatLng[0]),
            lng = Double.Parse(aLatLng[1])
          });
        }
      }
    }
  }
}
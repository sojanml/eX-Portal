
using CesiumLanguageWriter;
using CesiumLanguageWriter.Advanced;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CesiumFlight.Controllers
{
    public class CesiumController : Controller
    {
        public ExponentPortalEntities db = new ExponentPortalEntities();
        // GET: Cesium
        public ActionResult Index(int ID)
        {
            ViewBag.ID = ID;
            return View();
        }

        // GET: Cesium/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Cesium/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Cesium/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Cesium/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Cesium/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Cesium/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Cesium/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Info(int ID)
        {

            var flightmapdatas = db.FlightMapDatas.Where(x => x.FlightID == ID && (x.Altitude>0 || x.Speed>0)).OrderBy(x=>x.FlightMapDataID).ToList();
            int count = flightmapdatas.Count();
            FlightMapData firstItem = flightmapdatas.Take(1).FirstOrDefault();
            FlightMapData LastItem = flightmapdatas.LastOrDefault();
            JulianDate StartTime = new JulianDate(firstItem.ReadTime.Value);
            JulianDate EndTime = new JulianDate(LastItem.ReadTime.Value);
            string curFile = @"C:\Cesium\test.czml";
            FileInfo info = new FileInfo(curFile);
            List<Cartographic> pList = new List<Cartographic>();
            List<JulianDate> JList = new List<JulianDate>();
            List<Double> HList = new List<Double>();
            List<Double> PList = new List<Double>();
            List<Double> RList = new List<Double>();
            //MemoryStream ms = new MemoryStream();
            //if (!info.Exists)
            //{
            // using ()
            //        { 
            MemoryStream ms = new MemoryStream();
                var outputStream = new StreamWriter(ms);
                // Create an output stream writer for the response.
                //using (var outputStream = new StreamWriter(fileStream.BaseStream))
                //{
                    var cesiumWriter = new CesiumStreamWriter();
                var output = new CesiumOutputStream(outputStream)
                {
                    // Since this is a demo, turning on PrettyFormatting makes the response easier to view
                    // with web browser developer tools.  It just adds whitespace and newlines to the response,
                    // so production environments would typically leave this turned off.
                    PrettyFormatting = true
                };

                // The whole body of CZML must be wrapped in a JSON array, opened here.
                output.WriteStartSequence();

                // The first packet (JSON object) of CZML must be the document packet.
                using (var entity = cesiumWriter.OpenPacket(output))
                {

                    entity.WriteId("document");
                    entity.WriteVersion("1.0");
                    using (var clock = entity.OpenClockProperty())
                    {
                        clock.WriteCurrentTime(StartTime);
                        clock.WriteInterval(new TimeInterval(StartTime, EndTime));
                        clock.WriteMultiplier(2);
                        clock.WriteStep(ClockStep.SystemClockMultiplier);
                    }
                }



                // Open a new CZML packet for each point.
                using (var entity = cesiumWriter.OpenPacket(output))
                {

                    entity.WriteId("Aircraft");
                    entity.WriteVersion("1.0");
                    entity.WriteAvailability(new TimeInterval(StartTime, EndTime));

                    using (var model = entity.OpenBillboardProperty())
                    {
                        model.WriteColorProperty(Color.Red);
                        using (var eoffset = model.OpenEyeOffsetProperty())
                        {
                            eoffset.WriteCartesian(new Cartesian());

                        }
                        model.WriteHorizontalOriginProperty(CesiumHorizontalOrigin.Center);
                        using (var poffset = model.OpenPixelOffsetProperty())
                        {
                            poffset.WriteCartesian2(new Rectangular(0, 0));

                        }
                        model.WriteScaleProperty(1);
                        model.WriteShowProperty(true);
                        model.WriteVerticalOriginProperty(CesiumVerticalOrigin.Center);
                        //model.WriteImageProperty(Ima);
                    }

                    using (var path = entity.OpenPathProperty())
                    {
                        //  position.WriteUnitQuaternion();
                        //  path.OpenShowProperty();
                        using (var show = path.OpenShowProperty())
                        {
                            show.WriteInterval(new TimeInterval(StartTime, EndTime));
                            show.WriteBoolean(true);
                        }


                        //  path.OpenInterval();

                        //   path.OpenWidthProperty();
                        path.WriteWidthProperty(1);
                        using (var material = path.OpenMaterialProperty())
                        {
                            using (var sol = material.OpenSolidColorProperty())
                            {
                                sol.WriteColorProperty(0, 255, 255, 255);
                            }

                        }
                        path.WriteResolutionProperty(1200);
                    }

                    using (var position = entity.OpenPositionProperty())
                    {



                        foreach (FlightMapData fmp in flightmapdatas)
                        {

                            double lat = Convert.ToDouble(fmp.Latitude);
                            double lon = Convert.ToDouble(fmp.Longitude);
                            double alt = Convert.ToDouble(fmp.Altitude);
                            Cartographic nC = new Cartographic(lon, lat, alt);
                            pList.Add(nC);
                            JulianDate jd = new JulianDate(fmp.ReadTime.Value);
                            JList.Add(jd);
                            var heading = Convert.ToDouble(fmp.Heading);
                            HList.Add(heading);
                            var pitch = Convert.ToDouble(fmp.Pitch);
                            PList.Add(pitch);

                            var roll = Convert.ToDouble(fmp.Roll);
                            RList.Add(roll);
                            // var hpRoll = new Cesium.HeadingPitchRoll(heading, pitch, roll);

                            // var orientation = Quaternion.CreateFromYawPitchRoll(heading, pitch, roll);


                        }
                        position.WriteCartographicDegrees(JList, pList);


                    }



                    // position.WriteCartographicDegrees(new Cartographic(lon, lat, 0.0));
                }

                // Now we generate some sample points and send them down.
                // Close the JSON array that wraps the entire CZML document.

                using (var entity = cesiumWriter.OpenPacket(output))
                {
                    entity.WriteId("Custom");
                    entity.WriteVersion("1.0");
                    entity.WriteAvailability(new TimeInterval(StartTime, EndTime));
                    var custom = entity.OpenPropertiesProperty();

                    CustomPropertyCesiumWriter newpro = custom.OpenCustomPropertyProperty("heading");

                    // newpro.Open(output);
                    newpro.AsNumber();


                    newpro.WriteNumber(JList, HList);
                    newpro.Close();

                    CustomPropertyCesiumWriter newroll = custom.OpenCustomPropertyProperty("roll");
                    //  newroll.Open(output);
                    newroll.AsNumber();

                    newroll.WriteNumber(JList, RList);
                    newroll.Close();
                    CustomPropertyCesiumWriter newpitch = custom.OpenCustomPropertyProperty("pitch");
                    // newpitch.Open(output);
                    newpitch.AsNumber();

                    newpitch.WriteNumber(JList, PList);
                    newpitch.Close();
                    custom.Close();
                }

                output.WriteEndSequence();

            //ms.Position = 0;

            //ms.Seek(0, SeekOrigin.Begin);
            outputStream.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/json");
           // }
        }

      
    }
            
    }
       
    


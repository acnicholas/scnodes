using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

public static class SunSettings
{    
    public static List<Revit.Elements.SunSettings> ByDateTimesAndInterval(
        Revit.Elements.Views.View view,
        DateTime startTime,
        DateTime endTime,
        TimeSpan interval)
    {
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        View apiView = doc.GetElement(view.InternalElement.Id) as View;
        
        var result = new List<Revit.Elements.SunSettings>();
        
        if ( startTime > endTime || interval.Minutes < 1){
            return null;
        }

        while (startTime <= endTime) {
            SunAndShadowSettings sunSettings = apiView.SunAndShadowSettings;
            sunSettings.StartDateAndTime = startTime.To;
            sunSettings.SunAndShadowType = SunAndShadowType.StillImage;
            var dynSunSetting = Revit.Elements.ElementWrapper.ToDSType(sunSettings, true) as Revit.Elements.SunSettings;
           
            result.Add(dynSunSetting);
            startTime = startTime.Add(interval);
        }
        return result;
    }
    
    public static DateTime DateTimeToLocal(int year, int month, int day, int hour) {
        return new DateTime(year, month, day, hour, 0, 0).ToLocalTime;
    }
}


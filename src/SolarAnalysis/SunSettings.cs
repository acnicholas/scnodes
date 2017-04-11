using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;

public static class SunSettings
{
    public static List<Revit.Elements.SunSettings> ByDateTime(
        DateTime startTime,
        DateTime endTime,
        TimeSpan timespan)
    {
        
        var result = new List<Revit.Elements.SunSettings>();
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        var view = doc.ActiveView;
        
        while (startTime < endTime) {
            
            RevitServices.Transactions.TransactionManager.Instance.EnsureInTransaction(doc);
            
            //using (SubTransaction st = new SubTransaction(doc)) {
            //    if (st.Start() == TransactionStatus.Started) {
                    view.SunAndShadowSettings.SunAndShadowType = SunAndShadowType.StillImage;
                    view.SunAndShadowSettings.StartDateAndTime = startTime;
           //         if (st.Commit() != TransactionStatus.Committed) {
           //             st.RollBack();
           //             return null;
           //         }
           //         doc.Regenerate();
            //    }
            //}
            
            RevitServices.Transactions.TransactionManager.Instance.ForceCloseTransaction();
            doc.Regenerate();
            result.Add(Revit.Elements.SunSettings.Current());
            startTime.Add(timespan);   
        }
        RevitServices.Transactions.TransactionManager.Instance.EnsureInTransaction(doc);
        return result;
    }
    
    public static DateTime DateTimeToLocal(int year, int month, int day, int hour)
    {
        return new DateTime(year, month, day, hour, 0, 0).ToLocalTime();
    }
}


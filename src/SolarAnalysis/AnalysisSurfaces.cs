using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;

public static class AnalysisSurfaces
{
    public static string CreateMultipleAnalysisSurfaces(
        List<Autodesk.DesignScript.Geometry.UV> uvs,
        List<double> samples,
        List<Autodesk.DesignScript.Geometry.Face> faces)
    {
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        Autodesk.Revit.UI.UIDocument uidoc = RevitServices.Persistence.DocumentManager.Instance.CurrentUIDocument;
        
        foreach (Autodesk.DesignScript.Geometry.Face f in faces) {
            //Autodesk.Revit.DB.Face revitFace = f.
            var test = f.
        }

        Autodesk.Revit.DB.Analysis.SpatialFieldManager sfm = Autodesk.Revit.DB.Analysis.SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
        if (null == sfm) {
            sfm = Autodesk.Revit.DB.Analysis.SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView, 1);
        }
        
        Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Face, "Select a face");
        int idx = sfm.AddSpatialFieldPrimitive(reference);

        Autodesk.Revit.DB.Face face = doc.GetElement(reference).GetGeometryObjectFromReference(reference) as Autodesk.Revit.DB.Face;

        List<Autodesk.Revit.DB.UV> uvPts = new List<Autodesk.Revit.DB.UV>();
        BoundingBoxUV bb = face.GetBoundingBox();
        Autodesk.Revit.DB.UV min = bb.Min;
        Autodesk.Revit.DB.UV max = bb.Max;
        uvPts.Add(new Autodesk.Revit.DB.UV(min.U, min.V));
        uvPts.Add(new Autodesk.Revit.DB.UV(max.U, max.V));

        var pnts = new Autodesk.Revit.DB.Analysis.FieldDomainPointsByUV(uvPts);

        List<double> doubleList = new List<double>();
        List<Autodesk.Revit.DB.Analysis.ValueAtPoint> valList = new List<Autodesk.Revit.DB.Analysis.ValueAtPoint>();
        doubleList.Add(0);
        valList.Add(new Autodesk.Revit.DB.Analysis.ValueAtPoint(doubleList));
        doubleList.Clear();
        doubleList.Add(10);
        valList.Add(new Autodesk.Revit.DB.Analysis.ValueAtPoint(doubleList));

        Autodesk.Revit.DB.Analysis.FieldValues vals = new Autodesk.Revit.DB.Analysis.FieldValues(valList);
        Autodesk.Revit.DB.Analysis.AnalysisResultSchema resultSchema = new Autodesk.Revit.DB.Analysis.AnalysisResultSchema("Schema Name", "Description");
        int schemaIndex = sfm.RegisterResult(resultSchema);
        sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);

        return "Action time";
    }
}
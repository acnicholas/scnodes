using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

public static class AnalysisSurfaces
{
    public static void CreateMultipleAnalysisSurfaces(
        List<List<Autodesk.DesignScript.Geometry.UV>> uvs,
        List<List<double>> samples,
        List<Autodesk.DesignScript.Geometry.Face> dynFaces)
    {
        for (int i = 0; i < dynFaces.Count; i++) {
            CreateAnalysisSurface(uvs[i], samples[i], dynFaces[0], "name" + i.ToString(), "description"+ i.ToString());
        }
    }

    
    
    public static void CreateAnalysisSurfaceTest(
        Autodesk.DesignScript.Geometry.Face dynFace,
        string name,
        string description)
    {
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        Autodesk.Revit.UI.UIDocument uidoc = RevitServices.Persistence.DocumentManager.Instance.CurrentUIDocument;
        
        SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
        if (sfm == null){
            sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView,1);
        }
      

        Reference faceRef = dynFace.Tags.LookupTag("RevitFaceReference") as Reference;
        int idx = sfm.AddSpatialFieldPrimitive(faceRef);

        Face face = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;

        IList<UV> uvPts = new List<UV>();
        BoundingBoxUV bb = face.GetBoundingBox();
        UV min = bb.Min;
        UV max = bb.Max;
        uvPts.Add(new UV(min.U, min.V));
        uvPts.Add(new UV(max.U, max.V));

        FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);

        List<double> doubleList = new List<double>();
        IList<ValueAtPoint> valList = new List<ValueAtPoint>();
        doubleList.Add(0);
        valList.Add(new ValueAtPoint(doubleList));
        doubleList.Clear();
        doubleList.Add(10);
        valList.Add(new ValueAtPoint(doubleList));

        FieldValues vals = new FieldValues(valList);
        AnalysisResultSchema resultSchema = new AnalysisResultSchema(name, description);
        int schemaIndex = sfm.RegisterResult(resultSchema);
        sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);
        
    }
    
    
    
    
         public static void CreateAnalysisSurface(
        List<Autodesk.DesignScript.Geometry.Point> points,
        List<double> samples,
        Autodesk.DesignScript.Geometry.Face dynFace,
        string name,
        string description)
    {
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        Autodesk.Revit.UI.UIDocument uidoc = RevitServices.Persistence.DocumentManager.Instance.CurrentUIDocument;
        
        SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
        if (sfm == null){
            sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView,1);
        }
      
        Reference faceRef = dynFace.Tags.LookupTag("RevitFaceReference") as Reference;
        int idx = sfm.AddSpatialFieldPrimitive(faceRef);

        Face face = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;

        BoundingBoxUV bb = face.GetBoundingBox();
        IList<UV> uvPts = CreateUVPoints(points, face);
        FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);

        List<ValueAtPoint> valList = new List<ValueAtPoint>();
        foreach (double sample in samples) {
            List<double> doubleList = new List<double>();
            doubleList.Add(sample);
            valList.Add(new ValueAtPoint(doubleList));
            doubleList.Clear();
        }
        
        FieldValues vals = new FieldValues(valList);
        AnalysisResultSchema resultSchema = new AnalysisResultSchema(name, description);
        int schemaIndex = sfm.RegisterResult(resultSchema);
        sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);
        
    }
    
     private static IList<UV> CreateUVPoints(List<Autodesk.DesignScript.Geometry.Point> points, Face face)
    {
         var result = new List<UV>();
        foreach (Autodesk.DesignScript.Geometry.Point point in points) {
            XYZ p = new XYZ(point.X, point.Y, point.Z);
            result.Add(face.Project(p).UVPoint);
        }
        return result;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
     public static void CreateAnalysisSurface(
        List<Autodesk.DesignScript.Geometry.UV> uvs,
        List<double> samples,
        Autodesk.DesignScript.Geometry.Face dynFace,
        string name,
        string description)
    {
        Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
        Autodesk.Revit.UI.UIDocument uidoc = RevitServices.Persistence.DocumentManager.Instance.CurrentUIDocument;
        
        SpatialFieldManager sfm = SpatialFieldManager.GetSpatialFieldManager(doc.ActiveView);
        if (sfm == null){
            sfm = SpatialFieldManager.CreateSpatialFieldManager(doc.ActiveView,1);
        }
      
        Reference faceRef = dynFace.Tags.LookupTag("RevitFaceReference") as Reference;
        int idx = sfm.AddSpatialFieldPrimitive(faceRef);

        Face face = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef) as Face;

        BoundingBoxUV bb = face.GetBoundingBox();
        IList<UV> uvPts = CreateUVPoints(uvs, bb);
        FieldDomainPointsByUV pnts = new FieldDomainPointsByUV(uvPts);

        List<ValueAtPoint> valList = new List<ValueAtPoint>();
        foreach (double sample in samples) {
            List<double> doubleList = new List<double>();
            doubleList.Add(sample);
            valList.Add(new ValueAtPoint(doubleList));
            doubleList.Clear();
        }
        
        FieldValues vals = new FieldValues(valList);
        AnalysisResultSchema resultSchema = new AnalysisResultSchema(name, description);
        int schemaIndex = sfm.RegisterResult(resultSchema);
        sfm.UpdateSpatialFieldPrimitive(idx, pnts, vals, schemaIndex);
        
    }
     
    private static IList<UV> CreateUVPoints(List<Autodesk.DesignScript.Geometry.UV> uvs, BoundingBoxUV bb)
    {
        var result = new List<UV>();
        UV min = bb.Min;
        UV max = bb.Max;
        double umult = bb.Max.U - bb.Min.U;
        double vmult = bb.Max.V - bb.Min.V;
        
        foreach (Autodesk.DesignScript.Geometry.UV dynUV in uvs) {
            // result.Add(new UV(bb.Min.U + dynUV.U * umult, bb.Min.V + dynUV.V * vmult));
            result.Add(new UV(dynUV.U * umult, dynUV.V * vmult));
        }
        
        return result;
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}
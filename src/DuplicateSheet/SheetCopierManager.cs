namespace SheetCopier
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System;
    using System.Collections.Generic;
    using Autodesk.DesignScript.Runtime;
    
    public class SheetCopierManager
    {
        private Document doc;
        private Dictionary<string, View> existingSheets =
            new Dictionary<string, View>();
        
        private Dictionary<string, View> existingViews =
            new Dictionary<string, View>();
        
        private Dictionary<string, View> viewTemplates =
            new Dictionary<string, View>();
        
        private Dictionary<string, Level> levels =
            new Dictionary<string, Level>();
        
        private Collection<string> sheetCategories = 
            new Collection<string>();
        
        private List<Revision> hiddenRevisionClouds = new List<Revision>();
        private ElementId floorPlanViewFamilyTypeId = null;
        
        private List<SheetCopierSheet> sheets;
           
        private SheetCopierManager(Document doc)
        {
            this.doc = doc;
            this.sheets = new List<SheetCopierSheet>();
            this.hiddenRevisionClouds = GetAllHiddenRevisions(this.doc);
            this.GetViewTemplates();
            GetAllSheets(existingSheets, doc);
            this.GetAllLevelsInModel();
            GetAllViewsInModel(existingViews, doc);
            this.GetFloorPlanViewFamilyTypeId();
            this.GetAllSheetCategories();
        }
        
        /// <summary>
        /// Simple duplicate sheet node.
        /// New sheets and views will be automagically renamed.
        /// and detialing will be copied.
        /// </summary>
        /// <param name="sourceSheet">Revit sheet</param>
        /// <returns>summary text</returns>
        public static string DuplicateSheetSimple(Revit.Elements.Views.Sheet sourceSheet)
        { 
            StringBuilder result = new StringBuilder();
            Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
            ViewSheet vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
            SheetCopierManager scopy = new SheetCopierManager(doc);
            scopy.AddSheet(vs, true);
            SheetCopier.SheetCopierManager.CreateSheets(scopy, ref result);
            return result.ToString();
        }
        
        
        /// <summary>
        /// Advanced duplicate sheet node.
        /// </summary>
        /// <param name="sourceSheet">Revit sheet</param>
        /// <param name="duplicateWithDetailing">Set/Unset whether views on copoed sheets will have detailing copied</param>
        /// <param name="ViewTemplateName">Name of viewtemplate to assign to new views.</param>
        /// <param name="level">Level to associate with new plan views</param>
        /// <returns>summary text</returns>
        [MultiReturn(new[] { "summary", "sheets" })]
        public static Dictionary<string, object> DuplicateSheetAdvanced(
            Revit.Elements.Views.Sheet sourceSheet,
            bool duplicateWithDetailing,
            string ViewTemplateName,
            Revit.Elements.Level level)
        { 
            StringBuilder summary = new StringBuilder();
            Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
            ViewSheet vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
            SheetCopierManager scopy = new SheetCopierManager(doc);
            scopy.AddSheet(vs, duplicateWithDetailing);
            var result = SheetCopierManager.CreateSheets(scopy, ref summary);
            return new Dictionary<string, object>{
                { "summary", summary},{ "sheets",  result}
            };
        }
        
        private static SheetCopierManager ByValues(Document doc)
        {
            return new SheetCopierManager(doc);
        }
               
        #region properties

        private List<SheetCopierSheet> Sheets {
            get {
                return this.sheets;
            }
        }

        private Dictionary<string, View> ViewTemplates {
            get {
                return this.viewTemplates;
            }
        }

        private Dictionary<string, Level> Levels {
            get {
                return this.levels;
            }
        }
    
        private Dictionary<string, View> ExistingViews {
            get {
                return this.existingViews;
            }
        }
        
        private Collection<string> SheetCategories {
            get {
                return this.sheetCategories;
            }    
        }
    
        #endregion

        #region public methods

        private static ViewSheet ViewToViewSheet(View view)
        {
            return (view.ViewType != ViewType.DrawingSheet) ? null : view as ViewSheet;
        }
                  
        internal bool SheetNumberAvailable(string number)
        {
            foreach (SheetCopierSheet s in this.sheets) {
                if (s.Number.ToUpper(CultureInfo.InvariantCulture).Equals(number.ToUpper(CultureInfo.InvariantCulture))) {
                    return false;
                }
            }
            return !this.existingSheets.ContainsKey(number);
        }

        internal static bool ViewNameAvailable(string title, SheetCopierManager manager)
        {
            foreach (SheetCopierSheet s in manager.sheets) {
                foreach (SheetCopierViewOnSheet v in s.ViewsOnSheet) {
                    if (v.Title.ToUpper(CultureInfo.InvariantCulture).Equals(title.ToUpper(CultureInfo.InvariantCulture))) {
                        return false;
                    }
                }
            }
            return !manager.existingViews.ContainsKey(title);
        }

        internal static List<Revit.Elements.Views.Sheet> CreateSheets(SheetCopierManager manager, ref StringBuilder summary)
        { 
            var result = new List<Revit.Elements.Views.Sheet>();
            if (manager.sheets.Count < 1) {
                return null;
            }
            foreach (SheetCopierSheet sheet in manager.sheets) {
                result.Add(manager.CreateAndPopulateNewSheet(sheet, ref summary));
            }
            return result;
        }
    
        private void AddSheet(ViewSheet sourceSheet, bool duplicateWithDetailing)
        {
            string n = this.GetNewSheetNumber(sourceSheet.SheetNumber);
            string t = sourceSheet.Name + SheetCopierConstants.MenuItemCopy;
            this.sheets.Add(SheetCopierSheet.ByValues(n, t, this, sourceSheet, duplicateWithDetailing));
        }   
      
        #endregion

        #region private methods
                              
        private static Dictionary<ElementId, XYZ> GetVPDictionary(
            ViewSheet srcSheet, Document doc)
        {
            var result = new Dictionary<ElementId, XYZ>();
            foreach (ElementId viewPortId in srcSheet.GetAllViewports()) {
                var viewPort = (Viewport)doc.GetElement(viewPortId);
                var viewPortCentre = viewPort.GetBoxCenter();
                result.Add(
                    viewPort.ViewId, viewPortCentre);
            }
            return result;
        }

        private void GetViewTemplates()
        {
            this.viewTemplates.Clear();
            var c = new FilteredElementCollector(this.doc).OfCategory(BuiltInCategory.OST_Views);
            foreach (View view in c) {
                if (view.IsTemplate) {
                    this.viewTemplates.Add(view.Name, view);
                }
            }
        }
          
        private void GetAllSheetCategories()
        {
            this.sheetCategories.Clear();
            var c1 = new FilteredElementCollector(this.doc).OfCategory(BuiltInCategory.OST_Sheets);
            foreach (View view in c1) {
                var viewCategoryParamList = view.GetParameters(SheetCopierConstants.SheetCategory);
                if (viewCategoryParamList != null && viewCategoryParamList.Count > 0) {
                    Parameter viewCategoryParam = viewCategoryParamList.First();
                    string s = viewCategoryParam.AsString();
                    if (!string.IsNullOrEmpty(s) && !this.sheetCategories.Contains(s)) {
                        this.sheetCategories.Add(s);
                    }
                } 
            }
        }

        private static void GetAllSheets(Dictionary<string, View> existingSheets, Document doc)
        {
            existingSheets.Clear();
            var c1 = new FilteredElementCollector(doc);
            c1.OfCategory(BuiltInCategory.OST_Sheets);
            foreach (View view in c1) {
                var vs = view as ViewSheet;
                existingSheets.Add(vs.SheetNumber, view);
            }
        }

        private void GetFloorPlanViewFamilyTypeId()
        {
            foreach (ViewFamilyType vft in new FilteredElementCollector(this.doc).OfClass(typeof(ViewFamilyType))) {
                if (vft.ViewFamily == ViewFamily.FloorPlan) {
                    this.floorPlanViewFamilyTypeId = vft.Id;
                }
            }
        }
        
        private static void GetAllViewsInModel(Dictionary<string, View> existingViews, Document doc)
        {
            existingViews.Clear();
            FilteredElementCollector c = new FilteredElementCollector(doc).OfClass(typeof(Autodesk.Revit.DB.View));
            foreach (Element element in c) {
                var view = element as View;
                View tmpView;
                if (!existingViews.TryGetValue(view.Name, out tmpView)) {
                    existingViews.Add(view.Name, view);
                }
            }
        }

        private void GetAllLevelsInModel()
        {
            this.levels.Clear();
            var c3 = new FilteredElementCollector(this.doc).OfClass(typeof(Level));
            foreach (Element element in c3) {
                this.levels.Add(element.Name.ToString(), element as Level);
            }
        }

        // this is where the action happens
        private Revit.Elements.Views.Sheet CreateAndPopulateNewSheet(SheetCopierSheet sheet, ref StringBuilder summary)
        { 
           
            //turn on hidden revisions
            foreach (Revision rev in this.hiddenRevisionClouds) {
                rev.Visibility = RevisionVisibility.CloudAndTagVisible;
            }
            
            sheet.DestinationSheet = this.AddEmptySheetToDocument(
                sheet.Number,
                sheet.Title,
                sheet.SheetCategory);
 
            if (sheet.DestinationSheet != null) {
                Debug.WriteLine(sheet.Number + " added to document.");
                this.CreateViewports(sheet);
            } else {
                summary.Append("Could not create destination sheet");
                return null;
            }
            
            try {
                this.CopyElementsBetweenSheets(sheet);
            } catch (InvalidOperationException e) {
                Debug.WriteLine(e.Message);
            } 
            
            foreach (Revision rev in this.hiddenRevisionClouds) {
                rev.Visibility = RevisionVisibility.Hidden;
            }

            var oldNumber = sheet.SourceSheet.SheetNumber;
            var msg = " Sheet: " + oldNumber + " copied to: " + sheet.Number;
            summary.Append(msg + System.Environment.NewLine);
            var v = Revit.Elements.ElementWrapper.ToDSType(sheet.DestinationSheet, true);
            return v as Revit.Elements.Views.Sheet;
        }
    
        // add an empty sheet to the doc.
        // this comes first before copying titleblock, views etc.
        private ViewSheet AddEmptySheetToDocument(
            string sheetNumber,
            string sheetTitle,
            string viewCategory)
        {
            ViewSheet result;
            result = ViewSheet.Create(this.doc, ElementId.InvalidElementId);           
            result.Name = sheetTitle;
            result.SheetNumber = sheetNumber;
            var viewCategoryParamList = result.GetParameters(SheetCopierConstants.SheetCategory);
            if (viewCategoryParamList.Count > 0) {
                Parameter viewCategoryParam = viewCategoryParamList.First();
                viewCategoryParam.Set(viewCategory);
            }
            return result;
        }
        
        private void PlaceViewPortOnSheet(
            Element destSheet, ElementId destViewId, XYZ viewCentre)
        {
            Viewport.Create(this.doc, destSheet.Id, destViewId, viewCentre);
        }

        private string GetNewSheetNumber(string originalNumber)
        {
            int inc = 0;
            do {
                inc++;
            } while (!this.SheetNumberAvailable(originalNumber + "-" + inc.ToString(CultureInfo.InvariantCulture)));
            return originalNumber + "-" + inc.ToString(CultureInfo.InvariantCulture);
        }
        
        private void TryAssignViewTemplate(View view, string templateName)
        {
            if (templateName != SheetCopierConstants.MenuItemCopy) {
                View vt = null;
                if (this.viewTemplates.TryGetValue(templateName, out vt)) {
                    view.ViewTemplateId = vt.Id;
                }
            }   
        }
               
        private void PlaceNewViewOnSheet(
            SheetCopierViewOnSheet view,
            SheetCopierSheet sheet,
            XYZ sourceViewCentre)
        {
            Level level = null;
            this.levels.TryGetValue(view.AssociatedLevelName, out level);
            if (level != null) {
                ViewPlan vp = ViewPlan.Create(this.doc, this.floorPlanViewFamilyTypeId, level.Id);
                vp.CropBox = view.OldView.CropBox;
                vp.CropBoxActive = view.OldView.CropBoxActive;
                vp.CropBoxVisible = view.OldView.CropBoxVisible;
                this.TryAssignViewTemplate(vp, view.ViewTemplateName);
                this.PlaceViewPortOnSheet(sheet.DestinationSheet, vp.Id, sourceViewCentre);
            }
        }
        
        private static List<Revision> GetAllHiddenRevisions(Document doc)
        {
            var revisions = new List<Revision>();
            FilteredElementCollector collector = new FilteredElementCollector(doc);    
            collector.OfCategory(BuiltInCategory.OST_Revisions);
            foreach (Element e in collector) {
                Revision rev = e as Revision;
                if (rev.Visibility == RevisionVisibility.Hidden) {
                    revisions.Add(rev);
                }
            }
            return revisions;
        }
        
        private static void DeleteRevisionClouds(ElementId viewId, Document doc)
        {            
            FilteredElementCollector collector = new FilteredElementCollector(doc, viewId);
            collector.OfCategory(BuiltInCategory.OST_RevisionClouds);
            var clouds = new List<ElementId>();
            var issuedClouds = new List<Revision>();
            foreach (Element e in collector) {
                var cloud = e as RevisionCloud;
                var revisionId = cloud.RevisionId;
                var revision = doc.GetElement(revisionId) as Revision;
                if(revision.Issued) {
                    revision.Issued = false;
                    issuedClouds.Add(revision);
                }
                clouds.Add(e.Id);
            }
            doc.Delete(clouds);  
            foreach (Revision r in issuedClouds) {
                r.Issued = true;
            }
        }
        
        private void DuplicateViewOntoSheet(
            SheetCopierViewOnSheet view,
            SheetCopierSheet sheet,
            XYZ sourceViewCentre)
        {
            var d = view.DuplicateWithDetailing == true ? ViewDuplicateOption.WithDetailing : ViewDuplicateOption.Duplicate;          
            ElementId destViewId = view.OldView.Duplicate(d);
            DeleteRevisionClouds(destViewId, this.doc);
            string newName = SheetCopierSheet.GetNewViewName(view.OldView.Id, sheet);
            var v = this.doc.GetElement(destViewId) as View;
            if (newName != null) {
                v.Name = newName;
                var dv = this.doc.GetElement(destViewId) as View;  
                this.TryAssignViewTemplate(dv, view.ViewTemplateName);                
            }
            this.PlaceViewPortOnSheet(sheet.DestinationSheet, destViewId, sourceViewCentre);
        }

        private void CopyElementsBetweenSheets(SheetCopierSheet sheet)
        {
            IList<ElementId> list = new List<ElementId>();
            foreach (Element e in new FilteredElementCollector(this.doc).OwnedByView(sheet.SourceSheet.Id)) {
                if (!(e is Viewport)) {
                    Debug.WriteLine("adding " + e.GetType().ToString() + " to copy list(CopyElementsBetweenSheets).");
                    if (e is CurveElement) {
                        continue;
                    }
                    if (e.IsValidObject && e.ViewSpecific) {
                        list.Add(e.Id);
                    }
                }
            }             
            if (list.Count > 0) {
                Debug.WriteLine("Beggining element copy");
                ElementTransformUtils.CopyElements(
                    sheet.SourceSheet,
                    list,
                    sheet.DestinationSheet,
                    new Transform(ElementTransformUtils.GetTransformFromViewToView(sheet.SourceSheet, sheet.DestinationSheet)),
                    new CopyPasteOptions());
                DeleteRevisionClouds(sheet.DestinationSheet.Id, this.doc);
            }
        }
             
        private void CreateViewports(SheetCopierSheet sheet)
        {
            Dictionary<ElementId, XYZ> viewPorts =
                SheetCopierManager.GetVPDictionary(sheet.SourceSheet, this.doc);

            foreach (SheetCopierViewOnSheet view in sheet.ViewsOnSheet) {
                XYZ sourceViewPortCentre = null;
                if (!viewPorts.TryGetValue(view.OldId, out sourceViewPortCentre)) {
                    TaskDialog.Show("SCopy", "Error...");
                    continue;
                }
                             
                switch (view.CreationMode) {
                    case ViewPortPlacementMode.Copy:
                        this.DuplicateViewOntoSheet(view, sheet, sourceViewPortCentre);
                        break;
                    case ViewPortPlacementMode.New:
                        this.PlaceNewViewOnSheet(view, sheet, sourceViewPortCentre);
                        break;     
                    case ViewPortPlacementMode.Legend:
                        this.PlaceViewPortOnSheet(sheet.DestinationSheet, view.OldView.Id, sourceViewPortCentre);
                        break;                 
                }
            }       
        }
        #endregion
    }
}
/* vim: set ts=4 sw=4 nu expandtab: */

namespace SheetCopier
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    
    public class SheetCopierSheet
    {  
        private SheetCopierManager scopy;
        private string title;
        private string number;
        private string sheetCategory;            
        private BindingList<SheetCopierViewOnSheet> viewsOnSheet;
        
        private SheetCopierSheet(string number, string title,  SheetCopierManager scopy, ViewSheet sourceSheet, bool duplicateWithDetailing)
        {
            this.scopy = scopy;
            this.number = number;
            this.title = title; 
            this.SourceSheet = sourceSheet;
            this.sheetCategory = this.GetSheetCategory(SheetCopierConstants.SheetCategory);
            this.DestinationSheet = null;
            this.viewsOnSheet = new BindingList<SheetCopierViewOnSheet>();
             foreach (ElementId id in sourceSheet.GetAllPlacedViews()) {              
                Element element = sourceSheet.Document.GetElement(id);
                if (element != null) {
                    var v = element as View;
                    var vos = SheetCopierViewOnSheet.ByValues(v.Name, v, scopy);
                    vos.DuplicateWithDetailing = duplicateWithDetailing;
                    this.viewsOnSheet.Add(vos);
                }
            }
        }
        
        internal static SheetCopierSheet ByValues(
           string number,
           string title,
           SheetCopierManager scopy,
           ViewSheet sourceSheet)
        {      
            Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
            ViewSheet vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
            return new SheetCopierSheet(number, title, scopy, vs, true);
        }
        
        //ViewPortPlacementMode placementMode
        internal static SheetCopierSheet ByValues(
           string number,
           string title,
           SheetCopierManager scopy,
           ViewSheet sourceSheet,
           bool duplicateWithDetailing)
        {      
            Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
            ViewSheet vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
            return new SheetCopierSheet(number, title, scopy, vs, duplicateWithDetailing);
        }
        
               
        internal ViewSheet DestinationSheet {
            get; set;    
        }
                
        internal ViewSheet SourceSheet {
            get; set;    
        }
        
        internal string Number {
            get {
                return this.number;
            }
            
            set {
                if (value != this.number && this.scopy.SheetNumberAvailable(value)) {
                    this.number = value;
                } else {
                    Autodesk.Revit.UI.TaskDialog.Show(
                        "SCopy - WARNING", value + " exists, you can't use it!.");
                }
            }
        }

        internal string Title {
            get {
                return this.title;
            }
            
            set {
                this.title = value;
            }
        }
        
        internal string SheetCategory {
            get {
                return this.sheetCategory;
            }
            
            set {
                this.sheetCategory = value;
            }
        }

        internal BindingList<SheetCopierViewOnSheet> ViewsOnSheet {
            get {
                return this.viewsOnSheet;
            }
        }
        
        internal static string GetNewViewName(ElementId id, SheetCopierSheet sheet)
        {
            foreach (SheetCopierViewOnSheet v in sheet.viewsOnSheet) {
                if (id == v.OldId) {
                    return v.Title;
                }
            }
            return null;
        }
        
        private string GetSheetCategory(string parameterName)
        {
            var viewCategoryParamList = this.SourceSheet.GetParameters(parameterName);
            if (viewCategoryParamList != null && viewCategoryParamList.Count > 0) {
                Parameter viewCategoryParam = viewCategoryParamList.First();
                string s = viewCategoryParam.AsString();
                return s;
            } 
            return @"n/a";
        }               
    }
}
/* vim: set ts=4 sw=4 nu expandtab: */

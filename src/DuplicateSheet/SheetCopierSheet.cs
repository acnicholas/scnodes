//
//  SheetCopierSheet.cs
//
//  Author:
//       Andrew Nicholas<andrewnicholas@iinet.net.au>
//
//  Copyright (c) 2016 Andrew Nicholas
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace SheetCopier
{
    using System.Collections.Generic;
    using System.ComponentModel;
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
                    if (v != null) {
                        var vos = SheetCopierViewOnSheet.ByValues(v.Name, v, scopy);
                        vos.DuplicateWithDetailing = duplicateWithDetailing;
                        this.viewsOnSheet.Add(vos);
                    }
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
            var vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
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
            var vs = doc.GetElement(sourceSheet.UniqueId) as ViewSheet;
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
                return number;
            }
            
            set {
                if (value != number && scopy.SheetNumberAvailable(value)) {
                    number = value;
                } else {
                    Autodesk.Revit.UI.TaskDialog.Show(
                        "SCopy - WARNING", value + " exists, you can't use it!.");
                }
            }
        }

        internal string Title {
            get {
                return title;
            }
            
            set {
                title = value;
            }
        }
        
        internal string SheetCategory {
            get {
                return sheetCategory;
            }
            
            set {
                sheetCategory = value;
            }
        }

        internal BindingList<SheetCopierViewOnSheet> ViewsOnSheet {
            get {
                return viewsOnSheet;
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
            var viewCategoryParamList = SourceSheet.GetParameters(parameterName);
            if (viewCategoryParamList != null && viewCategoryParamList.Count > 0) {
                //Parameter viewCategoryParam = viewCategoryParamList.First();
                Parameter viewCategoryParam = viewCategoryParamList[0];
                string s = viewCategoryParam.AsString();
                return s;
            } 
            return @"n/a";
        }               
    }
}
/* vim: set ts=4 sw=4 nu expandtab: */

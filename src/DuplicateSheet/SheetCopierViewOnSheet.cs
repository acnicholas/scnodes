namespace SheetCopier
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using Autodesk.Revit.DB;

    public partial class SheetCopierViewOnSheet
    {
        private SheetCopierManager scopy;
        private string originalTitle;
        private string newTitle;
        private string associatedLevelName;
        private View oldView;
        private string viewTemplateName;
        private ElementId oldId;
        private bool duplicateWithDetailing;
        private ViewPortPlacementMode creationMode;
   
        private SheetCopierViewOnSheet(string title, View view, SheetCopierManager scopy)
        {
            this.scopy = scopy;
            this.oldView = view;
            this.oldId = view.Id;
            this.originalTitle = title;
            this.SetDefualtCreationMode();
            this.newTitle =
                title + @"(" + (DateTime.Now.TimeOfDay.Ticks / 100000).ToString(CultureInfo.InvariantCulture) + @")";
            this.associatedLevelName = SheetCopierConstants.MenuItemCopy;
            this.viewTemplateName = SheetCopierConstants.MenuItemCopy;
            this.duplicateWithDetailing = true;
        }
        
        internal static SheetCopierViewOnSheet ByValues(string title, View view, SheetCopierManager scopy)
        {
            return new SheetCopierViewOnSheet(title, view, scopy);
        }
            
        internal ViewPortPlacementMode CreationMode {
            get {
                return this.creationMode;
            }
        }

        internal ElementId OldId {
            get {
                return this.oldId;
            }
        }

        internal View OldView {
            get {
                return this.oldView;
            }
        }

        internal ViewType RevitViewType {
            get {
                return this.oldView.ViewType;
            }
        }

        internal string ViewTemplateName {
            get {
                return this.viewTemplateName;
            }
            
            set {
                this.viewTemplateName = value;
            }
        }

        internal string AssociatedLevelName {
            get {
                return this.associatedLevelName;
            }
            
            set {
                this.associatedLevelName = value;
                if (value != SheetCopierConstants.MenuItemCopy) {
                    this.DuplicateWithDetailing = false;
                    this.creationMode = ViewPortPlacementMode.New;
                } else {
                    this.creationMode = ViewPortPlacementMode.Copy;   
                }
            }
        }

        internal string Title {
            get {
                return this.newTitle;
            }
            
            set {
                if (value != this.newTitle && SheetCopierManager.ViewNameAvailable(value,scopy)) {
                    this.newTitle = value;
                } else {
                    Autodesk.Revit.UI.TaskDialog.Show(
                        "SCopy - WARNING", value + " exists, you can't use it!.");
                }
            }
        }

        internal string OriginalTitle {
            get {
                return this.originalTitle;
            }
        }
        
        internal bool DuplicateWithDetailing {
            get {
                return this.duplicateWithDetailing;
            }
            
            set {
                this.duplicateWithDetailing = value;
            }
        }
        
        private static bool PlanEnough(ViewType vt)
        {
            switch (vt) {
                case ViewType.FloorPlan:
                case ViewType.CeilingPlan:
                case ViewType.AreaPlan:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Is the view "Plan Enough" for you...
        /// i.e. can it be assigned to a level
        /// </summary>
        /// <returns></returns>
        internal bool PlanEnough()
        {
            return PlanEnough(this.RevitViewType);
        }
        
        private void SetDefualtCreationMode()
        {
            this.creationMode = this.oldView.ViewType == ViewType.Legend ? ViewPortPlacementMode.Legend : ViewPortPlacementMode.Copy;
        }  
    }
}
/* vim: set ts=4 sw=4 nu expandtab: */

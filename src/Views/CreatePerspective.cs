//
//  CreatePerspective
//
//  Author:
//       Andrew Nicholas<andrewnicholas@iinet.net.au>
//
//  Copyright (c) 2017 Andrew Nicholas
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
//

using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

public static class CreatePerspective
{

	public static Revit.Elements.Views.View CreatePerspectiveByView(Revit.Elements.Views.View view, string viewName)
	{
		Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
		UIDocument uidoc = RevitServices.Persistence.DocumentManager.Instance.CurrentUIDocument;
		View currentView = doc.ActiveView;
		Revit.Elements.Views.View result = null;

		switch (currentView.ViewType) {
			case ViewType.ThreeD:
				result = CreatePerspectiveFrom3D(uidoc, currentView as View3D);
				break;
			case ViewType.FloorPlan:
				result =CreatePerspectiveFromPlan(uidoc, currentView);
				break;
			case ViewType.CeilingPlan:
				break;
			case ViewType.Section:
				result = CreatePerspectiveFromSection(uidoc, currentView);
				break;
			default:
				//TaskDialog td = new TaskDialog("SCam - SC Camera Tool");
				//td.MainInstruction = "Oops!";
				//td.MainContent = "Currently cameras can only be created in 3d and Plan views" +
				//    System.Environment.NewLine +
				//    "Please create sections/elevations from an isometric view";
				//td.Show();
				break;
		}
		return result;
	}

	private static UIView ActiveUIView(UIDocument udoc, Element planView)
	{
		foreach (UIView view in udoc.GetOpenUIViews()) {
			View v = (View)udoc.Document.GetElement(view.ViewId);
			if (v.Name == planView.Name) {
				return view;
			}
		}
		return null;   
	}

	private static XYZ GetMiddleOfActiveViewWindow(UIView view)
	{ 
		if (view == null) {
			return new XYZ();
		}
		XYZ topLeft = view.GetZoomCorners()[0];
		XYZ bottomRight = view.GetZoomCorners()[1];
		double width = bottomRight.X - topLeft.X;
		double height = bottomRight.Y - topLeft.Y;
		double middleX = bottomRight.X - (width / 2);
		double middleY = bottomRight.Y - (height / 2);
		double eyeHeight = height > width ? (height * 1.5) : width;
		return new XYZ(middleX, middleY, eyeHeight);
	}  

	private static BoundingBoxXYZ ViewExtentsBoundingBox(UIView view)
	{
		if (view == null) {
			return new BoundingBoxXYZ();
		}
		BoundingBoxXYZ result = new BoundingBoxXYZ();
		XYZ min = new XYZ(view.GetZoomCorners()[0].X, view.GetZoomCorners()[0].Y, view.GetZoomCorners()[0].Z - 4);
		XYZ max = new XYZ(view.GetZoomCorners()[1].X, view.GetZoomCorners()[1].Y, view.GetZoomCorners()[1].Z + 4);
		result.set_Bounds(0, min);
		result.set_Bounds(1, max);
		return result;
	}

	private static BoundingBoxXYZ SectionViewExtentsBoundingBox(UIView view)
	{
		if (view == null) {
			return new BoundingBoxXYZ();
		}
		BoundingBoxXYZ result = new BoundingBoxXYZ();
		XYZ min = new XYZ(view.GetZoomCorners()[0].X, view.GetZoomCorners()[0].Y, view.GetZoomCorners()[0].Z - 4);
		XYZ max = new XYZ(view.GetZoomCorners()[1].X, view.GetZoomCorners()[1].Y, view.GetZoomCorners()[1].Z + 4);
		result.set_Bounds(0, min);
		result.set_Bounds(1, max);
		return result;
	}

	private static Revit.Elements.Views.View APIViewAsDynView(View view)
	{
		var result = Revit.Elements.ElementWrapper.ToDSType(view, true);
		return result as Revit.Elements.Views.View;
	}

	private static void ApplySectionBoxToView(BoundingBoxXYZ bounds, View3D view)
	{
		view.SetSectionBox(bounds);
	}

	private static Revit.Elements.Views.View CreatePerspectiveFromPlan(UIDocument udoc, View planView)
	{
		UIView view = ActiveUIView(udoc, planView);
		XYZ eye = GetMiddleOfActiveViewWindow(view);            
		XYZ up = new XYZ(0, 1, 0);
		XYZ forward = new XYZ(0, 0, -1);
		ViewOrientation3D v = new ViewOrientation3D(eye, up, forward);
		View3D np = View3D.CreatePerspective(udoc.Document, Get3DViewFamilyTypes(udoc.Document).First().Id);
		np.SetOrientation(new ViewOrientation3D(v.EyePosition, v.UpDirection, v.ForwardDirection));
		ApplySectionBoxToView(ViewExtentsBoundingBox(view), np);
		return APIViewAsDynView(np);
	}

	private static Revit.Elements.Views.View CreatePerspectiveFromSection(UIDocument udoc, View sectionView)
	{
		UIView view = ActiveUIView(udoc, sectionView);
		XYZ eye = GetMiddleOfActiveViewWindow(view);            
		XYZ up = new XYZ(0, 0, 1);
		XYZ forward = new XYZ(0, 0, -1);
		ViewOrientation3D v = new ViewOrientation3D(eye, up, forward);
		View3D np = View3D.CreatePerspective(udoc.Document, Get3DViewFamilyTypes(udoc.Document).First().Id);
		np.SetOrientation(new ViewOrientation3D(v.EyePosition, v.UpDirection, v.ForwardDirection));
		ApplySectionBoxToView(SectionViewExtentsBoundingBox(view), np);
		return APIViewAsDynView(np);
	}

	private static IEnumerable<ViewFamilyType> Get3DViewFamilyTypes(Document doc)
	{
		IEnumerable<ViewFamilyType> viewFamilyTypes
			= from elem in new FilteredElementCollector(doc)
			.OfClass(typeof(ViewFamilyType))
			let type = elem as ViewFamilyType
			where type.ViewFamily == ViewFamily.ThreeDimensional
			select type;   
		return viewFamilyTypes;
	}

	private static Revit.Elements.Views.View CreatePerspectiveFrom3D(UIDocument udoc, View3D view)
	{
		ViewOrientation3D v = view.GetOrientation();
		XYZ centreOfScreen = GetMiddleOfActiveViewWindow(ActiveUIView(udoc, (View)view));
		View3D np = View3D.CreatePerspective(udoc.Document, Get3DViewFamilyTypes(udoc.Document).First().Id);
		np.SetOrientation(new ViewOrientation3D(new XYZ(centreOfScreen.X, centreOfScreen.Y, v.EyePosition.Z), v.UpDirection, v.ForwardDirection));
		return APIViewAsDynView(np);
	}
}
/* vim: set ts=4 sw=4 nu expandtab: */

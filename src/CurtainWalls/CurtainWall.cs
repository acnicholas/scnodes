//
//  CurtainWall.cs
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

using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;

/// <summary>
/// Nodes for curtain walls.
/// </summary>
public static class CurtainWall
{ 
	public static void DeleteUGrids(Revit.Elements.Wall dynCurtainWall)
	{
		var curtainWall = GetRevitWall(dynCurtainWall);
		if (curtainWall != null)
		{
			var grid = curtainWall.CurtainGrid;
			GetDocument().Delete(grid.GetUGridLineIds());
		}
	}
	
	public static void DeleteVGrids(Revit.Elements.Wall dynCurtainWall)
	{
		var curtainWall = GetRevitWall(dynCurtainWall);
		if (curtainWall != null)
		{
			var grid = curtainWall.CurtainGrid;
			GetDocument().Delete(grid.GetVGridLineIds());
		}
	}
	
	public static void AssignUGrid(Revit.Elements.Wall dynCurtainWall, List<double> gridSpacing)
	{
		var curtainWall = GetRevitWall(dynCurtainWall);
		if (curtainWall != null)
		{
			var grid = curtainWall.CurtainGrid;
			//doc.Delete(grid.GetMullionIds());
		}
	}

	private static Document GetDocument()
	{
		return RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
	}

	private static Autodesk.Revit.DB.Wall GetRevitWall(ments.Wall dynCurtainWall)
	{
		Document doc = RevitServices.Persistence.DocumentManager.Instance.CurrentDBDocument;
		var curtainWall = doc.GetElement(dynCurtainWall.UniqueId) as Wall;
		if (curtainWall.WallType.Kind == WallKind.Curtain){
			return doc.GetElement(dynCurtainWall.UniqueId) as Wall;
		} else {
			return null;
		}

	}

}        

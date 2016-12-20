﻿//
//  Points.cs
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

using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

    public static class Points
    { 
        [MultiReturn(new[] { "X", "Y", "Z" })]
        public static Dictionary<string, double> PointValues(Point point)
        {
            return new Dictionary<string, double>{
                    { "X", point.X },
                    { "Y", point.Y },
                    { "Z", point.Z }
            };
        }       
            
        public static List<Point> CreatePointGrid(double xSpacing, double ySpacing, int nx, int ny)
        {
            var result = new List<Point>();
            for (double x = 0; x < nx * xSpacing; x += xSpacing) {
                for (double y = 0; y < ySpacing * ny; y += ySpacing) {
                    result.Add(Point.ByCoordinates(x, y, 0));
                }
            }
            return result;    
        } 

        public static List<List<Point>> Create2dPointGrid(double xSpacing, double ySpacing, int nx, int ny)
        {
            var result = new List<List<Point>>();
            for (double y = 0; y < ny * xSpacing; y += ySpacing) {
                var list1 = new List<Point>();
                for (double x = 0; x < xSpacing * nx; x += xSpacing) {
                    list1.Add(Point.ByCoordinates(x, y, 0));
                }
                result.Add(list1);
            }
            return result;    
        } 

        public static List<List<Point>> Create2dPointGridZ(double xSpacing, double ySpacing, int nx, int ny, List<double> zValues)
        {
            var result = new List<List<Point>>();
            int i = 0;
            for (double y = 0; y < ny * xSpacing; y += ySpacing) {
                var list1 = new List<Point>();
                for (double x = 0; x < xSpacing * nx; x += xSpacing) {
                    list1.Add(Point.ByCoordinates(x, y, zValues[i]));
                    i++;
                }
                result.Add(list1);
            }
            return result;    
        }        
        
    }

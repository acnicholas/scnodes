//
//  SightLines.cs
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

namespace SightLines
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Class for creating statium/theatre seating profiles by calculating line of sight c-values.
    /// </summary>
    public class SightLines
    {
        private double treadSize;
        private double eyeHeight;
        private double distanceToFirstRowX;
        private double distanceToFirstRowY;
        private int numberOfRows;
        private double minimumRiserHeight;
        private double minimumCValue;
        private double riserIncrement;
        private List<SightLinesRow> rows;

        /// <summary>
        /// Gets all the seating plats and thier calculated values.
        /// </summary>
        /// <value>The rows.</value>
        public List<SightLinesRow> Rows
        {
            get { return rows;}
        }

        /// <summary>
        /// Gets the height of the eye.
        /// </summary>
        /// <value>The height of the eye.</value>
        public double EyeHeight
        {
            get { return eyeHeight;}
        }

        /// <summary>
        /// Gets the size of the tread.
        /// </summary>
        /// <value>The size of the tread.</value>
        public double TreadSize
        {
            get { return treadSize;}
        }

        /// <summary>
        /// Gets the number of rows.
        /// </summary>
        /// <value>The number of rows(seating plats).</value>
        public double NumberOfRows
        {
            get { return numberOfRows;}
        }
        
        private SightLines(
            double eyeHeight,
            double treadSize,
            double riserIncrement,
            double minimumCValue,
            double minimumRiserHeight,
            double numberOfRows,
            double distanceToFirstRowX,
            double distanceToFirstRowY)
        {
            rows = new List<SightLinesRow>();
            this.eyeHeight = eyeHeight;
            this.treadSize = treadSize;
            this.riserIncrement = riserIncrement;
            this.minimumCValue = minimumCValue;
            this.minimumRiserHeight = minimumRiserHeight;
            this.numberOfRows = Convert.ToInt32(numberOfRows);
            this.distanceToFirstRowX = distanceToFirstRowX;
            this.distanceToFirstRowY = distanceToFirstRowY;
            this.InitializeRows();
        }
 
        /// <summary>
        /// Creates statium/theatre seating profiles by calculating line of sight c-values.
        /// </summary>
        /// <returns>The default values.</returns>
        public static SightLines ByDefaultValues()
        {
            return ByValues();
        }
        
        /// <summary>
        /// Creates statium/theatre seating profiles by calculating line of sight c-values.
        /// </summary>
        /// <param name="eyeHeight">The eye height of the seated person.</param>
        /// <param name="treadSize">The tread size of each seating plat.</param>
        /// <param name="riserIncrement">The max increment in each r</param>
        /// <param name="minimumCValue">The minimum C value.</param>
        /// <param name="minimumRiserHeight">The minimum riser height of each seating plat.</param>
        /// <param name="numberOfRows">The number of row in the stand</param>
        /// <param name="distanceToFirstRowX">Distance from focus point</param>
        /// <param name="distanceToFirstRowY">height above focus point(z value in 3d)</param>
        public static SightLines ByValues(
            double eyeHeight = 1220,
            double treadSize = 900,
            double riserIncrement = 25,
            double minimumCValue = 60,
            double minimumRiserHeight = 180,
            double numberOfRows = 12,
            double distanceToFirstRowX = 10000,
            double distanceToFirstRowY = 1000)
        {
            return new SightLines(eyeHeight, treadSize, riserIncrement, minimumCValue,
                                  minimumRiserHeight, numberOfRows,
                                  distanceToFirstRowX,  distanceToFirstRowY);
        }

        /// <summary>
        /// Create a 2d profile of statium/theatre seating.
        /// </summary>
        /// <returns>The geometry.</returns>
        /// <param name="sightLines">Sight lines.</param>
        public static Autodesk.DesignScript.Geometry.PolyCurve ProfileGeometry(SightLines sightLines)
        {
            return ProfileGeometryVerticallyMirrored (sightLines, false);
        }

        /// <summary>
        /// Create a 2d profile of statium/theatre seating.
        /// </summary>
        /// <returns>The geometry.</returns>
        /// <param name="sightLines">Sight lines.</param>
        /// <param name="verticalMirror">Option to mirror the profile along its YZ plane.</param>
        public static Autodesk.DesignScript.Geometry.PolyCurve ProfileGeometryVerticallyMirrored(SightLines sightLines, bool verticalMirror)
        {
            var points = GoingTopPointsVerticallyMirrored(sightLines, verticalMirror);
            var geom = Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(points);
            foreach (var point in points) {
                point.Dispose();
            }
            return Autodesk.DesignScript.Geometry.PolyCurve.ByPoints(GoingTopPointsVerticallyMirrored(sightLines, verticalMirror));
        }

        /// <summary>
        /// Creates an array of all the top points along a statium/theatre.
        /// These top points can be used for modelling treads/goings
        /// </summary>
        /// <returns>The top points of each going.</returns>
        /// <param name="sightLines">Sight lines.</param>
        public static List<Autodesk.DesignScript.Geometry.Point> GoingTopPoints(SightLines sightLines)
        {
            return GoingTopPointsVerticallyMirrored (sightLines, false);
        }
        
        /// <summary>
        /// Creates an array of all the top points along a statium/theatre.
        /// These top points can be used for modelling treads/goings
        /// </summary>
        /// <returns>The top points of each going.</returns>
        /// <param name="sightLines">Sight lines.</param>
        /// <param name="verticalMirror">Option to mirror the profile along its YZ plane.</param>
        public static List<Autodesk.DesignScript.Geometry.Point> GoingTopPointsVerticallyMirrored(SightLines sightLines, bool verticalMirror)
        {
         int xMultiplier = verticalMirror ? -1 : 1;
         var result = new List<Autodesk.DesignScript.Geometry.Point>();
            for (int i = 0; i < sightLines.numberOfRows; i++) {                  
                if (i > 0) {
                 result.Add(Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                        xMultiplier * (sightLines.rows[i].EyeToFocusX - sightLines.rows[i].Going),
                        0,
                        sightLines.rows[i].HeightToFocus - sightLines.rows[i].EyeHeight));
                   result.Add(Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                        xMultiplier * sightLines.rows[i].EyeToFocusX,
                        0,
                        sightLines.rows[i].HeightToFocus - sightLines.rows[i].EyeHeight));
                } 
            }
            return result;    
        }

        /// <summary>
        /// Creates an array of all the back points along each seating plats of a statium/theatre.
        /// Back points are usefull for hosting seats.
        /// </summary>
        /// <returns>The back points for each seating plat.</returns>
        /// <param name="sightLines">Sight lines.</param>
        public static List<Autodesk.DesignScript.Geometry.Point> GoingBackPoints(SightLines sightLines)
        {
         int xMultiplier = 1;
         var result = new List<Autodesk.DesignScript.Geometry.Point>();
            for (int i = 0; i < sightLines.numberOfRows; i++) {                  
                if (i > 0) {
                    result.Add(Autodesk.DesignScript.Geometry.Point.ByCoordinates(
                        xMultiplier * sightLines.rows[i].EyeToFocusX,
                        0,
                        sightLines.rows[i].HeightToFocus - sightLines.rows[i].EyeHeight));
                } 
            }
            return result;    
        }
        
        private void InitializeRows()
        {
            rows.Clear();
            for (int i = 0; i < this.numberOfRows; i++) {
                this.rows.Insert(i,SightLinesRow.ByValues(
                    this.distanceToFirstRowX + (i * this.treadSize),
                    this.distanceToFirstRowY, 
                    this.distanceToFirstRowY + this.eyeHeight,
                    this.treadSize,
                    this.eyeHeight));
                if (i > 0) {
                    this.rows[i].RiserHeight = this.minimumRiserHeight;
                    this.rows[i].HeightToFocus = this.rows[i - 1].HeightToFocus + this.minimumRiserHeight;
                    while (this.GetCValue(i - 1, this.rows[i].RiserHeight) < this.minimumCValue) {
                        this.rows[i].RiserHeight += this.riserIncrement;
                        this.rows[i].HeightToFocus += this.riserIncrement;
                    }
                    this.rows[i - 1].CValue = this.GetCValue(i - 1, this.rows[i].RiserHeight);
                }
            }
        }
            
        /// <summary>
        /// Dump info on the current settings, calculated c-valaues and riser heights.
        /// </summary>
        /// <returns>Info as a string.</returns>
        /// <param name="sightLines">Sight lines.</param>
        public static string GetInfoString(SightLines sightLines)
        {
            string s;
            int i;

            s = string.Empty;
            s += "Number of Rows in Section            =\t" + sightLines.numberOfRows + "\r\n";
            s += "Distance to first spectator          =\t" + sightLines.distanceToFirstRowX + "\r\n";
            s += "Minimum sight line clearance         =\t" + sightLines.minimumCValue + "\r\n";
            s += "Eye level above tread                =\t" + sightLines.eyeHeight + "\r\n";
            s += "Elevation of first tread above datum =\t" + sightLines.distanceToFirstRowY + "\r\n";
            s += "Tread size                           =\t" + sightLines.treadSize + "\r\n";
            s += "Minimum riser height                 =\t" + sightLines.minimumRiserHeight + "\r\n";
            s += "Minimum riser increment              =\t" + sightLines.riserIncrement + "\r\n\r\n";
            s += "row:\triser:\tdist:\telev:\tc-value:\r\n";

            for (i = 0; i < sightLines.numberOfRows; i++) {
            string c = i > 0 ? Math.Round(sightLines.rows[i - 1].CValue, 2).ToString(CultureInfo.InvariantCulture) : "NA";
            string r = i > 0 ? Math.Round(sightLines.rows[i].RiserHeight, 2).ToString(CultureInfo.InvariantCulture) : "NA";
                s += i + 1 + "\t" + r + "\t"
                + Math.Round(sightLines.rows[i].EyeToFocusX, 2) + "\t"
                + Math.Round(sightLines.rows[i].HeightToFocus - sightLines.eyeHeight, 2) + "\t"
                    + c + "\r\n";
            }

            return s;
        }
            
        private double GetCValue(int i, double nextn)
        {
            return ((this.rows[i].EyeToFocusX * (this.rows[i].HeightToFocus + nextn)) / (this.rows[i].EyeToFocusX + this.treadSize)) - this.rows[i].HeightToFocus;
        }
    }
}

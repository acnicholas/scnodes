//
//  SightLinesRow.cs
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

namespace SCDynamoNodes
{
    /// <summary>
    /// Class for to hold individual seating plat values for statium/theatre seating.
    /// </summary>
    public class SightLinesRow
    {
        /// <summary>
        /// Gets or sets the C value.
        /// </summary>
        /// <value>The C value.</value>
        public double CValue {
            get;
            set;
        }

        /// <summary>
        /// Gets the horizontal distance from focus to eye.
        /// </summary>
        /// <value>The eye to focus x.</value>
        public double EyeToFocusX {
            get;
            set;
        }

        /// <summary>
        /// Gets the height of the riser at the current plat.
        /// </summary>
        /// <value>The height of the riser.</value>
        public double RiserHeight {
            get;
            set;
        }

        /// <summary>
        /// Gets the vertical height from focus to eye level.
        /// </summary>
        /// <value>The height to focus.</value>
        public double HeightToFocus {
            get;
            set;
        }

        /// <summary>
        /// Gets the going length.
        /// </summary>
        /// <value>The going length.</value>
        public double Going {
            get;
            set;
        }

        /// <summary>
        /// Gets the height of the eye from plat level
        /// </summary>
        /// <value>The height of the eye.</value>
        public double EyeHeight {
            get;
            set;
        }

        private SightLinesRow(
            double eyeToFocusX,
            double riserHeight,
            double heightToFocus,
            double going,
            double eyeHeight)
        {
            this.EyeToFocusX = eyeToFocusX;
            this.RiserHeight = riserHeight;
            this.HeightToFocus = heightToFocus;
            this.Going = going;
            this.EyeHeight = eyeHeight;
        }
            
        /// <summary>
        /// Creates an individual seating plat
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="eyeToFocusX">Horizontal distance from eye to focus point</param>
        /// <param name="riserHeight">Riser height at current plat</param>
        /// <param name="heightToFocus">Vertical distance from eye to focus point</param>
        /// <param name="going">Going length</param>
        /// <param name="eyeHeight">Eye height from plat level</param>
        public static SightLinesRow ByValues(
            double eyeToFocusX,
            double riserHeight,
            double heightToFocus,
            double going,
            double eyeHeight)
        {
            return new SightLinesRow(eyeToFocusX, riserHeight, heightToFocus, going, eyeHeight);
        }
          
    }
}


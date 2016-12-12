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
    public class SightLinesRow
    {
        public double CValue {
            get;
            set;
        }

        public double EyeToFocusX {
            get;
            set;
        }

        public double RiserHeight {
            get;
            set;
        }

        public double HeightToFocus {
            get;
            set;
        }

        public double Going {
            get;
            set;
        }

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


using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class ParkingLayout
    { 
        private Point startPoint;
        private double carSpaceWidth;
        private double carSpaceDepth;
        private double aisleWidth;
        private double gridSpacing;
        private int baysX;
        private int baysY;
        private double rotation;
        private double rad;
        private bool endBays;
        
        /// <summary>
        /// Get the current rotation of the parking layout
        /// </summary>
        public double Rotation
        {
            set
            {
                rotation = value;
                rad = rotation * Math.PI / 180;
            }
            get
            {
                return rotation;
            }
        }
        
        /// <summary>
        /// Get the current rotation of the parking layout in radians
        /// </summary>
        public double Radians
        {
            get
            {
                return rad;
            }
        }
        
        /// <summary>
        /// Get the grid spacing
        /// </summary>
        public double GridSpacing
        {
            get
            {
                return gridSpacing;
            }
        }
        
        /// <summary>
        /// Get the car space width
        /// </summary>
        public double CarSpaceWidth
        {
            get
            {
                return carSpaceWidth;
            }
        }
        
        /// <summary>
        /// Get the car space depth
        /// </summary>
        public double CarSpaceDepth
        {
            get
            {
                return carSpaceDepth;
            }
        }
        
        /// <summary>
        /// Get the aisle with
        /// </summary>
        public double AisleWidth
        {
            get
            {
                return aisleWidth;
            }
        }
        
        /// <summary>
        /// Get the number of bays in the x direction
        /// </summary>
        public double BaysX
        {
            get
            {
                return baysX;
            }
        }
        
        /// <summary>
        /// Get the number of bays in the Y direction
        /// </summary>
        public double BaysY
        {
            get
            {
                return baysY;
            }
        }
        
        /// <summary>
        /// Get end bays flag.
        /// </summary>
        public bool EndBays
        {
            get
            {
                return endBays;
            }
        }
              
        /// <summary>
        /// Set the roation around the start point
        /// </summary>
        public static ParkingLayout SetLayoutRotation(ParkingLayout parkingLayout, double rotation)
        {
            parkingLayout.Rotation = rotation;
            return parkingLayout;
        }

        private ParkingLayout (
            Point startPoint,
            double carSpaceWidth,
            double carSpaceDepth,
            double aisleWidth,
            double gridSpacing,
            int baysX,
            int baysY,
            bool endBays)
        {
            this.startPoint = startPoint;
            this.carSpaceWidth = carSpaceWidth;
            this.carSpaceDepth = carSpaceDepth;
            this.aisleWidth = aisleWidth;
            this.gridSpacing = gridSpacing;
            this.baysX = baysX;
            this.baysY = baysY;
            this.endBays = endBays;
            this.rotation = 0;
            this.rad = 0;
        }

        /// <summary>
        /// Create a parking layout
        /// </summary>
        /// <param name="startPoint">The bottom left of the carpark layout</param>
        /// <param name="carSpaceWidth">Width of a single car space</param>
        /// <param name="carSpaceDepth">Depth of a single car space.</param>
        /// <param name="aisleWidth">Aisle / Driveway with between opposite car spaces</param>
        /// <param name="gridSpacing">Grid Spacing perpendicualr to aisle</param>
        /// <param name="baysX">Number of bays in X(defualt) direction</param>
        /// <param name="baysY">Number of bays in Y(defualt) direction</param>
        /// <param name="endBays">Draw end bays or not?</param>
        /// <returns></returns>
        public static ParkingLayout ByLocation(
            Point startPoint,
            double carSpaceWidth = 2400,
            double carSpaceDepth = 5400,
            double aisleWidth = 5800,
            double gridSpacing = 8200,
            int baysX = 4,
            int baysY = 4,
            bool endBays = false)
        {
            return new ParkingLayout(
                startPoint,carSpaceWidth,carSpaceDepth,
                aisleWidth,gridSpacing,baysX,baysY,endBays);
        }
       
        /// <summary>
        /// Get an array of points for placing a carpark family within Revit
        /// Points will be at the center back of each parking bay with
        /// a rotation parameter to rotate each family
        /// </summary>
        /// <param name="parking"></param>
        /// <returns></returns>       
        [MultiReturn(new[] { "points", "rotationAngles", "totalNumberOfCars" })]
        public static Dictionary<string, object> HostPointsWithRotation(ParkingLayout parking)
        {
            var points = new List<Point>();
            var rotationAngles = new List<double>();
            var carsPerBay = Math.Floor(parking.GridSpacing / parking.CarSpaceWidth);
            var aisleAndCars = 2 * parking.CarSpaceDepth + parking.AisleWidth;
            var gridSpacingY = aisleAndCars / 2;
            var dx = (parking.GridSpacing - carsPerBay * parking.CarSpaceWidth) / 2;
            var carsPerSideBay = Math.Floor(gridSpacingY / parking.CarSpaceWidth);
            var dy = (gridSpacingY - carsPerSideBay * parking.CarSpaceWidth) / 2 + parking.CarSpaceWidth/2;
            var leftSideX = -300 - parking.AisleWidth - parking.CarSpaceWidth/2 - parking.CarSpaceDepth;
                   
            for (double x = 0; x < parking.BaysX * parking.GridSpacing ; x += parking.GridSpacing){
                for (double y = 0; y < parking.BaysY * aisleAndCars; y += aisleAndCars){
                    if (parking.EndBays && x == 0) {
                        for (double e = 0; e < carsPerSideBay * parking.CarSpaceWidth; e += parking.CarSpaceWidth){ 
                            var x1 = leftSideX;
                            var y1 = y + dy + e;
                            var z1 = 0;
                            var xr1 = x1 * Math.Cos(parking.Radians) - y1 * Math.Sin(parking.Radians);
                            var yr1 = x1 * Math.Sin(parking.Radians) + y1 * Math.Cos(parking.Radians);
                            points.Add(Point.ByCoordinates(xr1, yr1, z1));
                            rotationAngles.Add(90 + parking.Rotation);
                            var x2 = x1;
                            var y2 = y1 + gridSpacingY;
                            var xr2 = x2 * Math.Cos(parking.Radians) - y2 * Math.Sin(parking.Radians);
                            var yr2 = x2 * Math.Sin(parking.Radians) + y2 * Math.Cos(parking.Radians);
                            points.Add(Point.ByCoordinates(xr2, yr2, z1));
                            rotationAngles.Add(90 + parking.Rotation);
                        }
                    }                    
                    for (double b = 0; b < carsPerBay * parking.CarSpaceWidth; b += parking.CarSpaceWidth){
                        var x1 = x + b;
                        var y1 = y;
                        var xr1 = x1 * Math.Cos(parking.Radians) - y1 * Math.Sin(parking.Radians);
                        var yr1 = x1 * Math.Sin(parking.Radians) + y1 * Math.Cos(parking.Radians);
                        points.Add(Point.ByCoordinates(xr1, yr1, 0));
                        rotationAngles.Add(0 + parking.Rotation);
                        points.Add(Point.ByCoordinates(xr1, yr1, 0));
                        rotationAngles.Add(180 + parking.Rotation);
                    }
                }
            }
            
            return new Dictionary<string, object>{
                    { "points", points },
                    { "rotationAngles", rotationAngles },
                    { "totalNumberOfCars", points.Count }
            };
        }
               
    }
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
        private bool endBays;
        
        public double Rotation
        {
            set { rotation = value * Math.PI / 180; }
            get { return rotation * 180 / Math.PI; }
        }
        
        /// <summary>
        /// The roation around the start point
        /// </summary>
        public static void SetLayoutRotation(ParkingLayout parkingLayout, double rotation)
        {
            parkingLayout.Rotation = rotation;
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
        }

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
                       
        [MultiReturn(new[] { "points", "rotationAngles", "totalNumberOfCars" })]
        public static Dictionary<string, object> HostPointsWithRotation(ParkingLayout parking)
        {
            var points = new List<Point>();
            var rotationAngles = new List<double>();
            var carsPerBay = Math.Floor(parking.gridSpacing / parking.carSpaceWidth);
            var aisleAndCars = 2 * parking.carSpaceDepth + parking.aisleWidth;
            var gridSpacingY = aisleAndCars / 2;
            var dx = (parking.gridSpacing - carsPerBay * parking.carSpaceWidth) / 2;
            var carsPerSideBay = Math.Floor(gridSpacingY / parking.carSpaceWidth);
            var dy = (gridSpacingY - carsPerSideBay * parking.carSpaceWidth) / 2 + parking.carSpaceWidth/2;
            var leftSideX = -300 - parking.aisleWidth - parking.carSpaceWidth/2 - parking.carSpaceDepth;
                   
            for (double x = 0; x < parking.baysX * parking.gridSpacing ; x += parking.gridSpacing){
                for (double y = 0; y < parking.baysY * aisleAndCars; y += aisleAndCars){
                    if (parking.endBays && x == 0) {
                        for (double e = 0; e < carsPerSideBay * parking.carSpaceWidth; e += parking.carSpaceWidth){ 
                            var x1 = leftSideX;
                            var y1 = y + dy + e;
                            var z1 = 0;
                            var xr1 = x1 * Math.Cos(parking.rotation) - y1 * Math.Sin(parking.rotation);
                            var yr1 = x1 * Math.Sin(parking.rotation) + y1 * Math.Cos(parking.rotation);
                            points.Add(Point.ByCoordinates(xr1, yr1, z1));
                            rotationAngles.Add(90 + parking.rotation);
                            var x2 = x1;
                            var y2 = y1 + gridSpacingY;
                            var xr2 = x2 * Math.Cos(parking.rotation) - y2 * Math.Sin(parking.rotation);
                            var yr2 = x2 * Math.Sin(parking.rotation) + y2 * Math.Cos(parking.rotation);
                            points.Add(Point.ByCoordinates(x2, y2, z1));
                            rotationAngles.Add(90 + parking.rotation);
                        }
                    }                    
                    for (double b = 0; b < carsPerBay * parking.carSpaceWidth; b += parking.carSpaceWidth){
                        var x1 = x + b;
                        var y1 = y;
                        var xr1 = x1 * Math.Cos(parking.rotation) - y1 * Math.Sin(parking.rotation);
                        var yr1 = x1 * Math.Sin(parking.rotation) + y1 * Math.Cos(parking.rotation);
                        points.Add(Point.ByCoordinates(xr1, yr1, 0));
                        rotationAngles.Add(0 + parking.rotation);
                        points.Add(Point.ByCoordinates(xr1, yr1, 0));
                        rotationAngles.Add(180 + parking.rotation);
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
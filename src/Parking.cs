using System;
using System.Collections.Generic;
//using Autodesk.Revit.DB;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace SCDynamoNodes
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public static class Parking
    { 
        /// <summary>
        /// Create a optimal parking layout
        /// </summary>
        /// <param name="startPoint">bottom left of parking layout</param>
        /// <param name="carSpaceWidth">width of individual parking bay</param>
        /// <param name="carSpaceDepth">depth of individual parking bay</param>
        /// <param name="aisleWidth">aisle space between oppsosite parking bays</param>
        /// <param name="gridSpacing">grid spacing in x direction</param>
        /// <param name="baysX"></param>
        /// <param name="baysY"></param>
        /// <param name="endBays">draw end bays or not</param>
        /// <returns></returns>
        [MultiReturn(new[] { "points", "rotationAngles", "totalNumberOfCars" })]
        public static Dictionary<string, object> CreateCarparkPoints(
            Point startPoint,
            double carSpaceWidth = 2400,
            double carSpaceDepth = 5400,
            double aisleWidth = 5800,
            double gridSpacing = 8200,
            int baysX = 4,
            int baysY = 4,
            bool endBays = false)
        {
            var points = new List<Point>();
            var rotationAngles = new List<double>();
            var carsPerBay = Math.Floor(gridSpacing / carSpaceWidth);
            var aisleAndCars = 2 * carSpaceDepth + aisleWidth;
            var gridSpacingY = aisleAndCars / 2;
            var dx = (gridSpacing - carsPerBay * carSpaceWidth) / 2;
            var carsPerSideBay = Math.Floor(gridSpacingY / carSpaceWidth);
            var dy = (gridSpacingY - carsPerSideBay * carSpaceWidth) / 2 + carSpaceWidth/2;
            var leftSideX = -300 - aisleWidth - carSpaceWidth/2 - carSpaceDepth;
                   
            for (double x = 0; x < baysX * gridSpacing ; x += gridSpacing){
                for (double y = 0; y < baysY * aisleAndCars; y += aisleAndCars){
                    if (endBays && x == 0) {
                        for (double e = 0; e < carsPerSideBay * carSpaceWidth; e += carSpaceWidth){ 
                            points.Add(Point.ByCoordinates(leftSideX, y + dy + e, 0));
                            rotationAngles.Add(90);
                            points.Add(Point.ByCoordinates(leftSideX, y + dy + e + gridSpacingY, 0));
                            rotationAngles.Add(90);
                        }
                    }                    
                    for (double b = 0; b < carsPerBay * carSpaceWidth; b += carSpaceWidth){
                        points.Add(Point.ByCoordinates(x + b, y, 0));
                        rotationAngles.Add(0);
                        points.Add(Point.ByCoordinates(x + b, y + aisleAndCars, 0));
                        rotationAngles.Add(180);
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
}
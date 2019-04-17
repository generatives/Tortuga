using Tortuga.Graphics;
using Tortuga.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Primitives2D
{
    /// <summary>
    /// </summary>
    public static class SpriteBatchExtensions
    {
        #region Private Members

        private static readonly Dictionary<String, List<Vector2>> circleCache = new Dictionary<string, List<Vector2>>();
        //private static readonly Dictionary<String, List<Vector2>> arcCache = new Dictionary<string, List<Vector2>>();

        #endregion

        #region Private Methods
        /// <summary>
        /// Draws a list of connecting points
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// /// <param name="position">Where to position the points</param>
        /// <param name="points">The points to connect with lines</param>
        /// <param name="color">The RgbaFloat to use</param>
        /// <param name="thickness">The thickness of the lines</param>
        private static void DrawPoints(DrawDevice drawDevice, Vector2 position, List<Vector2> points, RgbaFloat color, float thickness)
        {
            if (points.Count < 2)
                return;

            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(drawDevice, points[i - 1] + position, points[i] + position, color, thickness);
            }
        }


        /// <summary>
        /// Creates a list of vectors that represents a circle
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <returns>A list of vectors that, if connected, will create a circle</returns>
        private static List<Vector2> CreateCircle(double radius, int sides)
        {
            // Look for a cached version of this circle
            String circleKey = radius + "x" + sides;
            if (circleCache.ContainsKey(circleKey))
            {
                return circleCache[circleKey];
            }

            List<Vector2> vectors = new List<Vector2>();

            const double max = 2.0 * Math.PI;
            double step = max / sides;

            for (double theta = 0.0; theta < max; theta += step)
            {
                vectors.Add(new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta))));
            }

            // then add the first vector again so it's a complete loop
            vectors.Add(new Vector2((float)(radius * Math.Cos(0)), (float)(radius * Math.Sin(0))));

            // Cache this circle so that it can be quickly drawn next time
            circleCache.Add(circleKey, vectors);

            return vectors;
        }


        /// <summary>
        /// Creates a list of vectors that represents an arc
        /// </summary>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="sides">The number of sides to generate in the circle that this will cut out from</param>
        /// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
        /// <param name="radians">The radians to draw, clockwise from the starting angle</param>
        /// <returns>A list of vectors that, if connected, will create an arc</returns>
        private static List<Vector2> CreateArc(float radius, int sides, float startingAngle, float radians)
        {
            List<Vector2> points = new List<Vector2>();
            points.AddRange(CreateCircle(radius, sides));
            points.RemoveAt(points.Count - 1); // remove the last point because it's a duplicate of the first

            // The circle starts at (radius, 0)
            double curAngle = 0.0;
            double anglePerSide = (Math.PI * 2) / sides;

            // "Rotate" to the starting point
            while ((curAngle + (anglePerSide / 2.0)) < startingAngle)
            {
                curAngle += anglePerSide;

                // move the first point to the end
                points.Add(points[0]);
                points.RemoveAt(0);
            }

            // Add the first point, just in case we make a full circle
            points.Add(points[0]);

            // Now remove the points at the end of the circle to create the arc
            int sidesInArc = (int)((radians / anglePerSide) + 0.5);
            points.RemoveRange(sidesInArc + 1, points.Count - sidesInArc - 1);

            return points;
        }

        #endregion

        #region FillRectangle

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void FillRectangle(this DrawDevice drawDevice, RectangleF rect, RgbaFloat color)
        {
            // Simply use the function already there
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), rect, color);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        /// <param name="angle">The angle in radians to draw the rectangle at</param>
        public static void FillRectangle(this DrawDevice drawDevice, RectangleF rect, RgbaFloat color, float angle)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), rect.Size, Matrix3x2.CreateTranslation(rect.Position) * Matrix3x2.CreateRotation(angle), color);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void FillRectangle(this DrawDevice drawDevice, Vector2 location, Vector2 size, RgbaFloat color)
        {
            FillRectangle(drawDevice, location, size, color, 0.0f);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="angle">The angle in radians to draw the rectangle at</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void FillRectangle(this DrawDevice drawDevice, Vector2 location, Vector2 size, RgbaFloat color, float angle)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), size, Matrix3x2.CreateTranslation(location) * Matrix3x2.CreateRotation(angle), color);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x">The X coord of the left side</param>
        /// <param name="y">The Y coord of the upper side</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void FillRectangle(this DrawDevice drawDevice, float x, float y, float w, float h, RgbaFloat color)
        {
            FillRectangle(drawDevice, new Vector2(x, y), new Vector2(w, h), color, 0.0f);
        }


        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x">The X coord of the left side</param>
        /// <param name="y">The Y coord of the upper side</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        /// <param name="angle">The angle of the rectangle in radians</param>
        public static void FillRectangle(this DrawDevice drawDevice, float x, float y, float w, float h, RgbaFloat color, float angle)
        {
            FillRectangle(drawDevice, new Vector2(x, y), new Vector2(w, h), color, angle);
        }

        #endregion

        #region DrawRectangle

        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void DrawRectangle(this DrawDevice drawDevice, Rectangle rect, RgbaFloat color)
        {
            DrawRectangle(drawDevice, rect, color, 1.0f);
        }


        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the lines</param>
        public static void DrawRectangle(this DrawDevice drawDevice, Rectangle rect, RgbaFloat color, float thickness)
        {

            // TODO: Handle rotations
            // TODO: Figure out the pattern for the offsets required and then handle it in the line instead of here

            DrawLine(drawDevice, new Vector2(rect.X, rect.Y), new Vector2(rect.Right, rect.Y), color, thickness); // top
            DrawLine(drawDevice, new Vector2(rect.X + 1f, rect.Y), new Vector2(rect.X + 1f, rect.Bottom + thickness), color, thickness); // left
            DrawLine(drawDevice, new Vector2(rect.X, rect.Bottom), new Vector2(rect.Right, rect.Bottom), color, thickness); // bottom
            DrawLine(drawDevice, new Vector2(rect.Right + 1f, rect.Y), new Vector2(rect.Right + 1f, rect.Bottom + thickness), color, thickness); // right
        }


        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        public static void DrawRectangle(this DrawDevice drawDevice, Vector2 location, Vector2 size, RgbaFloat color)
        {
            DrawRectangle(drawDevice, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, 1.0f);
        }


        /// <summary>
        /// Draws a rectangle with the thickness provided
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="location">Where to draw</param>
        /// <param name="size">The size of the rectangle</param>
        /// <param name="color">The RgbaFloat to draw the rectangle in</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawRectangle(this DrawDevice drawDevice, Vector2 location, Vector2 size, RgbaFloat color, float thickness)
        {
            DrawRectangle(drawDevice, new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);
        }

        #endregion

        #region DrawLine

        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x1">The X coord of the first point</param>
        /// <param name="y1">The Y coord of the first point</param>
        /// <param name="x2">The X coord of the second point</param>
        /// <param name="y2">The Y coord of the second point</param>
        /// <param name="color">The RgbaFloat to use</param>
        public static void DrawLine(this DrawDevice drawDevice, float x1, float y1, float x2, float y2, RgbaFloat color)
        {
            DrawLine(drawDevice, new Vector2(x1, y1), new Vector2(x2, y2), color, 1.0f);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x1">The X coord of the first point</param>
        /// <param name="y1">The Y coord of the first point</param>
        /// <param name="x2">The X coord of the second point</param>
        /// <param name="y2">The Y coord of the second point</param>
        /// <param name="color">The RgbaFloat to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this DrawDevice drawDevice, float x1, float y1, float x2, float y2, RgbaFloat color, float thickness)
        {
            DrawLine(drawDevice, new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <param name="color">The RgbaFloat to use</param>
        public static void DrawLine(this DrawDevice drawDevice, Vector2 point1, Vector2 point2, RgbaFloat color)
        {
            DrawLine(drawDevice, point1, point2, color, 1.0f);
        }


        /// <summary>
        /// Draws a line from point1 to point2 with an offset
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="point1">The first point</param>
        /// <param name="point2">The second point</param>
        /// <param name="color">The RgbaFloat to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this DrawDevice drawDevice, Vector2 point1, Vector2 point2, RgbaFloat color, float thickness)
        {
            // calculate the distance between the two vectors
            float distance = Vector2.Distance(point1, point2);

            // calculate the angle between the two vectors
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

            DrawLine(drawDevice, point1, distance, angle, color, thickness);
        }


        /// <summary>
        /// Draws a line from a point along an angle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="point">The starting point</param>
        /// <param name="length">The length of the line</param>
        /// <param name="angle">The angle of this line from the starting point in radians</param>
        /// <param name="color">The RgbaFloat to use</param>
        public static void DrawLine(this DrawDevice drawDevice, Vector2 point, float length, float angle, RgbaFloat color)
        {
            DrawLine(drawDevice, point, length, angle, color, 1.0f);
        }


        /// <summary>
        /// Draws a line from a point along an angle with a thickness
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="point">The starting point</param>
        /// <param name="length">The length of the line</param>
        /// <param name="angle">The angle of this line from the starting point</param>
        /// <param name="color">The RgbaFloat to use</param>
        /// <param name="thickness">The thickness of the line</param>
        public static void DrawLine(this DrawDevice drawDevice, Vector2 point, float length, float angle, RgbaFloat color, float thickness)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1),
                new Vector2(length, thickness),
                Matrix3x2.CreateTranslation(0, -thickness / 2) * Matrix3x2.CreateRotation(angle) * Matrix3x2.CreateTranslation(point),
                color);
        }

        #endregion

        #region PutPixel

        public static void PutPixel(this DrawDevice drawDevice, float x, float y, RgbaFloat color)
        {
            PutPixel(drawDevice, new Vector2(x, y), color);
        }


        public static void PutPixel(this DrawDevice drawDevice, Vector2 position, RgbaFloat color)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), new RectangleF(position.X, position.Y, 1, 1), color);
        }

        #endregion

        #region DrawCircle

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The RgbaFloat of the circle</param>
        public static void DrawCircle(this DrawDevice drawDevice, Vector2 center, float radius, int sides, RgbaFloat color)
        {
            DrawPoints(drawDevice, center, CreateCircle(radius, sides), color, 1.0f);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The RgbaFloat of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        public static void DrawCircle(this DrawDevice drawDevice, Vector2 center, float radius, int sides, RgbaFloat color, float thickness)
        {
            DrawPoints(drawDevice, center, CreateCircle(radius, sides), color, thickness);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x">The center X of the circle</param>
        /// <param name="y">The center Y of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The RgbaFloat of the circle</param>
        public static void DrawCircle(this DrawDevice drawDevice, float x, float y, float radius, int sides, RgbaFloat color)
        {
            DrawPoints(drawDevice, new Vector2(x, y), CreateCircle(radius, sides), color, 1.0f);
        }


        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="x">The center X of the circle</param>
        /// <param name="y">The center Y of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="color">The RgbaFloat of the circle</param>
        /// <param name="thickness">The thickness of the lines used</param>
        public static void DrawCircle(this DrawDevice drawDevice, float x, float y, float radius, int sides, RgbaFloat color, float thickness)
        {
            DrawPoints(drawDevice, new Vector2(x, y), CreateCircle(radius, sides), color, thickness);
        }

        #endregion

        #region DrawArc

        /// <summary>
        /// Draw a arc
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="center">The center of the arc</param>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
        /// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
        /// <param name="color">The RgbaFloat of the arc</param>
        public static void DrawArc(this DrawDevice drawDevice, Vector2 center, float radius, int sides, float startingAngle, float radians, RgbaFloat color)
        {
            DrawArc(drawDevice, center, radius, sides, startingAngle, radians, color, 1.0f);
        }


        /// <summary>
        /// Draw a arc
        /// </summary>
        /// <param name="drawDevice">The destination drawing surface</param>
        /// <param name="center">The center of the arc</param>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="sides">The number of sides to generate</param>
        /// <param name="startingAngle">The starting angle of arc, 0 being to the east, increasing as you go clockwise</param>
        /// <param name="radians">The number of radians to draw, clockwise from the starting angle</param>
        /// <param name="color">The RgbaFloat of the arc</param>
        /// <param name="thickness">The thickness of the arc</param>
        public static void DrawArc(this DrawDevice drawDevice, Vector2 center, float radius, int sides, float startingAngle, float radians, RgbaFloat color, float thickness)
        {
            List<Vector2> arc = CreateArc(radius, sides, startingAngle, radians);
            //List<Vector2> arc = CreateArc2(radius, sides, startingAngle, degrees);
            DrawPoints(drawDevice, center, arc, color, thickness);
        }

        #endregion
    }
}
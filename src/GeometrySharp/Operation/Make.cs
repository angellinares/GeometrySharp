﻿using GeometrySharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using GeometrySharp.Geometry;

namespace GeometrySharp.Operation
{
    // ToDo this class has to be commented.
    // ToDo this class has to be tested.
    // Todo this class has to be understand the need, and eventually removed or modified.

    public class Make
    {
        public Make()
        {

        }

        public static NurbsCurve Polyline(List<Vector3> points)
        {
            Knot knots = new Knot() { 0.0, 0.0 };
            double lsum = 0.0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                lsum += points[i].DistanceTo(points[i + 1]);
                knots.Add(lsum);
            }
            knots.Add(lsum);
            var weights = points.Select(x => 1.0).ToList();
            points.ForEach(x => weights.Add(1.0));
            return new NurbsCurve(1, knots, LinearAlgebra.PointsHomogeniser(points, weights));
        }

        public static NurbsCurve RationalBezierCurve(List<Vector3> controlPoints, List<double> weights = null)
        {
            var degree = controlPoints.Count - 1;
            var knots = new Knot();
            for (int i = 0; i < degree + 1; i++)
                knots.Add(0.0);
            for (int i = 0; i < degree + 1; i++)
                knots.Add(1.0);
            if (weights == null)
                weights = Sets.RepeatData(1.0, controlPoints.Count);
            return new NurbsCurve(degree, knots, LinearAlgebra.PointsHomogeniser(controlPoints, weights));
            //weights = Sets.RepeatData(1.0, controlPoints.Count);
            //return new NurbsCurveData(degree, knots, Evaluation.PointsHomogeniser(controlPoints, weights));
            //return null;
        }
        

        //////////////////////////// =================================== not implemented yet ================================== ///////////////////

        
        /// <summary>
        /// Create the control points, weights, and knots of an elliptical arc
        /// </summary>
        /// <param name="center">the center</param>
        /// <param name="xaxis">the scaled x axis</param>
        /// <param name="yaxis">the scaled y axis</param>
        /// <param name="startAngle">start angle of the ellipse arc, between 0 and 2pi, where 0 points at the xaxis</param>
        /// <param name="endAngle">end angle of the arc, between 0 and 2pi, greater than the start angle</param>
        /// <returns>a NurbsCurveData object representing a NURBS curve</returns>
        public static NurbsCurve EllipseArc(Vector3 center, Vector3 xaxis, Vector3 yaxis, double startAngle, double endAngle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the control points, weights, and knots of an arbitrary arc
        /// (Corresponds to Algorithm A7.1 from Piegl & Tiller)
        /// </summary>
        /// <param name="center">the center of the arc</param>
        /// <param name="xaxis">the xaxis of the arc</param>
        /// <param name="yaxis">orthogonal yaxis of the arc</param>
        /// <param name="radius">radius of the arc</param>
        /// <param name="startAngle">start angle of the arc, between 0 and 2pi</param>
        /// <param name="endAngle">end angle of the arc, between 0 and 2pi, greater than the start angle</param>
        /// <returns>a NurbsCurveData object representing a NURBS curve</returns>
        public static NurbsCurve Arc(Vector3 center, Vector3 xaxis, Vector3 yaxis, double radius, double startAngle, double endAngle)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the control points, weights, and knots of an extruded surface
        /// </summary>
        /// <param name="axis">axis of the extrusion</param>
        /// <param name="length">length of the extrusion</param>
        /// <param name="profile">a NurbsCurveData object representing a NURBS surface</param>
        /// <returns>an object with the following properties: controlPoints, weights, knots, degree</returns>
        public static NurbsSurface ExtrudedSurface(Vector3 axis, double length, NurbsCurve profile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the control points, weights, and knots of a cylinder
        /// </summary>
        /// <param name="axis">normalized axis of cylinder</param>
        /// <param name="xaxis">xaxis in plane of cylinder</param>
        /// <param name="basePt">position of base of cylinder</param>
        /// <param name="height">height from base to top</param>
        /// <param name="radius">radius of the cylinder</param>
        /// <returns>an object with the following properties: controlPoints, weights, knotsU, knotsV, degreeU, degreeV</returns>
        public static NurbsSurface CylindricalSurface(Vector3 axis, Vector3 xaxis, Vector3 basePt, double height, double radius)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="center"></param>
        /// <param name="axis"></param>
        /// <param name="theta"></param>
        /// <returns></returns>
        public static NurbsSurface RevolvedSurface(NurbsCurve profile, Vector3 center, Vector3 axis, double theta)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the control points, weights, and knots of a sphere
        /// </summary>
        /// <param name="center">the center of the sphere</param>
        /// <param name="axis">normalized axis of sphere</param>
        /// <param name="xaxis">vector perpendicular to axis of sphere, starting the rotation of the sphere</param>
        /// <param name="radius">radius of the sphere</param>
        /// <returns>an object with the following properties: controlPoints, weights, knotsU, knotsV, degreeU, degreeV</returns>
        public static NurbsSurface SphericalSurface(Vector3 center, Vector3 axis, Vector3 xaxis, double radius)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create the control points, weights, and knots of a cone
        /// </summary>
        /// <param name="axis">normalized axis of cone</param>
        /// <param name="xaxis"></param>
        /// <param name="basePt">position of base of cone</param>
        /// <param name="height">height from base to tip</param>
        /// <param name="radius">radius at the base of the cone</param>
        /// <returns></returns>
        public static NurbsSurface ConicalSurface(Vector3 axis, Vector3 xaxis, Vector3 basePt, double height, double radius)
        {
            throw new NotImplementedException();
        }

        public static NurbsCurve RationalInterpCurve(List<List<double>> points, int degree, bool homogeneousPoints = false, Vector3 start_tangent = null, Vector3 end_tangent = null)
        {
            throw new NotImplementedException();
        }
    }
}

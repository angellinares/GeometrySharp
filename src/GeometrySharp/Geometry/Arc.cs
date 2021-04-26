﻿using GeometrySharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using GeometrySharp.Operation;

namespace GeometrySharp.Geometry
{
    // ToDo: ArcFromTangent
    /// <summary>
    /// Represents the value of a plane, two angles (interval) and a radius (radiance).
    /// </summary>
    public class Arc : IEquatable<Arc>
    {
        internal Interval AngleDomain;

        /// <summary>
        /// Initializes an arc from a plane, a radius and an angle domain expressed as an interval.
        /// </summary>
        /// <param name="plane">Base plane.</param>
        /// <param name="radius">Radius value.</param>
        /// <param name="angleDomain">Interval defining the angle of the arc. Interval should be between 0.0 to 2Pi</param>
        public Arc(Plane plane, double radius, Interval angleDomain)
        {
            // ToDo: If interval isDecreasing interval 0 to -40 -> 360-40 to 0 use angular diff.
            // ToDo: If angle length > 2.0*Pi use angular diff. 
            Plane = plane;
            Radius = radius;
            AngleDomain = angleDomain;
        }

        /// <summary>
        /// Initializes an arc from a plane, a radius and an angle.
        /// </summary>
        /// <param name="plane">Base plane.</param>
        /// <param name="radius">Radius value.</param>
        /// <param name="angle">Angle of the arc.</param>
        public Arc(Plane plane, double radius, double angle)
            : this(plane, radius, new Interval(0.0, angle))
        {
        }

        /// <summary>
        /// Initializes an arc from three points.
        /// https://github.com/sergarrido/random/tree/master/circle3d
        /// </summary>
        /// <param name="pt1">Start point of the arc.</param>
        /// <param name="pt2">Interior point on arc.</param>
        /// <param name="pt3">End point of the arc.</param>
        public Arc(Vector3 pt1, Vector3 pt2, Vector3 pt3)
        {
            Circle c = new Circle(pt1, pt2, pt3);
            Plane p = c.Plane;
            (double u, double v) = p.ClosestParameters(pt3);

            double angle = Math.Atan2(v, u);
            if (angle < 0.0)
            {
                angle += 2.0 * Math.PI;
            }

            Plane = p;
            Radius = c.Radius;
            AngleDomain = new Interval(0.0, angle);
        }

        /// <summary>
        /// Gets the plane on which the arc lies.
        /// </summary>
        public Plane Plane { get; }

        /// <summary>
        /// Gets the radius of the arc.
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Gets the center point of this arc.
        /// </summary>
        public Vector3 Center => Plane.Origin;

        /// <summary>
        /// Gets the angle of this arc.
        /// Angle value in radians.
        /// </summary>
        public double Angle => AngleDomain.Length;

        /// <summary>
        /// Calculates the length of the arc.
        /// </summary>
        public double Length => Math.Abs(Angle * Radius);

        /// <summary>
        /// Gets the BoundingBox of this arc.
        /// https://stackoverflow.com/questions/1336663/2d-bounding-box-of-a-sector
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                Plane orientedPlane = Plane.Align(Vector3.XAxis);
                Vector3 pt0 = PointAt(0.0);
                Vector3 pt1 = PointAt(1.0);
                Vector3 ptC = orientedPlane.Origin;

                double theta0 = Math.Atan2(pt0[1] - ptC[1], pt0[0] - ptC[0]);
                double theta1 = Math.Atan2(pt1[1] - ptC[1], pt1[0] - ptC[0]);

                List<Vector3> pts = new List<Vector3>{ pt0, pt1 };

                if (AnglesSequence(theta0, 0, theta1))
                {
                    pts.Add(ptC + orientedPlane.XAxis * Radius);
                }
                if (AnglesSequence(theta0, Math.PI / 2, theta1))
                {
                    pts.Add(ptC + orientedPlane.YAxis * Radius);
                }
                if (AnglesSequence(theta0, Math.PI, theta1))
                {
                    pts.Add(ptC - orientedPlane.XAxis * Radius);
                }
                if (AnglesSequence(theta0, Math.PI * 3 / 2, theta1))
                {
                    pts.Add(ptC - orientedPlane.YAxis * Radius);
                }

                return new BoundingBox(pts);
            }
        }

        private bool AnglesSequence(double angle1, double angle2, double angle3)
        {
            return AngularDiff(angle1, angle2) + AngularDiff(angle2, angle3) < 2 * Math.PI;
        }

        private double AngularDiff(double theta1, double theta2)
        {
            double dif = theta2 - theta1;
            while (dif >= 2 * Math.PI)
                dif -= 2 * Math.PI;
            while (dif <= 0)
                dif += 2 * Math.PI;
            return dif;
        }

        /// <summary>
        /// Returns the point at the parameter t on the arc.
        /// </summary>
        /// <param name="t">A parameter between 0.0 to 1.0 or between the angle domain.></param>
        /// <param name="parametrize">True per default using parametrize value between 0.0 to 1.0.</param>
        /// <returns>Point on the arc.</returns>
        public Vector3 PointAt(double t, bool parametrize = true)
        {

            double tRemap = (parametrize) ? GeoSharpMath.RemapValue(t, new Interval(0.0, 1.0), AngleDomain) : t;

            Vector3 xDir = Plane.XAxis * Math.Cos(tRemap) * Radius;
            Vector3 yDir = Plane.YAxis * Math.Sin(tRemap) * Radius;

            return Plane.Origin + xDir + yDir;
        }

        /// <summary>
        /// Returns the tangent at the parameter t on the arc.
        /// </summary>
        /// <param name="t">A parameter between 0.0 to 1.0 or between the angle domain.</param>
        /// <param name="parametrize">True per default using parametrize value between 0.0 to 1.0.</param>
        /// <returns>Tangent at the t parameter.</returns>
        public Vector3 TangentAt(double t, bool parametrize = true)
        {
            double tRemap = (parametrize) ? GeoSharpMath.RemapValue(t, new Interval(0.0, 1.0), AngleDomain) : t;

            return new Circle(this.Plane, this.Radius).TangentAt(tRemap, false);
        }

        /// <summary>
        /// Calculates the point on an arc that is close to a test point.
        /// </summary>
        /// <param name="pt">The test point. Point to get close to.</param>
        /// <returns>The point on the arc that is close to the test point.</returns>
        public Vector3 ClosestPt(Vector3 pt)
        {
            double twoPi = 2.0 * Math.PI;

            (double u, double v) = Plane.ClosestParameters(pt);
            if (Math.Abs(u) < GeoSharpMath.MAXTOLERANCE && Math.Abs(v) < GeoSharpMath.MAXTOLERANCE)
            {
                return PointAt(0.0);
            }

            double t = Math.Atan2(v, u);
            if (t < 0.0)
            {
                t += twoPi;
            }

            t -= AngleDomain.Min;

            while (t < 0.0)
            {
                t += twoPi;
            }

            while (t >= twoPi)
            {
                t -= twoPi;
            }

            double t1 = AngleDomain.Length;
            if (t > t1)
            {
                t = t > 0.5 * t1 + Math.PI ? 0.0 : t1;
            }

            return PointAt(AngleDomain.Min + t, false);
        }

        /// <summary>
        /// Applies a transformation to the plane where the arc is on.
        /// </summary>
        /// <param name="transformation">Transformation matrix to apply.</param>
        /// <returns>A transformed arc.</returns>
        public Arc Transform(Transform transformation)
        {
            Plane plane = this.Plane.Transform(transformation);
            Interval angleDomain = new Interval(this.AngleDomain.Min, this.AngleDomain.Max);

            return new Arc(plane, this.Radius, angleDomain);
        }

        /// <summary>
        /// Constructs a nurbs curve representation of this arc.
        /// Implementation of Algorithm A7.1 from The NURBS Book by Piegl & Tiller.
        /// </summary>
        /// <returns>A Nurbs curve shaped like this arc.</returns>
        public NurbsCurve ToNurbsCurve()
        {
            double radius = Radius;
            Vector3 axisX = Plane.XAxis;
            Vector3 axisY = Plane.YAxis;
            double theta = Angle;
            int numberOfArc;
            Vector3[] ctrPts;
            double[] weights;

            // Number of arcs.
            double piNum = 0.5 * Math.PI;
            if (theta <= piNum)
            {
                numberOfArc = 1;
                ctrPts = new Vector3[3];
                weights = new double[3];
            }
            else if (theta <= piNum * 2)
            {
                numberOfArc = 2;
                ctrPts = new Vector3[5];
                weights = new double[5];
            }
            else if (theta <= piNum * 3)
            {
                numberOfArc = 3;
                ctrPts = new Vector3[7];
                weights = new double[7];
            }
            else
            {
                numberOfArc = 4;
                ctrPts = new Vector3[9];
                weights = new double[9];
            }

            double detTheta = theta / numberOfArc;
            double weight1 = Math.Cos(detTheta / 2);
            Vector3 p0 = Center + (axisX * (radius * Math.Cos(AngleDomain.Min)) + axisY * (radius * Math.Sin(AngleDomain.Min)));
            Vector3 t0 = axisY * Math.Cos(AngleDomain.Min) - axisX * Math.Sin(AngleDomain.Min);

            Knot knots = new Knot(Sets.RepeatData(0.0, ctrPts.Length + 3));
            int index = 0;
            double angle = AngleDomain.Min;

            ctrPts[0] = p0;
            weights[0] = 1.0;

            for (int i = 1; i < numberOfArc + 1; i++)
            {
                angle += detTheta;
                Vector3 p2 = Center + (axisX * (radius * Math.Cos(angle)) + axisY * (radius * Math.Sin(angle)));
                
                weights[index + 2] = 1;
                ctrPts[index + 2] = p2;

                Vector3 t2 = (axisY * Math.Cos(angle)) - (axisX * Math.Sin(angle));
                Line ln0 = new Line(p0, t0.Unitize() + p0);
                Line ln1 = new Line(p2, t2.Unitize() + p2);
                bool intersect = Intersect.LineLine(ln0, ln1, out _, out _, out double u0, out double u1);
                Vector3 p1 = p0 + (t0 * u0);

                weights[index + 1] = weight1;
                ctrPts[index + 1] = p1;
                index += 2;

                if (i >= numberOfArc) continue;
                p0 = p2;
                t0 = t2;
            }

            int j = 2 * numberOfArc + 1;
            for (int i = 0; i < 3; i++)
            {
                knots[i] = 0.0;
                knots[i + j] = 1.0;
            }

            switch (numberOfArc)
            {
                case 2:
                    knots[3] = knots[4] = 0.5;
                    break;
                case 3:
                    knots[3] = knots[4] = (double) 1 / 3;
                    knots[5] = knots[6] = (double) 2 / 3;
                    break;
                case 4:
                    knots[3] = knots[4] = 0.25;
                    knots[5] = knots[6] = 0.5;
                    knots[7] = knots[8] = 0.75;
                    break;
            }

            return new NurbsCurve(2, knots, ctrPts.ToList(), weights.ToList());
        }

        /// <summary>
        /// Determines whether the arc is equal to another arc.
        /// The arcs are equal if have the same plane, radius and angle.
        /// </summary>
        /// <param name="other">The arc to compare to.</param>
        /// <returns>True if the arc are equal, otherwise false.</returns>
        public bool Equals(Arc other)
        {
            return Math.Abs(this.Radius - other.Radius) < GeoSharpMath.MAXTOLERANCE &&
                   Math.Abs(this.Angle - other.Angle) < GeoSharpMath.MAXTOLERANCE &&
                   this.Plane == other.Plane;
        }

        /// <summary>
        /// Computes a hash code for the arc.
        /// </summary>
        /// <returns>A unique hashCode of an arc.</returns>
        public override int GetHashCode()
        {
            return this.Radius.GetHashCode() ^ this.Angle.GetHashCode() ^ this.Plane.GetHashCode();
        }

        /// <summary>
        /// Determines whether two arcs have same values.
        /// </summary>
        /// <param name="a">The first arc.</param>
        /// <param name="b">The second arc.</param>
        /// <returns>True if all the value are equal, otherwise false.</returns>
        public static bool operator ==(Arc a, Arc b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Determines whether two arcs have different values.
        /// </summary>
        /// <param name="a">The first arc.</param>
        /// <param name="b">The second arc.</param>
        /// <returns>True if all the value are different, otherwise false.</returns>
        public static bool operator !=(Arc a, Arc b)
        {
            return !Equals(a, b);
        }

        /// <summary>
        /// Gets the text representation of an arc.
        /// </summary>
        /// <returns>Text value.</returns>
        public override string ToString()
        {
            return $"Arc(R:{Radius} - A:{GeoSharpMath.ToDegrees(Angle)})";
        }
    }
}

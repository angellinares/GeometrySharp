﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GeometrySharp.Core;
using GeometrySharp.Operation;

namespace GeometrySharp.Geometry
{
    /// <summary>
    /// A NURBS curve - this class represents the base class of many curve types and provides tools for analysis and evaluation.
    /// This object is deliberately constrained to be immutable. The methods deliberately return copies.
    /// /// </summary>
    public class NurbsCurve : IEquatable<NurbsCurve>
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        public NurbsCurve()
        {
        }

        /// <summary>
        /// Creates a Nurbs curve object.
        /// </summary>
        /// <param name="degree">Curve degree.</param>
        /// <param name="knots">Knot defining the curve.</param>
        /// <param name="controlPoints">Control points, as a collection of Vector3.</param>
        /// <param name="weights">Weight values, as a collection of doubles.</param>
        public NurbsCurve(int degree, Knot knots, List<Vector3> controlPoints, List<double>? weights = null)
        {
            if (controlPoints is null) throw new ArgumentNullException(nameof(ControlPoints));
            if (knots is null) throw new ArgumentNullException(nameof(Knots));
            if (degree < 1) throw new ArgumentException("Degree must be greater than 1!");
            if (knots.Count != controlPoints.Count + degree + 1)
                throw new ArgumentException("Number of points + degree + 1 must equal knots length!");
            if (!knots.AreValidKnots(degree, controlPoints.Count))
                throw new ArgumentException("Invalid knot format! Should begin with degree + 1 repeats and end with degree + 1 repeats!");

            Weights = weights ?? Sets.RepeatData(1.0, controlPoints.Count);
            Degree = degree;
            Knots = knots;
            HomogenizedPoints = LinearAlgebra.PointsHomogeniser(controlPoints, Weights);
        }

        /// <summary>
        /// Creates a Nurbs curve object.
        /// </summary>
        /// <param name="controlPoints">Control points, as a collection of Vector3.</param>
        /// <param name="degree">Curve degree.</param>
        public NurbsCurve(List<Vector3> controlPoints, int degree)
            : this(degree, new Knot(degree, controlPoints.Count), controlPoints)
        {
        }

        /// <summary>
        /// Construct a NurbsCurve by a NurbsCurve object.
        /// </summary>
        /// <param name="curve">The curve object</param>
        public NurbsCurve(NurbsCurve curve)
        {
            Degree = curve.Degree;
            HomogenizedPoints = new List<Vector3>(curve.HomogenizedPoints);
            Knots = new Knot(curve.Knots);
            Weights = new List<double>(curve.Weights!);
        }

        /// <summary>
        /// Integer degree of curve.
        /// </summary>
        public int Degree { get; }

        /// <summary>
        /// 2d list of control points, where each control point is a list like (x,y,z) of length (dim).
        /// </summary>
        public List<Vector3> ControlPoints => LinearAlgebra.PointDehomogenizer1d(HomogenizedPoints);

        /// <summary>
        /// 2d list of points, where represented a set (wi*pi, wi) with length (dim+1).
        /// </summary>
        public List<Vector3> HomogenizedPoints { get; }

        /// <summary>
        /// List of weight values.
        /// </summary>
        public List<double> Weights { get; }

        /// <summary>
        /// List of non-decreasing knot values.
        /// </summary>
        public Knot Knots { get; }

        /// <summary>
        /// Obtain a copy of the NurbsCurve.
        /// </summary>
        /// <returns>The copied curve.</returns>
        public NurbsCurve Clone() => new NurbsCurve(this);

        /// <summary>
        /// Determine the valid domain of the curve.
        /// </summary>
        /// <returns>representing the high and end point of the domain of the curve.</returns>
        public Interval Domain() => new Interval(this.Knots.First(), this.Knots.Last());

        /// <summary>
        /// Transform a curve with the given matrix.
        /// </summary>
        /// <param name="mat">4d set representing the transform.</param>
        /// <returns>A new NurbsCurve transformed.</returns>
        /// ToDo implement the async method.
        public NurbsCurve Transform(Matrix mat) => new NurbsCurve(Modify.RationalCurveTransform(this, mat));

        /// <summary>
        /// Split the curve at the give parameter.
        /// </summary>
        /// <param name="t">The parameter at which to split the curve</param>
        /// <returns>Two curves - one at the lower end of the parameter range and one at the higher end.</returns>
        /// ToDo implement the async method.
        public List<NurbsCurve> Split(double t) => Divide.CurveSplit(this, t);

        /// <summary>
        /// Sample a point at the given parameter.
        /// </summary>
        /// <param name="t">The parameter to sample the curve.</param>
        /// <returns>A point at the given parameter.</returns>
        /// ToDo implement the async method.
        public Vector3 PointAt(double t) => LinearAlgebra.PointDehomogenizer(Evaluation.CurvePointAt(this, t));

        /// <summary>
        /// Obtain the curve tangent at the given parameter.
        /// </summary>
        /// <param name="t">The parameter to sample the curve.</param>
        /// <returns>The vector at the given parameter.</returns>
        /// ToDo implement the async method.
        public Vector3 Tangent(double t) => Evaluation.RationalCurveTanget(this, t);

        /// <summary>
        /// Determine the arc length of the curve.
        /// </summary>
        /// <returns>The length of the curve.</returns>
        /// ToDo implement the async method.
        public double Length() => Analyze.RationalCurveArcLength(this);

        /// <summary>
        /// Get the derivatives at a given parameter.
        /// </summary>
        /// <param name="parameter">The parameter to sample the curve.</param>
        /// <param name="numberDerivs">The number of derivatives to obtain.</param>
        /// <returns>A point represented by an array of length (dim).</returns>
        /// ToDo implement the async method.
        /// Note this method doesn't see necessary here in this class.
        public List<Vector3> Derivatives(double parameter, int numberDerivs = 1) =>
            Evaluation.RationalCurveDerivatives(this, parameter, numberDerivs);

        /// <summary>
        /// Reverse the parametrization of the curve.
        /// </summary>
        /// <returns>A reversed curve.</returns>
        /// ToDo implement the async method.
        public NurbsCurve Reverse() => Modify.ReverseCurve(this);

        /// <summary>
        /// Divide a curve into equal length segments.
        /// </summary>
        /// <param name="divisions">Number of divisions of the curve.</param>
        /// <returns>A tuple define the t values where the curve is divided and the lengths between each division.</returns>
        /// ToDo implement the async method.
        public (List<double> tValues, List<double> lengths) DividedByCount(int divisions) =>
            Divide.RationalCurveByDivisions(this, divisions);

        /// <summary>
        /// Divide a curve into segments with a preset length.
        /// </summary>
        /// <param name="divisions">Length of each segment.</param>
        /// <returns>A tuple define the t values where the curve is divided and the lengths between each division.</returns>
        /// ToDo implement the async method.
        public (List<double> tValues, List<double> lengths) DividedByLength(double length) =>
            Divide.RationalCurveByEqualLength(this, length);

        /// <summary>
        /// Compute the closest point on the curve to the given point.
        /// </summary>
        /// <param name="point">Point to analyze.</param>
        /// <param name="t">Parameter of local closest point.</param>
        /// <returns>The closest point on the curve.</returns>
        /// ToDo implement the async method.
        public Vector3 ClosestPoint(Vector3 point, out double t) =>
            Analyze.RationalCurveClosestPoint(this, point, out t);

        /// <summary>
        /// Gets the parameter along the curve which coincides with a given length along the curve.
        /// </summary>
        /// <param name="segmentLength">Length of segment to measure. Must be less than or equal to the length of the curve.</param>
        /// <param name="tolerance">If set less or equal 0.0, the tolerance used is 1e-10.</param>
        /// <returns></returns>
        public double ParameterAtLength(double segmentLength, double tolerance = -1.0) =>
            Analyze.RationalCurveParameterAtLength(this, segmentLength, tolerance);

        /// <summary>
        /// Get the length curve which coincides with a given parameter t.
        /// </summary>
        /// <param name="t">The parameter at which to evaluate.</param>
        /// <returns>The length of the curve at the give parameter t.</returns>
        public double LengthParameter(double t) => Analyze.RationalCurveArcLength(this, t);

        /// <summary>
        /// Compare if two NurbsCurves are the same.
        /// Two NurbsCurve are equal when the have same degree, same control points order and dimension, and same knots.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Return true if the NurbsCurves are equal.</returns>
        public bool Equals(NurbsCurve? other)
        {
            var pts = this.ControlPoints;
            var otherPts = other?.ControlPoints;

            if (other == null) return false;
            if (pts.Count != otherPts.Count) return false;
            if (this.Knots.Count != other.Knots.Count) return false;
            if (pts.Where((t, i) => !t.Equals(otherPts[i])).Any())
            {
                return false;
            }

            return Degree == other.Degree && Knots.All(other.Knots.Contains) && Weights.All(other.Weights.Contains);
        }

        /// <summary>
        /// Implement the override method to string.
        /// </summary>
        /// <returns>The representation of a NurbsCurve in string.</returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var controlPts = string.Join("\n", this.ControlPoints.Select(first => $"({string.Join(",", first)})"));
            var knots = $"Knots = ({string.Join(",", this.Knots)})";
            var degree = $"CurveDegree = {Degree}";

            stringBuilder.AppendLine(controlPts);
            stringBuilder.AppendLine(knots);
            stringBuilder.AppendLine(degree);

            return stringBuilder.ToString();
        }
    }
}

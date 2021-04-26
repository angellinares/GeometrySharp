﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeometrySharp.Core;
using GeometrySharp.ExtendedMethods;
using GeometrySharp.Geometry;

namespace GeometrySharp.Operation
{
    /// <summary>
    /// Modify contains many fundamental algorithms for working with NURBS. These include algorithms for:
    /// knot insertion, knot refinement, degree elevation, reparameterization.
    /// Many of these algorithms owe their implementation to Piegl & Tiller's, "The NURBS Book".
    /// </summary>
    public class Modify
    {
        /// <summary>
		/// Insert a collection of knots on a curve.
		/// Implementation of Algorithm A5.4 of The NURBS Book by Piegl & Tiller, 2nd Edition.
		/// </summary>
		/// <param name="curve">The NurbsCurve object.</param>
		/// <param name="knotsToInsert">The set of Knots.</param>
		/// <returns>A NurbsCurve with refined knots.</returns>
		public static NurbsCurve CurveKnotRefine(NurbsCurve curve, List<double> knotsToInsert)
        {
            if (knotsToInsert.Count == 0)
                return new NurbsCurve(curve);

            int degree = curve.Degree;
            List<Vector3> controlPoints = curve.ControlPoints;
            Knot knots = curve.Knots;

            // Initialize common variables.
            int n = controlPoints.Count - 1;
            int m = n + degree + 1;
            int r = knotsToInsert.Count - 1;
            int a = knots.Span(degree, knotsToInsert[0]);
            int b = knots.Span(degree, knotsToInsert[r]);
            Vector3[] controlPointsPost = new Vector3[n + r + 2];
            double[] knotsPost = new double[m + r + 2];

            // New control points.
            for (int i = 0; i < a - degree + 1; i++)
                controlPointsPost[i] = controlPoints[i];
            for (int i = b - 1; i < n + 1; i++)
                controlPointsPost[i + r + 1] = controlPoints[i];

            // New knot vector.
            for (int i = 0; i < a + 1; i++)
                knotsPost[i] = knots[i];
            for (int i = b + degree; i < m + 1; i++)
                knotsPost[i + r + 1] = knots[i];

            // Initialize variables for knot refinement.
            int g = b + degree - 1;
            int k = b + degree + r;
            int j = r;

            // Apply knot refinement.
            while (j >= 0)
            {
                while (knotsToInsert[j] <= knots[g] && g > a)
                {
                    controlPointsPost[k - degree - 1] = controlPoints[g - degree - 1];
                    knotsPost[k] = knots[g];
                    --k;
                    --g;
                }

                controlPointsPost[k - degree - 1] = controlPointsPost[k - degree];

                for (int l = 1; l < degree + 1; l++)
                {
                    int ind = k - degree + l;
                    double alfa = knotsPost[k + l] - knotsToInsert[j];

                    if (Math.Abs(alfa) < GeoSharpMath.EPSILON)
                        controlPointsPost[ind - 1] = controlPointsPost[ind];
                    else
                    {
                        alfa /= (knotsPost[k + l] - knots[g - degree + l]);
                        controlPointsPost[ind - 1] = (controlPointsPost[ind - 1] * alfa) + (controlPointsPost[ind] * (1.0 - alfa));
                    }
                }
                knotsPost[k] = knotsToInsert[j];
                --k;
                --j;
            }
            return new NurbsCurve(degree, knotsPost.ToKnot(), controlPointsPost.ToList());
        }

        /// <summary>
        /// Decompose a NurbsCurve into a collection of bezier's.  Useful
        /// as each bezier fits into it's convex hull.  This is a useful starting
        /// point for intersection, closest point, divide & conquer algorithms
        /// </summary>
        /// <param name="curve">NurbsCurve object representing the curve</param>
        /// <returns>List of NurbsCurve objects, defined by degree, knots, and control points</returns>
        public static List<NurbsCurve> DecomposeCurveIntoBeziers(NurbsCurve curve)
        {
            var degree = curve.Degree;
            var controlPoints = curve.ControlPoints;
            var knots = curve.Knots;

            // Find all of the unique knot values and their multiplicity.
            // For each, increase their multiplicity to degree + 1.
            var knotMultiplicities = knots.Multiplicities();
            var reqMultiplicity = degree + 1;

            // Insert the knots.
            foreach (var (key, value) in knotMultiplicities)
            {
                if (value < reqMultiplicity)
                {
                    var knotsToInsert = Sets.RepeatData(key, reqMultiplicity - value);
                    var curveTemp = new NurbsCurve(degree, knots, controlPoints);
                    var curveResult = CurveKnotRefine(curveTemp, knotsToInsert);
                    knots = curveResult.Knots;
                    controlPoints = curveResult.ControlPoints;
                }
            }

            var crvKnotLength = reqMultiplicity * 2;
            var curves = new List<NurbsCurve>();
            var i = 0;

            while (i < controlPoints.Count)
            {
                var knotsRange = knots.GetRange(i, crvKnotLength).ToKnot();
                var ptsRange = controlPoints.GetRange(i, reqMultiplicity);

                var tempCrv = new NurbsCurve(degree, knotsRange, ptsRange);
                curves.Add(tempCrv);
                i += reqMultiplicity;
            }

            return curves;
        }

        /// <summary>
        /// Transform a NurbsCurve using a matrix.
        /// </summary>
        /// <param name="curve">The curve to transform.</param>
        /// <param name="mat">The matrix to use for the transform - the dimensions should be the dimension of the curve + 1 in both directions.</param>
        /// <returns>A new NurbsCurve after transformation.</returns>
        public static NurbsCurve RationalCurveTransform(NurbsCurve curve, Matrix mat)
        {
            var pts = curve.ControlPoints;
            for (int i = 0; i < pts.Count; i++)
            {
                var pt = pts[i];
                pt.Add(1.0);
                pts[i] = (pt * mat).Take(pt.Count - 1).ToVector();
            }

            return new NurbsCurve(curve.Degree, curve.Knots, pts, curve.Weights!);
        }

        /// <summary>
        /// Reverses the parametrization of a NurbsCurve. The domain is unaffected.
        /// </summary>
        /// <param name="curve">The NurbsCurve has to be reversed.</param>
        /// <returns>A NurbsCurve with a reversed parametrization.</returns>
        public static NurbsCurve ReverseCurve(NurbsCurve curve)
        {
            var pts = curve.ControlPoints;
            pts.Reverse();

            var weights = curve.Weights;
            weights.Reverse();

            var knots = Knot.Reverse(curve.Knots);

            return new NurbsCurve(curve.Degree, knots, pts, weights);
        }

        /// <summary>
        /// Perform knot refinement on a NURBS surface by inserting knots at various parameters
        /// </summary>
        /// <param name="nurbsSurface">The surface to insert the knots into</param>
        /// <param name="knots">The knots to insert - an array of parameter positions within the surface domain</param>
        /// <param name="useU">Whether to insert in the U direction or V direction of the surface. U is default</param>
        /// <returns></returns>
        public static NurbsSurface SurfaceKnotRefine(NurbsSurface nurbsSurface, Knot knotsToInsert, bool useU = true)
        {
            List<List<Vector3>> ctrlPts = new List<List<Vector3>>();
            List<List<Vector3>> refinedPts = new List<List<Vector3>>();
            Knot knots = new Knot();
            int degree = -1;

            //u dir
            if (useU)
            {
                ctrlPts = nurbsSurface.ControlPoints;
                degree = nurbsSurface.DegreeU;
                knots = nurbsSurface.KnotsU;
            }
            //v dir
            else
            {
                //Reverse the points matrix
                ctrlPts = ReverseControlPoints2dMatrix(nurbsSurface.ControlPoints);
                degree = nurbsSurface.DegreeV;
                knots = nurbsSurface.KnotsV;
            }

            //Do knot refinement on every row
            NurbsCurve crv = new NurbsCurve();
            foreach (var cptRow in ctrlPts)
            {
                crv = CurveKnotRefine(new NurbsCurve(degree, knots, cptRow), knotsToInsert);
                refinedPts.Add(crv.ControlPoints);
            }

            Knot newKnots = crv.Knots;
            if (useU)
                return new NurbsSurface(nurbsSurface.DegreeU, nurbsSurface.DegreeV, newKnots, nurbsSurface.KnotsV, ReverseControlPoints2dMatrix(refinedPts));
            else
                return new NurbsSurface(nurbsSurface.DegreeU, nurbsSurface.DegreeV, nurbsSurface.KnotsU, newKnots, refinedPts);

        }

        /// <summary>
        /// Reverse a 2D matrix composed by control points
        /// </summary>
        /// <param name="ctrlPts"></param>
        /// <returns></returns>
        private static List<List<Vector3>> ReverseControlPoints2dMatrix(List<List<Vector3>> ctrlPts)
        {
            List<List<Vector3>> reversedPts = new List<List<Vector3>>();
            //Reverse the points matrix
            if (ctrlPts.Count == 0)
            {
                return null;
            }

            int rows = ctrlPts.Count;
            int columns = ctrlPts[0].Count;
            for (int c = 0; c < columns; c++)
            {
                List<Vector3> rr = new List<Vector3>();
                for (int r = 0; r < rows; r++)
                {
                    rr.Add(ctrlPts[r][c]);
                }
                reversedPts.Add(rr);
            }

            return reversedPts;
        }
    }
}

﻿using FluentAssertions;
using GeometrySharp.Core;
using GeometrySharp.ExtendedMethods;
using GeometrySharp.Geometry;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace GeometrySharp.Test.XUnit.Geometry
{
    public class LineTests
    {
        private readonly ITestOutputHelper _testOutput;
        public LineTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public static Line ExampleLine => new Line(new Vector3 {5, 0, 0}, new Vector3 {15, 15, 0});

        public static TheoryData<Line> DataLine => new TheoryData<Line> {ExampleLine};

        [Fact]
        public void It_Returns_A_Line()
        {
            Vector3 p1 = new Vector3 {-0.913, 1.0, 4.68};
            Vector3 p2 = new Vector3 {6.363, 10.0, 7.971};
            Line l = new Line(p1, p2);
            
            l.Should().NotBeNull();
            l.Start.All(p1.Contains).Should().BeTrue();
        }

        [Fact]
        public void It_Throws_An_Exception_If_Inputs_Are_Not_Valid_Or_Equals()
        {
            Func<Line> func = () =>  new Line(new Vector3{5,5,0}, new Vector3 { 5, 5, 0 });
            Func<Line> func1 = () => new Line(new Vector3 { 5, 5, 0 }, Vector3.Unset);

            func.Should().Throw<Exception>();
            func1.Should().Throw<Exception>().WithMessage("Inputs are not valid, or are equal");
        }

        [Fact]
        public void It_Returns_A_Line_By_A_Starting_Point_Direction_Length()
        {
            Vector3 startingPoint = new Vector3 {0, 0, 0};

            Line line1 = new Line(startingPoint, Vector3.XAxis, 15);
            Line line2 = new Line(startingPoint, Vector3.XAxis, -15);

            line1.Length.Should().Be(line2.Length).And.Be(15);
            line1.Start.Should().BeEquivalentTo(line2.Start).And.BeEquivalentTo(startingPoint);

            line1.Direction.Should().BeEquivalentTo(new Vector3 {1, 0, 0});
            line1.End.Should().BeEquivalentTo(new Vector3 { 15, 0, 0 });

            line2.Direction.Should().BeEquivalentTo(new Vector3 { -1, 0, 0 });
            line2.End.Should().BeEquivalentTo(new Vector3 { -15, 0, 0 });
        }

        [Fact]
        public void It_Throws_An_Exception_If_Length_Is_Zero()
        {
            Vector3 startingPoint = new Vector3 { 0, 0, 0 };

            Func<Line> func = () => new Line(startingPoint, Vector3.XAxis, 0);

            func.Should().Throw<Exception>().WithMessage("Length must not be 0.0");
        }

        [Fact]
        public void It_Returns_The_Length_Of_The_Line()
        {
            Vector3 p1 = new Vector3 { -0.913, 1.0, 4.68 };
            Vector3 p2 = new Vector3 { 6.363, 10.0, 7.971 };
            Line l = new Line(p1, p2);
            double expectedLength = 12.03207;

            l.Length.Should().BeApproximately(expectedLength, 5);
        }

        [Fact]
        public void It_Returns_The_Line_Direction()
        {
            Vector3 p1 = new Vector3 { 0, 0, 0 };
            Vector3 p2 = new Vector3 { 5, 0, 0 };
            Line l = new Line(p1, p2);
            Vector3 expectedDirection = new Vector3 { 1, 0, 0 };

            l.Direction.Should().BeEquivalentTo(expectedDirection);
        }

        [Fact]
        public void It_Returns_The_ClosestPoint()
        {
            Line line = new Line(new Vector3{ 0, 0, 0 }, new Vector3{ 30, 45, 0 });
            Vector3 pt = new Vector3{ 10, 20, 0 };
            Vector3 expectedPt = new Vector3{ 12.30769230769231, 18.461538461538463, 0 };

            Vector3 closestPt = line.ClosestPoint(pt);

            closestPt.Should().BeEquivalentTo(expectedPt);
        }

        [Theory]
        [MemberData(nameof(DataLine))]
        public void PointAt_Throw_An_Exception_If_Parameter_Outside_The_Curve_Domain(Line line)
        {
            Func<Vector3> func = () => line.PointAt(2);

            func.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Parameter is outside the domain 0.0 to 1.0 *");
        }

        // Values compared with Rhino.
        [Theory]
        [InlineData(0.0, new[] {5.0, 0, 0})]
        [InlineData(0.15, new[] {6.5, 2.25, 0})]
        [InlineData(0.36, new[] {8.6, 5.4, 0})]
        [InlineData(0.85, new[] {13.5, 12.75, 0})]
        [InlineData(1.0, new[] {15.0, 15.0, 0})]
        public void It_Returns_The_Evaluated_Point_At_The_Given_Parameter(double t, double[] ptExpected)
        {
            Vector3 ptEvaluated = ExampleLine.PointAt(t);

            ptEvaluated.Equals(ptExpected.ToVector()).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_A_Flipped_Line()
        {
            Line flippedLine = ExampleLine.Flip();

            flippedLine.Start.Equals(ExampleLine.End).Should().BeTrue();
            flippedLine.End.Equals(ExampleLine.Start).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_An_Extend_Line()
        {
            Line extendedLine = ExampleLine.Extend(0, -5);

            extendedLine.Length.Should().BeApproximately(13.027756, GeoSharpMath.MAXTOLERANCE);
            extendedLine.Start.Should().BeEquivalentTo(ExampleLine.Start);
        }

        [Fact]
        public void It_Checks_If_Two_Lines_Are_Equals()
        {
            Line lineFlip = ExampleLine.Flip();
            Line lineFlippedBack = lineFlip.Flip();

            lineFlip.Equals(lineFlippedBack).Should().BeFalse();
            lineFlippedBack.Equals(ExampleLine).Should().BeTrue();
        }

        [Fact]
        public void It_Returns_A_Transformed_Line()
        {
            Vector3 translatedVec = new Vector3{10,10,0};
            Transform transform = Transform.Translation(translatedVec);

            Line transformedLine = ExampleLine.Transform(transform);

            transformedLine.Start.Should().BeEquivalentTo(new Vector3 {15, 10, 0});
            transformedLine.End.Should().BeEquivalentTo(new Vector3 { 25, 25, 0 });
        }

        [Fact]
        public void It_Returns_A_NurbsCurve_From_The_Line()
        {
            Vector3 p1 = new Vector3 { 0.0, 0.0, 0.0 };
            Vector3 p2 = new Vector3 { 2.0, 0.0, 0.0 };
            Line l = new Line(p1, p2);

            NurbsCurve curve = l.ToNurbsCurve();

            curve.ControlPoints.Count.Should().Be(2);
            curve.Degree.Should().Be(1);
            curve.ControlPoints[0].Should().BeEquivalentTo(p1);
            curve.ControlPoints[1].Should().BeEquivalentTo(p2);
        }
    }
}

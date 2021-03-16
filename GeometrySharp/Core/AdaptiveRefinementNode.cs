using GeometrySharp.Geometry;
using GeometrySharp.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometrySharp.Core
{
    /// <summary>
    /// Structure of the child nodes
    /// in the adaptive refinement tree
    /// 
    ///   v
    ///   ^
    ///   |
    ///   +--> u
    /// 
    ///                         neighbors[2]
    /// 
    ///                 (u0,v1)---(u05,v1)---(u1,v1)
    ///                   |           |          |
    ///                   |     3     |     2    |
    ///                   |           |          |
    /// neighbors[3]   (u0,v05)--(u05,v05)--(u1,v05)   neighbors[1]
    ///                   |           |          |
    ///                   |     0     |     1    |
    ///                   |           |          |
    ///                 (u0,v0)---(u05,v0)---(u1,v0)
    /// 
    ///                         neighbors[0]
    /// </summary>
    public class AdaptiveRefinementNode
    {
        public NurbsSurface NurbsSurface { get; set; }
        public List<AdaptiveRefinementNode> Children { get; set; }
        public List<AdaptiveRefinementNode> Neighbors { get; set; }
        public List<Vector3> Corners { get; set; }
        public List<Vector3> MidPoints { get; set; }
        public Vector3 CenterPoint { get; set; }
        public bool SplitVert { get; set; }
        public bool SplitHoriz { get; set; }
        public bool Horizontal { get; set; }
        public double U05 { get; set; }
        public double V05 { get; set; }

        public bool IsLeaf => this.Children == null;
        public Vector3 Center => this.CenterPoint != null ? this.CenterPoint : EvaluateSurface(this.U05, this.V05);

        public AdaptiveRefinementNode(NurbsSurface nurbsSurface, List<Vector3> corners, List<AdaptiveRefinementNode> neighbors = null)
        {
            this.NurbsSurface = nurbsSurface;
            this.Neighbors = neighbors == null ? new List<AdaptiveRefinementNode>() { null, null, null, null } : neighbors;
            this.Corners = corners;

            if (this.Corners == null)
                this.Corners = nurbsSurface.Corners;
            /// to be implemented ===========================================

        }

        public Vector3 EvaluateSurface(double u, double v, Vector3 srfPt = null)
        {
            var derivs = Evaluation.RationalSurfaceDerivatives(this.NurbsSurface, u, v);
            var pt = derivs[0][0];
            var norm = Vector3.Cross(derivs[0][1], derivs[1][0]);
            if (!norm.IsZero())
                norm = norm.Unitize();
            ///return new Vector3();
            /// to be implemented ===========================================
        }

    }
}

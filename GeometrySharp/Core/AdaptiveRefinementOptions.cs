using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeometrySharp.Core
{
    public class AdaptiveRefinementOptions
    {
        public double NormTol { get; set; } = 2.5e-2;
        public int MinDepth { get; set; } = 0;
        public int MaxDepth { get; set; } = 10;
        public bool Refine { get; set; } = true;
        public int MinDivsU => 1;
        public int MinDivsV => 1;

    }
}

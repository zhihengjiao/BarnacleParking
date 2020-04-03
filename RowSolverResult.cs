using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Barnacle
{
    [Serializable]
    public class RowSolverResult : SolverResult
    {
        // public List<RowNode> result;
        // public Metric metric;
        public double totalWidth;

        public RowSolverResult() : this(new List<RowNode>())
        { }
        public RowSolverResult(List<RowNode> result) : base(result)
        {
            this.totalWidth = 0;
        }

        public void Add(RowNode rowNode)
        {
            result.Add(rowNode);
            totalWidth += rowNode.GetWidth();
        }



        public override List<GeometryBase> Draw()
        {
            List<GeometryBase> list = new List<GeometryBase>();

            foreach (RowNode node in result)
            {
          
                list.Add(node.referenceLine.ToNurbsCurve());

            }
            return list;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        
    }

}

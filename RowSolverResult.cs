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
        public RowNode endNode;

        public RowSolverResult() : this(new List<RowNode>())
        { }
        public RowSolverResult(List<RowNode> result) : base(result)
        {
            this.totalWidth = 0;
        }

        public RowSolverResult Clone()
        {
            RowSolverResult res = new RowSolverResult();
            res.totalWidth = this.totalWidth;
            res.endNode = this.endNode;
            return res;
        }

        public void Add(RowNode rowNode)
        {
            // result.Add(rowNode);
            endNode = rowNode;
            totalWidth += rowNode.GetWidth();
        }

        public void StepBack()
        {
            totalWidth -= endNode.GetWidth();
            endNode = endNode.prev;
            
        }

      public double CalculateTotalStall()
        {
            RowNode node = endNode;
            double res = 0;
            while (node != null)
            {
                res += node.GetLineLength() / node.metaItem.GetWidth();
                node = node.prev;
            }
            return res;
        }

        public override List<GeometryBase> Draw()
        {
            List<GeometryBase> list = new List<GeometryBase>();
            RowNode node = endNode;
            while (node != null)
            {
                    
                list.AddRange(node.Draw());
                node = node.prev;

            }
            return list;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        
    }

}

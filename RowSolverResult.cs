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
    public class RowSolverResult : SolverResult, IComparable
    {
        // public List<RowNode> result;
        // public Metric metric;
        public double totalWidth;
        public RowNode endNode;
        public double totalStall = 0;

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

#pragma warning disable CS0184 // 'is' expression's given expression is never of the provided type
                if (node.GetType() == typeof(CarStallMeta))
#pragma warning restore CS0184 // 'is' expression's given expression is never of the provided type
                {
                    CarStallMeta meta = (CarStallMeta)node.metaItem;
                    res += node.GetLineLength() / meta.GetLength();
                }


                Rhino.RhinoApp.WriteLine(node.ToString());
                node = node.prev;
                
            }
            this.totalStall = res;
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

        public new int CompareTo(object obj)
        {
            if (!(obj is RowSolverResult))
            {
                throw new ArgumentException();
            }
            RowSolverResult other = (RowSolverResult)obj;
            return (int) (this.totalStall - other.totalStall);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        
    }

}

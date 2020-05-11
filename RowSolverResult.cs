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
        public int singleRowNum = 0;

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
            res.singleRowNum = this.singleRowNum;
            return res;
        }

        public void Add(RowNode rowNode)
        {
            // result.Add(rowNode);
            endNode = rowNode;
            totalWidth += rowNode.GetClearHeight();
             if (!rowNode.metaItem.IsDouble() && (rowNode.metaItem.Type() == "car"))
            {
                this.singleRowNum++;
            }
        }

        public void StepBack()
        {
            totalWidth -= endNode.GetClearHeight();
            endNode = endNode.prev;
            
        }

      public double CalculateTotalStall()
        {
            RowNode node = endNode;
            double res = 0;
            while (node != null)
            {

                if (node.name.Equals("CarStallRow"))
                {
                    CarStallMeta meta = (CarStallMeta)node.metaItem;
                    int multi = meta.IsDouble() ? 2 : 1;
                    res += (node.GetLineLength() / meta.GetClearLength()) * multi;
                }


                //Rhino.RhinoApp.WriteLine(node.ToString());
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

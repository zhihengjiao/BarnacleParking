using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barnacle
{
    class BoundarySolverResult : SolverResult, IComparable
    {

        public List<RowNode> list;
        int totalCount = 0;


        public BoundarySolverResult() : this(new List<RowNode>())
        { }
        public BoundarySolverResult(List<RowNode> list) : base(list)
        {
            this.list = new List<RowNode>();
        }

        public new void Add(RowNode node)
        {
            if (node != null)
            {
                list.Add(node);

            }
        }

        public BoundarySolverResult Clone()
        {
            BoundarySolverResult res = new BoundarySolverResult();
            res.list = new List<RowNode>(list);
            res.totalCount = this.totalCount;
            return res;
        }

        public override List<GeometryBase> Draw()
        {
            List<GeometryBase> list = new List<GeometryBase>();

            foreach (RowNode node in this.list)
            {
                list.AddRange(node.Draw());
            }
            return list;
        }

        public List<GeometryBase> Out()
        {
            List<GeometryBase> list = new List<GeometryBase>();

            foreach (RowNode node in this.list)
            {
                list.Add(node.highLine.ToNurbsCurve());
            }
            return list;
        }
    }
}

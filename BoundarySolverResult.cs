using Rhino.Geometry;
using Rhino.Geometry.Intersect;
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
        public int totalCount = 0;


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

        public void RemoveLast()
        {
            this.list.RemoveAt(this.list.Count - 1);
        }

        public void ChangeToInnerLine()
        {
            List<RowNode> replaceList = new List<RowNode>();
            for (int i = 0; i < this.list.Count; i++)
            {
                RowNode node = list[i];
                // intersect
                double leftParam;
                double thisParam;
                double rightParam;
                Intersection.LineLine(node.highLine, list[(i + 1) % list.Count].highLine, out leftParam, out thisParam);
                Intersection.LineLine(node.highLine, list[(i + list.Count - 1) % list.Count].highLine, out rightParam, out thisParam);
                Line newRefLine = new Line(node.highLine.PointAt(leftParam), node.highLine.PointAt(rightParam));
                Vector3d moveBack = new Vector3d(node.zone.offsetDirection[i]);
                moveBack.Reverse();
                moveBack.Unitize();
                newRefLine.Transform(Transform.Translation(moveBack * node.metaItem.GetClearHeight()));

                RowNode replaceNode = new CarStallRow(
                    i,
                    newRefLine,
                    (CarStallMeta)node.metaItem,
                    node.zone
                    );
                replaceList.Add(replaceNode);
            }
            this.list = replaceList;
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

            for (int i = 0; i < this.list.Count; i++)
            {
                RowNode node = this.list[i];
                Line innerLine = node.zone.OffsetInZone(node.highLine, i, RoadMeta.NORMAL_ROAD.GetClearHeight());
                list.Add(innerLine.ToNurbsCurve());
            }
            return list;
        }

        public int CalculateTotalStall()
        {
            int res = 0;
            for (int i = 0; i < this.list.Count; i++)
            {
                RowNode node = this.list[i];
                if (node.name.Equals("CarStallRow"))
                {
                    CarStallMeta meta = (CarStallMeta)node.metaItem;
                    int multi = meta.IsDouble() ? 2 : 1;
                    res +=  (int) (node.GetLineLength() / meta.GetClearLength()) * multi;
                }
            }
            this.totalCount = res;

            return res;
        }
    }
}
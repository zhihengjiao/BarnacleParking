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
    public class RowSolver : Solver
    {
        public SortedSet<RowSolverResult> resultRepository;
        public Zone zone;
        public RowSolver(Metric metric, RowSolverResult RowSolverResult) : base(metric, RowSolverResult)
        {
            resultRepository = new SortedSet<RowSolverResult>();
        }

        public RowSolver WithZone(Zone newZone)
        {
            this.zone = newZone;
            return this;
        }

        public static Line midLine(Line a, Line b)
        {
            /*
            Point3d start = new Point3d(
                (a.FromX + b.FromX) / 2,
                (a.FromY + b.FromY) / 2,
                (a.FromZ + b.FromZ) / 2);
            Point3d end = new Point3d(
                (a.ToX + b.ToX) / 2,
                (a.ToY + b.ToY) / 2,
                (a.ToZ + b.ToZ) / 2);
             */
            if (a.Length > b.Length)
            {
                return midLine(b, a);
            }
            Point3d midFrom = a.PointAt(0.5);
            Point3d midTo = b.ClosestPoint(midFrom, true);
            Vector3d dir = new Vector3d(
              (midTo.X - midFrom.X)/2,
              (midTo.Y - midFrom.Y)/2,
              (midTo.Z - midFrom.Z)/2);
            Line mid = new Line(a.From, a.To);
            mid.Transform(Transform.Translation(dir));

            return mid;
        }

        public void Solve()
        {
            // for each ref edge
            for (int i = 0; i < zone.edges.Length; i++)
            {
                // permute
                // init
                Line refLine = zone.edges[i];
                double offsetBound = zone.maxOffsetLength[i];
                List<List<IMeta>> permutation = new List<List<IMeta>>();
                List<Line> rows = new List<Line>();
                List<double> totalWidth = new List<double>();
                double curOffset = 0;

                while (offsetBound < curOffset + RoadMeta.NORMAL_ROAD.GetWidth() + C)
                    // first row
                    for (int c = 0; c < CarStallMeta.META_LIST.Length; c++)
                {
                    CarStallMeta firstRow = CarStallMeta.META_LIST[c];
                    if (firstRow.GetWidth() > offsetBound)
                    {
                        break;
                    } else
                    {
                        // getting the middle line
                        Line highLine = zone.OffsetInZone(refLine, i, firstRow.GetWidth());
                        Line lowLine = refLine;
                        Line middleLine = midLine(lowLine, highLine);
                        // add to list
                        rows.Add(middleLine);
                        permutation.Add(firstRow);
                    }
                }
                // more rows
                
                for (int c = 0; c < CarStallMeta.META_LIST.Length; c++)
                {

                }
            }
        }

        /*
        public new SortedSet<RowSolverResult> Solve()
        {
            for (int i = 0; i < zone.edges.Length; i++)
            {
                DFS(i);
            }
            return resultRepository;
        }
        */

        void DFS(int baseLineID)
        {
            CarStallRow c = new CarStallRow(baseLineID,
                zone.edges[baseLineID],
                CarStallMeta.NINETY_DEGREE);
            RoadRow r = new RoadRow(baseLineID,
                zone.edges[baseLineID],
                RoadMeta.NORMAL_ROAD);

            // create new result and metric
            RowSolverResult branchC = (RowSolverResult) this.SOLVER_RESULT_FACTORY.CreateNew();
            RowSolverResult branchR = (RowSolverResult) this.SOLVER_RESULT_FACTORY.CreateNew();
            Metric mC = this.METRIC_FACTORY.CreateNew();
            Metric mR = this.METRIC_FACTORY.CreateNew();
            SetMetricAndResult(mC, branchC);
            SetMetricAndResult(mR, branchR);


            // RowNode startC = GrowNode(null, c, branchC);
            // RowNode startR = GrowNode(null, r, branchR);
            RowNode startC = c;
            RowNode startR = r;
            branchC.Add(startC);
            branchR.Add(startR);

            Grow(startC, branchC, baseLineID);
            Grow(startR, branchR, baseLineID);

            
        }


        RowNode GrowNode(RowNode node, RowNode newNode, RowSolverResult branch)
        {
            Console.WriteLine("GrowNode\n");
            // node is Road && newNode is CarStall
            // For adding connection to newNode
            if (node != null)
            {
                if (node is RoadRow && newNode is CarStallRow)
                {
                    newNode = (CarStallRow)newNode;
                    newNode.AddConnection();
                }
            }
            newNode.prev = node;

            // add new Node
            branch.Add(newNode);

            return newNode;
        }

        bool IsLegalAddition(RowNode node)
        {
            if (node is CarStallRow)
            {
                if (!node.prev.IsConnectedToRoadRow())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        void Grow(RowNode node, RowSolverResult branch, int baseLineID)
        {
            foreach (CarStallMeta carMeta in CarStallMeta.META_LIST)
            {
                // If exceed the maxWidth(base case)
                if (branch.totalWidth + carMeta.GetWidth() >= zone.maxOffsetLength[baseLineID])
                {
                    if (!IsLegalAddition(node))
                    {
                        continue;
                    }
                    else
                    {
                        resultRepository.Add(branch);
                        if (resultRepository.Count >= BEST_RESULT_NUMBER)
                        {
                            resultRepository.Remove(resultRepository.Min);
                        }
                        continue;
                    }
                }

                // recursive case
                // if node.referenceLine is None:
                // self.resultRepository.append(branch)
                // return

                // if this node is CarStall
                if (node is CarStallRow)
                {
                    // not connected to Road
                    if (!node.IsConnectedToRoadRow())
                    {
                        RoadRow r = new RoadRow(
                            baseLineID,
                            zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                            RoadMeta.NORMAL_ROAD
                            );

                        RowSolverResult newBranch = Copy.DeepClone(branch);
                        RowNode newNode = GrowNode(node, r, newBranch);
                        Grow(newNode, newBranch, baseLineID);
                    }
                    // if connected to Road
                    else
                    {
                        CarStallRow c = new CarStallRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                carMeta
                                );

                        RoadRow r = new RoadRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                RoadMeta.NORMAL_ROAD
                                );

                        // Grow a Car Branch
                        RowSolverResult carBranch = Copy.DeepClone(branch);
                        RowNode carNode = GrowNode(node, c, carBranch);
                        Grow(carNode, carBranch, baseLineID);

                        // Grow a Road Branch
                        RowSolverResult roadBranch = Copy.DeepClone(branch);
                        RowNode roadNode = GrowNode(node, c, roadBranch);
                        Grow(roadNode, roadBranch, baseLineID);
                    }
                }
                // if this node is a Road
                else if (node is RoadRow)
                {
                    CarStallRow c = new CarStallRow(
                                baseLineID,
                                zone.OffsetInZone(node.referenceLine, baseLineID, node.GetWidth()),
                                carMeta
                                );
                    RowSolverResult newBranch = Copy.DeepClone(branch);
                    RowNode newNode = GrowNode(node, c, newBranch);
                    Grow(newNode, newBranch, baseLineID);

                }
            }
        }

        public RowSolverResult GetBest()
        {
            double max = 0;
            RowSolverResult res = resultRepository.Max;
            foreach (RowSolverResult cur in resultRepository)
            {
                if (cur.totalWidth > max)
                {
                    res = cur;
                    max = cur.totalWidth;
                }
            }
            return res;
        }


    }

}

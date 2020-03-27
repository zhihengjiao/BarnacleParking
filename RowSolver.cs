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
        public new List<RowSolverResult> resultRepository;
        public Zone zone;
        public RowSolver(Metric metric, SolverResult solverResult) : base(metric, solverResult)
        {

        }

        public RowSolver WithZone(Zone newZone)
        {
            this.zone = newZone;
            return this;
        }

        public new List<RowSolverResult> Solve()
        {
            for (int i = 0; i < zone.edges.Length; i++)
            {
                DFS(i);
            }
            return resultRepository;
        }

        void DFS(int baseLineID)
        {
            CarStallRow c = new CarStallRow(baseLineID,
                zone.edges[baseLineID],
                CarStallMeta.NINETY_DEGREE);
            RoadRow r = new RoadRow(baseLineID,
                zone.edges[baseLineID],
                RoadMeta.NORMAL_ROAD);

            // create new result and metric
            SolverResult branchC = this.SOLVER_RESULT_FACTORY.CreateNew();
            SolverResult branchR = this.SOLVER_RESULT_FACTORY.CreateNew();
            Metric mC = this.METRIC_FACTORY.CreateNew();
            Metric mR = this.METRIC_FACTORY.CreateNew();
            SetMetricAndResult(mC, branchC);
            SetMetricAndResult(mR, branchR);

            RowNode startC = GrowNode(null, c, branchC);
            RowNode startR = GrowNode(null, r, branchR);
        }

        RowNode GrowNode(RowNode node, RowNode newNode, SolverResult branch)
        {
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
            RowSolverResult res = resultRepository[0];
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

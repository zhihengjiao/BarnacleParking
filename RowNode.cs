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
    public abstract class RowNode
    {
        public RowNode next;
        public RowNode prev;
        public IMeta metaItem;
        public Line referenceLine;
        public int baseLineID;

        public RowNode(int baseLineID, Line referenceLine, IMeta metaItem)
        {
            this.baseLineID = baseLineID;
            this.referenceLine = referenceLine;
            this.metaItem = metaItem;
            next = null;
            prev = null;
        }

        public double GetWidth()
        {
            return metaItem.GetWidth();
        }

        public int GetBaseLineID()
        {
            return baseLineID;
        }

        public Line GetReferenceLine()
        {
            return referenceLine;
        }

        public double GetLineLength()
        {
            return referenceLine.Length;
        }

        public abstract void AddConnection();

        public abstract bool IsConnectedToRoadRow();
    }


    [Serializable]
    class CarStallRow : RowNode
    {
        int requiredConnection;
        public CarStallRow(int baseLineID, Line referenceLine, CarStallMeta metaItem) :
            base(baseLineID, referenceLine, metaItem)
        {
            requiredConnection = metaItem.RequiredConnection();

        }

        public override void AddConnection()
        {
            requiredConnection -= 1;
        }

        public override bool IsConnectedToRoadRow()
        {
            return requiredConnection <= 0;
        }



        public override string ToString()
        {
            return "CarStallRow";
        }
    }


    [Serializable]
    class RoadRow : RowNode
    {

        public RoadRow(int baseLineID, Line referenceLine, RoadMeta metaItem) :
            base(baseLineID, referenceLine, metaItem)
        { }

        public override void AddConnection()
        {
            throw new NotImplementedException();
        }

        public override bool IsConnectedToRoadRow()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "RoadRow";
        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SparseGrid
{
    public struct AxisVector3Int : IEquatable<AxisVector3Int>
    {
        public Axis Axis;
        public Vector3Int Index;

        public AxisVector3Int(Axis axis, int x, int y, int z)
        {
            Index = new Vector3Int(x, y, z);
            Axis = axis;
        }

        public bool Equals(AxisVector3Int other)
        {
            return Index == other.Index && Axis == other.Axis;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ (Axis.GetHashCode() << 2);
        }
    }
}

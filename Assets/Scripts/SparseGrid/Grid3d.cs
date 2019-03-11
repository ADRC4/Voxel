using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using QuickGraph;
using QuickGraph.Algorithms;
using Debug = UnityEngine.Debug;

namespace SparseGrid
{
    public class Grid3d
    {
        public Dictionary<Vector3Int, Voxel> Voxels = new Dictionary<Vector3Int, Voxel>();
        public Dictionary<Vector3Int, Corner> Corners = new Dictionary<Vector3Int, Corner>();
        public Dictionary<AxisVector3Int, Face> Faces = new Dictionary<AxisVector3Int, Face>();
        public Dictionary<AxisVector3Int, Edge> Edges = new Dictionary<AxisVector3Int, Edge>();


        public float VoxelSize;

        public static Grid3d MakeGridWithBounds(IEnumerable<Bounds> bounds, float voxelSize)
        {
            var grid = new Grid3d(voxelSize);

            foreach (var bound in bounds)
            {
                var min = grid.IndexFromPoint(bound.min);
                var max = grid.IndexFromPoint(bound.max);

                for (int z = min.z; z <= max.z; z++)
                    for (int y = min.y; y <= max.y; y++)
                        for (int x = min.x; x <= max.x; x++)
                        {
                            var index = new Vector3Int(x, y, z);
                            grid.AddVoxel(index);
                        }
            }

            return grid;
        }

        public Grid3d(float voxelSize = 1.0f)
        {
            VoxelSize = voxelSize;
        }

        //public Grid3d Clone()
        //{
        //    return new Grid3d(BBox, VoxelSize);
        //}

        public bool AddVoxel(Vector3Int index)
        {
            if (Voxels.TryGetValue(index, out _)) return false;

            var voxel = new Voxel(index, this);
            Voxels.Add(index, voxel);

            int x = index.x;
            int y = index.y;
            int z = index.z;

            var indices =  new[]
                {
                  new AxisVector3Int(Axis.X, x - 1, y, z),
                  new AxisVector3Int(Axis.X, x + 1, y, z),
                  new AxisVector3Int(Axis.Y, x, y - 1, z),
                  new AxisVector3Int(Axis.Y, x, y + 1, z),
                  new AxisVector3Int(Axis.Z, x, y, z - 1),
                  new AxisVector3Int(Axis.Z, x, y, z + 1),
                };

            foreach(var i in indices)
            {
                if (Faces.TryGetValue(i, out _)) continue;

                var face = new Face(i.Index.x, i.Index.y, i.Index.z, i.Axis, this);
                Faces.Add(i, face);
            }

            return true;
        }

        public Vector3Int IndexFromPoint(Vector3 point)
        {
            point /= VoxelSize;
            point -= Vector3.one * 0.5f;
            return new Vector3Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y), Mathf.RoundToInt(point.z));
        }

        public IEnumerable<Voxel> GetVoxels()
        {
            return Voxels.Select(v => v.Value);
        }

        public IEnumerable<Corner> GetCorners()
        {
            return Corners.Select(v => v.Value);
        }

        public IEnumerable<Face> GetFaces()
        {
            return Faces.Select(v => v.Value);
        }

        public IEnumerable<Edge> GetEdges()
        {
            return Edges.Select(v => v.Value);
        }

        public int GetConnectedComponents()
        {
            var graph = new UndirectedGraph<Voxel, Edge<Voxel>>();
            graph.AddVertexRange(GetVoxels().Where(v => v.IsActive));
            graph.AddEdgeRange(GetFaces().Where(f => f.IsActive).Select(f => new Edge<Voxel>(f.Voxels[0], f.Voxels[1])));

            var components = new Dictionary<Voxel, int>();
            var count = graph.ConnectedComponents(components);
            return count;
        }
    }
}
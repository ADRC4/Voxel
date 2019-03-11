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

        //public Vector3Int Size;
        public float VoxelSize;
        //public Vector3 Corner;
        //public Bounds BBox;

        //public static Grid3d MakeGridWithVoids(IEnumerable<MeshCollider> voids, float voxelSize, bool invert = false)
        //{
        //    var bbox = new Bounds();
        //    foreach (var v in voids.Select(v => v.bounds))
        //        bbox.Encapsulate(v);

        //    var grid = new Grid3d(voxelSize);

        //    return grid;
        //}

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
            //  var watch = Stopwatch.StartNew();

            //  BBox = bbox;
            VoxelSize = voxelSize;

            // bbox.min = new Vector3(bbox.min.x, 0, bbox.min.z);
            // var sizef = bbox.size / voxelSize;
            // Size = new Vector3Int((int)sizef.x, (int)sizef.y, (int)sizef.z);
            //sizef = new Vector3(Size.x, Size.y, Size.z);

            //Corner = bbox.min + (bbox.size - sizef * voxelSize) * 0.5f;

            // make voxels
            //Voxels = new Voxel[Size.x, Size.y, Size.z];

            //for (int z = 0; z < Size.z; z++)
            //    for (int y = 0; y < Size.y; y++)
            //        for (int x = 0; x < Size.x; x++)
            //        {
            //            Voxels[x, y, z] = new Voxel(new Vector3Int(x, y, z), this);
            //        }

            //// make corners
            //Corners = new Corner[Size.x + 1, Size.y + 1, Size.z + 1];

            //for (int z = 0; z < Size.z + 1; z++)
            //    for (int y = 0; y < Size.y + 1; y++)
            //        for (int x = 0; x < Size.x + 1; x++)
            //        {
            //            Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
            //        }

            //// make faces
            //Faces[0] = new Face[Size.x + 1, Size.y, Size.z];

            //for (int z = 0; z < Size.z; z++)
            //    for (int y = 0; y < Size.y; y++)
            //        for (int x = 0; x < Size.x + 1; x++)
            //        {
            //            Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
            //        }

            //Faces[1] = new Face[Size.x, Size.y + 1, Size.z];

            //for (int z = 0; z < Size.z; z++)
            //    for (int y = 0; y < Size.y + 1; y++)
            //        for (int x = 0; x < Size.x; x++)
            //        {
            //            Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
            //        }

            //Faces[2] = new Face[Size.x, Size.y, Size.z + 1];

            //for (int z = 0; z < Size.z + 1; z++)
            //    for (int y = 0; y < Size.y; y++)
            //        for (int x = 0; x < Size.x; x++)
            //        {
            //            Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
            //        }

            //// make edges
            //Edges[2] = new Edge[Size.x + 1, Size.y + 1, Size.z];

            //for (int z = 0; z < Size.z; z++)
            //    for (int y = 0; y < Size.y + 1; y++)
            //        for (int x = 0; x < Size.x + 1; x++)
            //        {
            //            Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
            //        }

            //Edges[0] = new Edge[Size.x, Size.y + 1, Size.z + 1];

            //for (int z = 0; z < Size.z + 1; z++)
            //    for (int y = 0; y < Size.y + 1; y++)
            //        for (int x = 0; x < Size.x; x++)
            //        {
            //            Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
            //        }

            //Edges[1] = new Edge[Size.x + 1, Size.y, Size.z + 1];

            //for (int z = 0; z < Size.z + 1; z++)
            //    for (int y = 0; y < Size.y; y++)
            //        for (int x = 0; x < Size.x + 1; x++)
            //        {
            //            Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
            //        }

            // Debug.Log($"Grid took: {watch.ElapsedMilliseconds} ms to create.\r\nGrid size: {Size}, {Size.x * Size.y * Size.z} voxels.");
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
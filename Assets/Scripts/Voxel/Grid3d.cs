using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using QuickGraph;
using Debug = UnityEngine.Debug;

public enum Axis { X, Y, Z };

public class Grid3d
{
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];

    public Vector3Int Size;
    public float VoxelSize;
    public Vector3 Corner;
    public Bounds BBox;

    // public Mesh[] Mesh;

    public static Grid3d MakeGridWithVoids(IEnumerable<MeshCollider> voids, float voxelSize, bool invert = false)
    {
        var bbox = new Bounds();
        foreach (var v in voids.Select(v => v.bounds))
            bbox.Encapsulate(v);

        var grid = new Grid3d(bbox, voxelSize);
        grid.AddVoids(voids, invert);

        return grid;
    }

    public Grid3d(Bounds bbox, float voxelSize = 1.0f)
    {
        var watch = Stopwatch.StartNew();

        BBox = bbox;
        VoxelSize = voxelSize;

        bbox.min = new Vector3(bbox.min.x, 0, bbox.min.z);
        var sizef = bbox.size / voxelSize;
        Size = new Vector3Int((int)sizef.x, (int)sizef.y, (int)sizef.z);
        sizef = new Vector3(Size.x, Size.y, Size.z);

        Corner = bbox.min + (bbox.size - sizef * voxelSize) * 0.5f;

        // make voxels
        Voxels = new Voxel[Size.x, Size.y, Size.z];

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Voxels[x, y, z] = new Voxel(new Vector3Int(x, y, z), this);
                }

        // make corners
        Corners = new Corner[Size.x + 1, Size.y + 1, Size.z + 1];

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }

        // make faces
        Faces[0] = new Face[Size.x + 1, Size.y, Size.z];

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[Size.x, Size.y + 1, Size.z];

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[Size.x, Size.y, Size.z + 1];

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }

        // make edges
        Edges[2] = new Edge[Size.x + 1, Size.y + 1, Size.z];

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[Size.x, Size.y + 1, Size.z + 1];

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[Size.x + 1, Size.y, Size.z + 1];

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }

        Debug.Log($"Grid took: {watch.ElapsedMilliseconds} ms to create.\r\nGrid size: {Size}, {Size.x * Size.y * Size.z} voxels.");
    }

    public Grid3d Clone()
    {
        return new Grid3d(BBox, VoxelSize);
    }

    public void AddVoids(IEnumerable<MeshCollider> voids, bool invert = false)
    {
        foreach (var voxel in GetVoxels())
        {
            voxel.IsActive = !voxel.IsInside(voids);

            if (invert)
                voxel.IsActive = !voxel.IsActive;
        }
    }

    public IEnumerable<Voxel> GetVoxels()
    {
        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    public IEnumerable<Corner> GetCorners()
    {
        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    yield return Corners[x, y, z];
                }
    }

    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int z = 0; z < zSize; z++)
                for (int y = 0; y < ySize; y++)
                    for (int x = 0; x < xSize; x++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int z = 0; z < zSize; z++)
                for (int y = 0; y < ySize; y++)
                    for (int x = 0; x < xSize; x++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    public int GetConnectedComponents()
    {
        var graph = new UndirectedGraph<Voxel, Edge<Voxel>>();
        graph.AddVertexRange(GetVoxels().Where(v => v.IsActive));
        graph.AddEdgeRange(GetFaces().Where(f => f.IsActive).Select(f => new Edge<Voxel>(f.Voxels[0], f.Voxels[1])));

        Dictionary<Voxel, int> components = new Dictionary<Voxel, int>();
        var count = QuickGraph.Algorithms.AlgorithmExtensions.ConnectedComponents(graph, components);
        return count;
    }
}
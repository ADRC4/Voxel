using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;
using System.Diagnostics;
using QuickGraph;
using Debug = UnityEngine.Debug;

public enum Normal { X, Y, Z };

public class Grid3d
{
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public List<Face> Faces;
    public List<Edge> Edges;

    public Vector3Int Size;
    public float VoxelSize;
    public Vector3 Corner;
    public Bounds Bbox;


    public Mesh[] Mesh;

    private float _displacement = 0;

    public float DisplacementScale
    {
        get { return _displacement; }
        set
        {
            if (value != _displacement)
            {
                _displacement = value;
                MakeVoxelMesh();
            }
        }
    }

    public static Grid3d MakeGridWithVoids(IEnumerable<MeshCollider> voids, float voxelSize)
    {
        var bbox = new Bounds();
        foreach (var v in voids.Select(v => v.bounds))
            bbox.Encapsulate(v);

        var grid = new Grid3d(bbox, voxelSize);
        grid.AddVoids(voids);

        return grid;
    }

    public Grid3d(Bounds bbox, float voxelSize = 1.0f, float displacement = 10f)
    {
        var watch = new Stopwatch();
        watch.Start();

        Bbox = bbox;
        VoxelSize = voxelSize;
        _displacement = displacement;

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
        Faces = new List<Face>();

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Faces.Add(new Face(x, y, z, Normal.X, this));
                }

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Faces.Add(new Face(x, y, z, Normal.Y, this));
                }

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Faces.Add(new Face(x, y, z, Normal.Z, this));
                }

        // make edges
        Edges = new List<Edge>();

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Edges.Add(new Edge(x, y, z, Normal.Z, this));
                }

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    Edges.Add(new Edge(x, y, z, Normal.X, this));
                }

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Edges.Add(new Edge(x, y, z, Normal.Y, this));
                }

        // calculate
        //Analysis();

       // Debug.Log($"Time to generate grid: {watch.ElapsedMilliseconds} ms");

       // Debug.Log($"Grid size: {Size} units = {Size.x * Size.y * Size.z} voxels.");
    }

    public Grid3d Clone()
    {
        return new Grid3d(Bbox, VoxelSize);
    }

    public void AddVoids(IEnumerable<MeshCollider> voids)
    {
        Physics.queriesHitBackfaces = true;

        foreach (var voxel in GetVoxels())
        {
            voxel.IsActive = !voids.Any(voxel.IsInside);
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

    public int GetConnectedComponents()
    {
        var graph = new UndirectedGraph<Voxel, Edge<Voxel>>();
        graph.AddVertexRange(GetVoxels().Where(v => v.IsActive));
        graph.AddEdgeRange(Faces.Where(f => f.IsActive).Select(f => new Edge<Voxel>(f.Voxels[0], f.Voxels[1])));

        Dictionary<Voxel, int> components = new Dictionary<Voxel, int>();
        var count = QuickGraph.Algorithms.AlgorithmExtensions.ConnectedComponents(graph, components);
        return count;
    }

    public void Analysis()
    {
        // analysis model
        var model = new Model();

        var nodes = GetVoxels()
             .Where(b => b.IsActive)
             .SelectMany(v => v.GetCorners())
             .Distinct()
             .ToArray();


        var elements = GetVoxels()
             .Where(b => b.IsActive)
             .SelectMany(v => v.MakeTetrahedrons())
             .ToArray();

        model.Nodes.Add(nodes);
        model.Elements.Add(elements);

        model.Solve();

        // analysis results
        foreach (var node in nodes)
        {
            var d = node
           .GetNodalDisplacement(LoadCase.DefaultLoadCase)
           .Displacements;

            node.Displacement = new Vector3((float)d.X, (float)d.Z, (float)d.Y);
            var length = node.Displacement.magnitude;

            foreach (var voxel in node.GetConnectedVoxels())
                voxel.Value += length;
        }

        var activeVoxels = GetVoxels().Where(v => v.IsActive);

        foreach (var voxel in activeVoxels)
            voxel.Value /= voxel.GetCorners().Count();

        var min = activeVoxels.Min(v => v.Value);
        var max = activeVoxels.Max(v => v.Value);

        foreach (var voxel in activeVoxels)
            voxel.Value = Mathf.InverseLerp(min, max, voxel.Value);
    }

    public void MakeVoxelMesh()
    {
        Mesh = GetVoxels()
            .Where(v => v.IsActive)
            .Select(v =>
        {
            var corners = v.GetCorners()
                    .Select(c => c.DisplacedPosition)
                    .ToArray();

            return Drawing.MakeTwistedBox(corners, v.Value, null);
        }).ToArray();
    }
}
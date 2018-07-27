using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;
using System.Diagnostics;
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
    public IEnumerable<MeshCollider> Voids;
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
                MakeMesh();
            }
        }
    }

    public Grid3d(IEnumerable<MeshCollider> voids, float voxelSize = 1.0f, float displacement = 10f)
    {
        var watch = new Stopwatch();
        watch.Start();

        Voids = voids;
        VoxelSize = voxelSize;
        _displacement = displacement;

        var bbox = new Bounds();
        foreach (var v in voids.Select(v => v.bounds))
            bbox.Encapsulate(v);

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

        Debug.Log($"Time to generate grid: {watch.ElapsedMilliseconds} ms");
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

    public void MakeMesh()
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
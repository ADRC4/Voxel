using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;

public class Grid3d
{
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public List<Links> Links;

    public Vector3Int Size;
    public float VoxelSize;
    public Vector3 Corner;
    public IEnumerable<Bounds> Voids;

    private float _displacement = 0;

    public float DisplacementScale
    {
        get { return _displacement; }
        set
        {
            if (value != _displacement)
            {
                _displacement = value;
                foreach (var v in Voxels) v.MeshUpdate();
            }
        }
    }

    public Grid3d(IEnumerable<Bounds> voids, float voxelSize = 1.0f, float displacement = 10f)
    {
        Voids = voids;
        VoxelSize = voxelSize;
        _displacement = displacement;

        var bbox = new Bounds();
        foreach (var v in voids)
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

        // make links
        Links = new List<Links>();

        for (int z = 0; z < Size.z; z++)
            for (int y = 0; y < Size.y; y++)
                for (int x = 0; x < Size.x; x++)
                {
                    if (x < Size.x - 1)
                        Links.Add(new Links(Voxels[x, y, z], Voxels[x + 1, y, z]));

                    if (y < Size.y - 1)
                        Links.Add(new Links(Voxels[x, y, z], Voxels[x, y + 1, z]));

                    if (z < Size.z - 1)
                        Links.Add(new Links(Voxels[x, y, z], Voxels[x, y, z + 1]));
                }

        // make corners
        Corners = new Corner[Size.x + 1, Size.y + 1, Size.z + 1];

        for (int z = 0; z < Size.z + 1; z++)
            for (int y = 0; y < Size.y + 1; y++)
                for (int x = 0; x < Size.x + 1; x++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }

        // calculate
        Analysis();
    }

    IEnumerable<Voxel> GetNeighbours(Vector3Int index)
    {
        for (int zi = -1; zi <= 1; zi++)
        {
            int z = zi + index.z;
            if (z == -1 || z == Size.z) continue;

            for (int yi = -1; yi <= 1; yi++)
            {
                int y = yi + index.y;
                if (y == -1 || y == Size.y) continue;

                for (int xi = -1; xi <= 1; xi++)
                {
                    int x = xi + index.x;
                    if (x == -1 || x == Size.x) continue;

                    var i = new Vector3Int(x, y, z);
                    if (index == i) continue;

                    yield return Voxels[x, y, z];
                }
            }
        }
    }

    IEnumerable<Voxel> GetNeighboursOrtho(Vector3Int index)
    {
        int x = index.x;
        int y = index.y;
        int z = index.z;

        if (x != 0) yield return Voxels[x - 1, y, z];
        if (x != Size.x - 1) yield return Voxels[x + 1, y, z];

        if (y != 0) yield return Voxels[x, y - 1, z];
        if (y != Size.y - 1) yield return Voxels[x, y + 1, z];

        if (z != 0) yield return Voxels[x, y, z - 1];
        if (z != Size.z - 1) yield return Voxels[x, y, z + 1];
    }

    IEnumerable<Voxel> GetVoxels()
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
                voxel.Color += length;
        }

        var min = GetVoxels().Min(v => v.Color);
        var max = GetVoxels().Max(v => v.Color);

        foreach (var voxel in GetVoxels())
              voxel.Color = Mathf.InverseLerp(min, max, voxel.Color);
    }
}
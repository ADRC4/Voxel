using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Voxel
{
    public Vector3Int Index;
    public Vector3 Center;
    public bool IsActive;
    public float Value;
    public List<Face> Faces = new List<Face>(6);

    public bool IsClimbable => IsActive && Faces.Any(f => f.IsClimbable);

    Grid3d _grid;

    public Voxel(Vector3Int index, Grid3d grid)
    {
        _grid = grid;
        Index = index;
        Center = grid.Corner + new Vector3(index.x + 0.5f, index.y + 0.5f, index.z + 0.5f) * grid.VoxelSize;
        IsActive = true;
    }

    internal bool IsInside(IEnumerable<MeshCollider> colliders)
    {
        Physics.queriesHitBackfaces = true;

        var point = Center;
        var sortedHits = new Dictionary<Collider, int>();
        foreach (var collider in colliders)
            sortedHits.Add(collider, 0);

        while (Physics.Raycast(new Ray(point, Vector3.forward), out RaycastHit hit))
        {
            var collider = hit.collider;

            if (sortedHits.ContainsKey(collider))
                sortedHits[collider]++;

            point = hit.point + Vector3.forward * 0.00001f;
        }

        bool isInside = sortedHits.Any(kv => kv.Value % 2 != 0);
        return isInside;
    }


    public IEnumerable<Corner> GetCorners()
    {
        for (int y = 0; y <= 1; y++)
            for (int z = 0; z <= 1; z++)
                for (int x = 0; x <= 1; x++)
                {
                    yield return _grid.Corners[Index.x + x, Index.y + y, Index.z + z];
                }
    }

    public IEnumerable<Voxel> GetCornerNeighbours()
    {
        var s = _grid.Size;

        for (int zi = -1; zi <= 1; zi++)
        {
            int z = zi + Index.z;
            if (z == -1 || z == s.z) continue;

            for (int yi = -1; yi <= 1; yi++)
            {
                int y = yi + Index.y;
                if (y == -1 || y == s.y) continue;

                for (int xi = -1; xi <= 1; xi++)
                {
                    int x = xi + Index.x;
                    if (x == -1 || x == s.x) continue;

                    var i = new Vector3Int(x, y, z);
                    if (Index == i) continue;

                    yield return _grid.Voxels[x, y, z];
                }
            }
        }
    }

    public IEnumerable<Voxel> GetFaceNeighbours()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _grid.Size;

        if (x != 0) yield return _grid.Voxels[x - 1, y, z];
        if (x != s.x - 1) yield return _grid.Voxels[x + 1, y, z];

        if (y != 0) yield return _grid.Voxels[x, y - 1, z];
        if (y != s.y - 1) yield return _grid.Voxels[x, y + 1, z];

        if (z != 0) yield return _grid.Voxels[x, y, z - 1];
        if (z != s.z - 1) yield return _grid.Voxels[x, y, z + 1];
    }
}
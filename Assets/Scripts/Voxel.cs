using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;

public class Voxel
{
    public Vector3Int Index;
    public Vector3 Center;
    public bool IsActive;
    public float Value;
    public List<Link> Links = new List<Link>(6);

    Grid3d _grid;

    public Voxel(Vector3Int index, Grid3d grid)
    {
        _grid = grid;
        Index = index;
        Center = grid.Corner + new Vector3(index.x + 0.5f, index.y + 0.5f, index.z + 0.5f) * grid.VoxelSize;
        IsActive = !grid.Voids.Any(v => v.Contains(Center));
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

    public IEnumerable<Tetrahedral> MakeTetrahedrons()
    {
        var c = GetCorners().ToArray();

        var t = new[,]
        {
           { c[0], c[1], c[2], c[4]},
           { c[3], c[1], c[2], c[7]},
           { c[1], c[2], c[4], c[7]},
           { c[4], c[5], c[7], c[1]},
           { c[4], c[7], c[6], c[2]}
        };

        for (int i = 0; i < 5; i++)
        {
            var tetra = new Tetrahedral() { E = 210e9, Nu = 0.33 };

            for (int j = 0; j < 4; j++)
                tetra.Nodes[j] = t[i, j];

            yield return tetra;
        }
    }
}

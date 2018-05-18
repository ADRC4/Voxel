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
    public List<Links> Beams = new List<Links>(6);
    public float Color;
    public Vector3 Displacement;
    public Vector3 DisplacedCenter => Center + Displacement * _grid.DisplacementScale;
    Grid3d _grid;
    public Mesh Mesh { get; set; }

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

    public void MeshUpdate()
    {
        var corners = GetCorners()
            .Select(c => c.DisplacedPosition)
            .ToArray();

       Mesh = Drawing.MakeTwistedBox(corners, Mesh);
    }
}

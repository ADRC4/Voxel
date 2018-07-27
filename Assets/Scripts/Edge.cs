using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Edge
{
    public Vector3Int Index;
    public Normal Direction;
    public Voxel[] Voxels;
    public Face[] Faces;
    public Face[] ClimbableFaces;

    Grid3d _grid;

    public Edge(int x, int y, int z, Normal direction, Grid3d grid)
    {
        _grid = grid;

        Index = new Vector3Int(x, y, z);
        Direction = direction;
        Voxels = GetVoxels();
        Faces = GetFaces();
        ClimbableFaces = Faces.Where(f => f != null && f.IsClimbable).ToArray();
    }

    Voxel[] GetVoxels()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        switch (Direction)
        {
            case Normal.X:
                return new[]
                {
                   (z == 0 || y == 0) ? null : _grid.Voxels[x, y - 1, z - 1],
                   (z == _grid.Size.z || y == 0) ? null : _grid.Voxels[x, y - 1, z],
                   (z == 0 || y == _grid.Size.y) ? null : _grid.Voxels[x, y, z - 1],
                   (z == _grid.Size.z || y == _grid.Size.y) ? null : _grid.Voxels[x, y, z]
                 };
            case Normal.Y:
                return new[]
                {
                   (x == 0 || z == 0) ? null : _grid.Voxels[x - 1, y, z - 1],
                   (x == _grid.Size.x || z == 0) ? null : _grid.Voxels[x, y, z - 1],
                   (x == 0 || z == _grid.Size.z) ? null : _grid.Voxels[x - 1, y, z],
                   (x == _grid.Size.x || z == _grid.Size.z) ? null : _grid.Voxels[x, y, z]
                };
            case Normal.Z:
                return new[]
                {
                   (x == 0 || y == 0) ? null : _grid.Voxels[x - 1, y - 1, z],
                   (x == _grid.Size.x || y == 0) ? null : _grid.Voxels[x, y - 1, z],
                   (x == 0 || y == _grid.Size.y) ? null : _grid.Voxels[x - 1, y, z],
                   (x == _grid.Size.x || y == _grid.Size.y) ? null : _grid.Voxels[x, y, z]
                };
            default:
                throw new Exception("Wrong direction.");
        }
    }

    Face[] GetFaces()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        switch (Direction)
        {
            case Normal.X:
                return new[]
                {
                    y == 0 ? Voxels[2]?.Faces[2] : Voxels[0]?.Faces[3],
                    y == 0 ? Voxels[3]?.Faces[2] : Voxels[1]?.Faces[3],
                    z == 0 ? Voxels[1]?.Faces[4] : Voxels[0]?.Faces[5],
                    z == 0 ? Voxels[3]?.Faces[4] : Voxels[2]?.Faces[5],
                };
            case Normal.Y:
                return new[]
                {
                    x == 0 ? Voxels[1]?.Faces[0] : Voxels[0]?.Faces[1],
                    x == 0 ? Voxels[3]?.Faces[0] : Voxels[2]?.Faces[1],
                    z == 0 ? Voxels[2]?.Faces[4] : Voxels[0]?.Faces[5],
                    z == 0 ? Voxels[3]?.Faces[4] : Voxels[1]?.Faces[5],
                };
            case Normal.Z:
                return new[]
                {
                    x == 0 ? Voxels[1]?.Faces[0] : Voxels[0]?.Faces[1],
                    x == 0 ? Voxels[3]?.Faces[0] : Voxels[2]?.Faces[1],
                    y == 0 ? Voxels[2]?.Faces[2] : Voxels[0]?.Faces[3],
                    y == 0 ? Voxels[3]?.Faces[2] : Voxels[1]?.Faces[3],
                };
            default:
                throw new Exception("Wrong direction.");
        }
    }
}
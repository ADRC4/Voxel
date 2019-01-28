using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Edge
{
    public Vector3Int Index;
    public Axis Direction;
    public Vector3 Center;
    public Voxel[] Voxels;
    public Face[] Faces;
    public Face[] ClimbableFaces => Faces.Where(f => f?.IsClimbable == true).ToArray();

    Grid3d _grid;

    public Edge(int x, int y, int z, Axis direction, Grid3d grid)
    {
        _grid = grid;
        Index = new Vector3Int(x, y, z);
        Direction = direction;
        Center = GetCenter();
        Voxels = GetVoxels();
        Faces = GetFaces();
    }

    public Vector3 Normal
    {
        get
        {
            Vector3 normal = Vector3.zero;

            foreach (var face in Faces.Where(f => f != null))
                normal += face.Normal;

            return normal.normalized;
        }
    }

    Vector3 GetCenter()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        switch (Direction)
        {
            case Axis.X:
                return _grid.Corner + new Vector3(x + 0.5f, y, z) * _grid.VoxelSize;
            case Axis.Y:
                return _grid.Corner + new Vector3(x, y + 0.5f, z) * _grid.VoxelSize;
            case Axis.Z:
                return _grid.Corner + new Vector3(x, y, z + 0.5f) * _grid.VoxelSize;
            default:
                throw new Exception("Wrong direction.");
        }
    }

    Voxel[] GetVoxels()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        switch (Direction)
        {
            case Axis.X:
                return new[]
                {
                   (z == 0 || y == 0) ? null : _grid.Voxels[x, y - 1, z - 1],
                   (z == _grid.Size.z || y == 0) ? null : _grid.Voxels[x, y - 1, z],
                   (z == 0 || y == _grid.Size.y) ? null : _grid.Voxels[x, y, z - 1],
                   (z == _grid.Size.z || y == _grid.Size.y) ? null : _grid.Voxels[x, y, z]
                 };
            case Axis.Y:
                return new[]
                {
                   (x == 0 || z == 0) ? null : _grid.Voxels[x - 1, y, z - 1],
                   (x == _grid.Size.x || z == 0) ? null : _grid.Voxels[x, y, z - 1],
                   (x == 0 || z == _grid.Size.z) ? null : _grid.Voxels[x - 1, y, z],
                   (x == _grid.Size.x || z == _grid.Size.z) ? null : _grid.Voxels[x, y, z]
                };
            case Axis.Z:
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
            case Axis.X:
                return new[]
                {
                    y == 0 ? Voxels[2]?.Faces[2] : Voxels[0]?.Faces[3],
                    y == 0 ? Voxels[3]?.Faces[2] : Voxels[1]?.Faces[3],
                    z == 0 ? Voxels[1]?.Faces[4] : Voxels[0]?.Faces[5],
                    z == 0 ? Voxels[3]?.Faces[4] : Voxels[2]?.Faces[5],
                };
            case Axis.Y:
                return new[]
                {
                    x == 0 ? Voxels[1]?.Faces[0] : Voxels[0]?.Faces[1],
                    x == 0 ? Voxels[3]?.Faces[0] : Voxels[2]?.Faces[1],
                    z == 0 ? Voxels[2]?.Faces[4] : Voxels[0]?.Faces[5],
                    z == 0 ? Voxels[3]?.Faces[4] : Voxels[1]?.Faces[5],
                };
            case Axis.Z:
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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BriefFiniteElementNet;
using BriefFiniteElementNet.Elements;
using System;
using QuickGraph;

public class Face
{
    public Voxel[] Voxels;
    public Vector3Int Index;
    public Vector3 Center;
    public Normal Direction;
    //public float NormalizedDistance = 0f;
    public Mesh Geometry;

    Grid3d _grid;
    // public FrameElement2Node Frame;

    public bool IsActive => Voxels.Count(v => v != null && v.IsActive) == 2;

    public bool IsClimbable
    {
        get
        {
            if (Index.y == 0 && Direction == Normal.Y) return false;
            return Voxels.Count(v => v != null && v.IsActive) == 1;
        }
    }

    public Face(int x, int y, int z, Normal direction, Grid3d grid)
    {
        _grid = grid;
        Index = new Vector3Int(x, y, z);
        Direction = direction;
        Voxels = GetVoxels();

        foreach (var v in Voxels.Where(v => v != null))
            v.Faces.Add(this);

        Center = GetCenter();

        // var center = Corner + new Vector3(x, y+0.5f, z + 0.5f) * VoxelSize;

        //Frame = new FrameElement2Node(start, end)
        //{
        //    Iy = 0.02,
        //    Iz = 0.02,
        //    A = 0.01,
        //    J = 0.05,
        //    E = 210e9,
        //    G = 70e9,
        //    ConsiderShearDeformation = false,
        //};
    }

    Vector3 GetCenter()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        switch (Direction)
        {
            case Normal.X:
                return _grid.Corner + new Vector3(x, y + 0.5f, z + 0.5f) * _grid.VoxelSize;
            case Normal.Y:
                return _grid.Corner + new Vector3(x + 0.5f, y, z + 0.5f) * _grid.VoxelSize;
            case Normal.Z:
                return _grid.Corner + new Vector3(x + 0.5f, y + 0.5f, z) * _grid.VoxelSize;
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
            case Normal.X:
                return new[]
                {
                   x == 0 ? null : _grid.Voxels[x - 1, y, z],
                   x == _grid.Size.x ? null : _grid.Voxels[x, y, z]
                };
            case Normal.Y:
                return new[]
                {
                   y == 0 ? null : _grid.Voxels[x, y - 1, z],
                   y == _grid.Size.y ? null : _grid.Voxels[x, y, z]
                };
            case Normal.Z:
                return new[]
                {
                   z == 0 ? null : _grid.Voxels[x, y, z - 1],
                   z == _grid.Size.z ? null : _grid.Voxels[x, y, z]
                 };
            default:
                throw new Exception("Wrong direction.");
        }
    }
}

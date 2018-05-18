using System.Collections.Generic;
using UnityEngine;
using BriefFiniteElementNet;
using Constraint = BriefFiniteElementNet.Constraints;

public class Corner : Node
{
    public Vector3 Position;
    public Vector3Int Index;
    public Vector3 Displacement;
    public Vector3 DisplacedPosition => Position + Displacement * _grid.DisplacementScale;
    Grid3d _grid;

    public Corner(Vector3Int index, Grid3d grid)
    {
        _grid = grid;
        Index = index;
        Position = grid.Corner + new Vector3(index.x, index.y, index.z) * grid.VoxelSize;

        Location = new Point(Position.x, Position.z, Position.y);
        Constraints = index.y == 0 ? Constraint.Fixed : Constraint.RotationFixed;
        Loads.Add(new NodalLoad(new Force(0, 0, -2000000, 0, 0, 0)));
    }

    public IEnumerable<Voxel> GetConnectedVoxels()
    {
        for (int zi = -1; zi <= 0; zi++)
        {
            int z = zi + Index.z;
            if (z == -1 || z == _grid.Size.z) continue;

            for (int yi = -1; yi <= 0; yi++)
            {
                int y = yi + Index.y;
                if (y == -1 || y == _grid.Size.y) continue;

                for (int xi = -1; xi <= 0; xi++)
                {
                    int x = xi + Index.x;
                    if (x == -1 || x == _grid.Size.x) continue;

                    var i = new Vector3Int(x, y, z);
                    if (Index == i) continue;

                    yield return _grid.Voxels[x, y, z];
                }
            }
        }
    }
}

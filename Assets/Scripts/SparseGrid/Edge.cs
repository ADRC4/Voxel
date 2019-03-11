using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SparseGrid
{
    public class Edge
    {
        public Vector3Int Index;
        public Axis Direction;
        public Vector3 Center;
        public Voxel[] Voxels => GetVoxels();
        public Face[] Faces => GetFaces();
        public Face[] ClimbableFaces => Faces.Where(f => f?.IsClimbable == true).ToArray();

        Grid3d _grid;

        public Edge(int x, int y, int z, Axis direction, Grid3d grid)
        {
            _grid = grid;
            Index = new Vector3Int(x, y, z);
            Direction = direction;
            Center = GetCenter();
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
                    return new Vector3(x + 0.5f, y, z) * _grid.VoxelSize;
                case Axis.Y:
                    return new Vector3(x, y + 0.5f, z) * _grid.VoxelSize;
                case Axis.Z:
                    return new Vector3(x, y, z + 0.5f) * _grid.VoxelSize;
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
                    {
                        return new[]
                        {
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y - 1, z - 1), out var bottomLeft) ? bottomLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y - 1, z), out var bottomRight) ? bottomRight : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y, z - 1), out var topLeft) ? topLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y, z), out var topRight) ? topRight : null,
                        };
                    }
                case Axis.Y:
                    {
                        return new[]
                        {
                            _grid.Voxels.TryGetValue(new Vector3Int(x - 1, y, z - 1), out var bottomLeft) ? bottomLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y, z - 1), out var bottomRight) ? bottomRight : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x - 1, y, z), out var topLeft) ? topLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y, z), out var topRight) ? topRight : null,
                        };
                    }
                case Axis.Z:
                    {
                        return new[]
                        {
                            _grid.Voxels.TryGetValue(new Vector3Int(x - 1, y - 1, z), out var bottomLeft) ? bottomLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y - 1, z), out var bottomRight) ? bottomRight : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x - 1, y, z), out var topLeft) ? topLeft : null,
                            _grid.Voxels.TryGetValue(new Vector3Int(x, y, z), out var topRight) ? topRight : null,
                        };
                    }
                default:
                    throw new Exception("Wrong direction.");
            }
        }

        Face[] GetFaces()
        {
            int x = Index.x;
            int y = Index.y;
            int z = Index.z;

            var voxels = Voxels;

            switch (Direction)
            {
                case Axis.X:
                    return new[]
                    {
                    y == 0 ? voxels[2]?.Faces[2] : Voxels[0]?.Faces[3],
                    y == 0 ? voxels[3]?.Faces[2] : Voxels[1]?.Faces[3],
                    z == 0 ? voxels[1]?.Faces[4] : Voxels[0]?.Faces[5],
                    z == 0 ? voxels[3]?.Faces[4] : Voxels[2]?.Faces[5],
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
}
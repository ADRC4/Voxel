using System.Collections.Generic;
using UnityEngine;

namespace SparseGrid
{
    public class Corner
    {
        public Vector3 Position;
        public Vector3Int Index;
        protected Grid3d _grid;

        public Corner(Vector3Int index, Grid3d grid)
        {
            _grid = grid;
            Index = index;
            Position = new Vector3(index.x, index.y, index.z) * grid.VoxelSize;
        }

        public IEnumerable<Voxel> GetConnectedVoxels()
        {
            for (int zi = -1; zi <= 0; zi++)
            {
                int z = zi + Index.z;

                for (int yi = -1; yi <= 0; yi++)
                {
                    int y = yi + Index.y;

                    for (int xi = -1; xi <= 0; xi++)
                    {
                        int x = xi + Index.x;

                        var i = new Vector3Int(x, y, z);
                        if (_grid.Voxels.TryGetValue(i, out var voxel))
                            yield return voxel;
                    }
                }
            }
        }
    }
}
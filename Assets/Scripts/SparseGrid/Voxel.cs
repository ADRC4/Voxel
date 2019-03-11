using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SparseGrid
{
    public class Voxel
    {
        public Vector3Int Index;
        public Vector3 Center;
        public bool IsActive;
        public float Value;
        // public List<Face> Faces = new List<Face>(6);

        public bool IsClimbable => IsActive && Faces.Any(f => f.IsClimbable);

        Grid3d _grid;

        public Voxel(Vector3Int index, Grid3d grid)
        {
            _grid = grid;
            Index = index;
            Center = new Vector3(index.x + 0.5f, index.y + 0.5f, index.z + 0.5f) * grid.VoxelSize;
            IsActive = true;
        }

        //internal bool IsInside(IEnumerable<MeshCollider> colliders)
        //{
        //    Physics.queriesHitBackfaces = true;

        //    var point = Center;
        //    var sortedHits = new Dictionary<Collider, int>();
        //    foreach (var collider in colliders)
        //        sortedHits.Add(collider, 0);

        //    while (Physics.Raycast(new Ray(point, Vector3.forward), out RaycastHit hit))
        //    {
        //        var collider = hit.collider;

        //        if (sortedHits.ContainsKey(collider))
        //            sortedHits[collider]++;

        //        point = hit.point + Vector3.forward * 0.00001f;
        //    }

        //    bool isInside = sortedHits.Any(kv => kv.Value % 2 != 0);
        //    return isInside;
        //}


        public Face[] Faces
        {
            get
            {
                int x = Index.x;
                int y = Index.y;
                int z = Index.z;

                return new[]
                {
                  _grid.Faces[new AxisVector3Int(Axis.X, x - 1, y, z)],
                  _grid.Faces[new AxisVector3Int(Axis.X, x + 1, y, z)],
                  _grid.Faces[new AxisVector3Int(Axis.Y, x, y - 1, z)],
                  _grid.Faces[new AxisVector3Int(Axis.Y, x, y + 1, z)],
                  _grid.Faces[new AxisVector3Int(Axis.Z, x, y, z - 1)],
                  _grid.Faces[new AxisVector3Int(Axis.Z, x, y, z + 1)],
                };
            }
        }

        public IEnumerable<Corner> GetCorners()
        {
            for (int y = 0; y <= 1; y++)
                for (int z = 0; z <= 1; z++)
                    for (int x = 0; x <= 1; x++)
                    {
                        var index = new Vector3Int(Index.x + x, Index.y + y, Index.z + z);
                        yield return _grid.Corners[index];
                    }
        }

        public IEnumerable<Voxel> GetCornerNeighbours()
        {
            // var s = _grid.Size;

            for (int zi = -1; zi <= 1; zi++)
            {
                int z = zi + Index.z;

                for (int yi = -1; yi <= 1; yi++)
                {
                    int y = yi + Index.y;

                    for (int xi = -1; xi <= 1; xi++)
                    {
                        int x = xi + Index.x;

                        var i = new Vector3Int(x, y, z);
                        if (Index == i) continue;

                        if (_grid.Voxels.TryGetValue(i, out var voxel))
                            yield return voxel;
                    }
                }
            }
        }

        public IEnumerable<Voxel> GetFaceNeighbours()
        {
            int x = Index.x;
            int y = Index.y;
            int z = Index.z;

            var indices = new[]
            {
                new Vector3Int(x - 1, y, z),
                new Vector3Int(x + 1, y, z),
                new Vector3Int(x, y - 1, z),
                new Vector3Int(x, y + 1, z),
                new Vector3Int(x, y, z - 1),
                new Vector3Int(x, y, z + 1),
            };

            foreach (var i in indices)
                if (_grid.Voxels.TryGetValue(i, out var voxel))
                    yield return voxel;
        }
    }
}
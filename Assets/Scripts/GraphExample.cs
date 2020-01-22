using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DenseGrid;

public class GraphExample : MonoBehaviour
{
    Grid3d _grid;
    List<Vector3> _centers = new List<Vector3>();

    void Start()
    {
        var bounds = new Bounds(new Vector3(0, 5, 0), new Vector3(15, 9, 9));
        _grid = new Grid3d(bounds, 0.8f);

        foreach (var voxel in _grid.GetVoxels())
            voxel.IsActive = true;

        StartCoroutine(GrowGrid());
    }

    IEnumerator GrowGrid()
    {
        int count = 100000;
        int lastFitness = 0;

        while (count-- > 0)
        {
            var skinVoxels = _grid.GetVoxels().Where(v => v.IsSkin).ToList();

            if (skinVoxels.Count == 0)
                break;

            var index = Random.Range(0, skinVoxels.Count);
            var candidate = skinVoxels[index];

            candidate.IsActive = false;

            int componentCount = _grid.GetConnectedComponents();
            if(componentCount != 1)
            {
                Debug.Log("Tried to remove a voxel that disconnected the structure.");
                candidate.IsActive = true;
                yield return null;
            }

            int fitness = 0;

            foreach(var voxel in _grid.GetVoxels())
            {
                if (!voxel.IsActive) continue;
                int neighbourCount = voxel.GetFaceNeighbours().Count(n => n.IsActive);

                if (neighbourCount == 2)
                    fitness += 1;
            }

            if(fitness < lastFitness)
            {
                Debug.Log("Fitness decreased.");
                candidate.IsActive = true;
                continue;
            }

            lastFitness = fitness;
            UpdateCenters();
            yield return new WaitForSeconds(0.001f);
        }
    }


    void UpdateCenters()
    {
        _centers.Clear();

        foreach (var voxel in _grid.GetVoxels())
        {
            if (voxel.IsActive)
                _centers.Add(voxel.Center);
        }
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var center in _centers)
        {
            Drawing.DrawCube(center, _grid.VoxelSize);
        }
    }
}
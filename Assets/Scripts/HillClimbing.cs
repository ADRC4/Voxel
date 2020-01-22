using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DenseGrid;

public class HillClimbing : MonoBehaviour
{
    [SerializeField] GUISkin _skin = null;

    Grid3d _grid = null;
    string _voxelSize = "0.8";
    CancellationTokenSource _cancel = new CancellationTokenSource();
    List<Vector3> _centers = new List<Vector3>();


    private void Start()
    {
        Run();
    }

    void Update()
    {
        if (_grid == null) return;

        lock (_centers)
        {
            var size = _grid.VoxelSize;

            foreach (var c in _centers)
                Drawing.DrawCube(c, size, 0);
        }
    }

    void OnGUI()
    {
        int i = 1;
        int s = 25;
        GUI.skin = _skin;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (GUI.Button(new Rect(s, s * i++, 100, 20), "Generate"))
        {
            Run();
        }
    }

    void UpdateActiveVoxels()
    {
        _centers.Clear();

        foreach (var voxel in _grid.GetVoxels())
        {
            if (voxel.IsActive)
                _centers.Add(voxel.Center);
        }
    }

    private void Run()
    {
        _cancel.Cancel();
        _cancel = new CancellationTokenSource();
        MakeGrid(12, 6, 8);

        Task.Run(() => RemoveVoxels(_cancel.Token), _cancel.Token);
    }

    void MakeGrid(float x, float y, float z)
    {
        var voxelSize = float.Parse(_voxelSize);
        var bbox = new Bounds();
        bbox.SetMinMax(new Vector3(-x * 0.5f, 0, -z * 0.5f), new Vector3(x * 0.5f, y, z * 0.5f));
        _grid = new Grid3d(bbox, voxelSize);
    }

    void RemoveVoxels(CancellationToken token)
    {
        int lastFitness = int.MinValue;

        for (int i = 0; i < 100000; i++)
        {
            if (token.IsCancellationRequested) break;

            var actives = _grid.GetVoxels().Where(v => v.IsSkin).ToList();
            var random = Drawing.Random.Next(0, actives.Count);
            var active = actives[random];

            active.IsActive = false;

            if (_grid.GetConnectedComponents() != 1)
            {
                active.IsActive = true;
                continue;
            }

            int fitness = 0;

            foreach (var voxel in _grid.GetVoxels())
            {
                if (!voxel.IsActive) continue;

                var neighbourCount = voxel
                    .GetFaceNeighbours()
                    .Count(n => n.IsActive);

                if (neighbourCount == 2)
                    fitness += 1;
            }

            if (fitness < lastFitness)
            {
                active.IsActive = true;
                continue;
            }

            lastFitness = fitness;

            lock (_centers)
                UpdateActiveVoxels();

            // Debug.Log(fitness);
        }
    }

    private void OnApplicationQuit()
    {
        _cancel.Cancel();
    }
}

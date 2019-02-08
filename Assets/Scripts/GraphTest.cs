using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    Grid3d _grid = null;
    string _voxelSize = "0.8";
    CancellationTokenSource _cancel = new CancellationTokenSource();
    private bool _enableDraw = true;

    [SerializeField] GUISkin _skin;


    private void Start()
    {
        Run();
    }

    void Update()
    {
        if (_grid == null) return;
        DrawActiveBoxes();
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

    List<Vector3> _centers = new List<Vector3>();

    void DrawActiveBoxes()
    {
        if (_enableDraw)
        {
            _centers.Clear();
            foreach (var voxel in _grid.GetVoxels())
            {
                if (voxel.IsActive)
                    _centers.Add(voxel.Center);
            }
        }

        var size = _grid.VoxelSize;

        foreach (var c in _centers)
            Drawing.DrawCube(c, size,0);
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
        int lastClimbable = _grid.GetFaces().Count(f => f.IsClimbable);

        for (int i = 0; i < 100000; i++)
        {
            var actives = _grid.GetVoxels().Where(v => v.IsClimbable).ToList();
            var random = Drawing.Random.Next(0, actives.Count);
            var active = actives[random];

            _enableDraw = false;

            active.IsActive = false;

            if (_grid.GetConnectedComponents() != 1)
                active.IsActive = true;

            int climbableCount = _grid.GetFaces().Count(f => f.IsClimbable);

            if (climbableCount < lastClimbable)
                active.IsActive = true;
            else
                lastClimbable = climbableCount;

            _enableDraw = true;

            Debug.Log(lastClimbable);

            if (token.IsCancellationRequested) break;
        }
    }

    private void OnApplicationQuit()
    {
        _cancel.Cancel();
    }
}

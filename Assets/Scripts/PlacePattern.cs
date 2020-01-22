using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;
using QuickGraph.Algorithms;
using DenseGrid;

public class PlacePattern : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin = null;

    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    Grid3d _grid = null;


    private void Start()
    {
        var bbox = new Bounds(new Vector3(5, 5, 3), new Vector3(10, 10, 6));
        _grid = new Grid3d(bbox, 1f);

        PatternTest();
    }

    void OnGUI()
    {
        GUI.skin = _skin;
        _windowRect = GUI.Window(0, _windowRect, WindowFunction, string.Empty);
    }

    void WindowFunction(int windowID)
    {
        //int i = 1;
        //int s = 25;
    }

    void PatternTest()
    {
        var lPattern = new Vector3Int[]
        {
            new Vector3Int(0,0,0),
            new Vector3Int(1,0,0),
            new Vector3Int(2,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,2,0),
            new Vector3Int(0,3,0)
        };

        var anchor = new Vector3Int(1, 8, 0);
        var rotation = Quaternion.Euler(0, 0, -90);

        if (!_grid.TryPlacePattern(1, lPattern, anchor, rotation))
            Debug.Log("Pattern outside grid bounds.");
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var voxel in _grid.GetVoxels())
        {
            Drawing.DrawCube(voxel.Center, _grid.VoxelSize, voxel.Tile);
        }
    }
}


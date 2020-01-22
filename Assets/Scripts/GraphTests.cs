using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;
using QuickGraph.Algorithms;
using DenseGrid;

public class GraphTests : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin = null;

    bool _toggleVoids = true;
    string _voxelSize = "0.8";
    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    GameObject _voids;
    Grid3d _grid;
    Coroutine _animation;

    private void Start()
    {
        _voids = GameObject.Find("Voids");
    }

    void OnGUI()
    {
        GUI.skin = _skin;
        _windowRect = GUI.Window(0, _windowRect, WindowFunction, string.Empty);
    }

    void WindowFunction(int windowID)
    {
        int i = 1;
        int s = 25;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (GUI.Button(new Rect(s, s * i++, 100, 20), "Generate"))
        {
            if (_animation != null) StopCoroutine(_animation);
            _animation = StartCoroutine(GrowVoxelsAnimation());
        }

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
            ToggleVoids(!_toggleVoids);
    }

    void ToggleVoids(bool toggle)
    {
        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = toggle;

        _toggleVoids = toggle;
    }

    List<Face> _sequence = new List<Face>();

    void Update()
    {
        if (_grid == null) return;

        foreach (var voxel in _sequence)
            Drawing.DrawFace(voxel.Center, voxel.Direction, _grid.VoxelSize);
    }

    IEnumerator GrowVoxelsAnimation()
    {
        float size = float.Parse(_voxelSize);
        var voids = _voids.GetComponentsInChildren<MeshCollider>();
        _grid = Grid3d.MakeGridWithVoids(voids, size);
        ToggleVoids(false);

        _sequence.Clear();

        foreach (var voxel in _grid.GetFaces().Where(v => v.IsSkin))
        {
            _sequence.Add(voxel);
            yield return null;
        }
    }
}

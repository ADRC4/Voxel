using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;
using QuickGraph.Algorithms;
using System;
using SparseGrid;

public class Sparse : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin;

    bool _toggleVoids = true;
    bool _toggleTransparency = true;
    string _voxelSize = "0.8";
    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    Grid3d _grid = null;
    GameObject _voids;
    Mesh[] _meshes;

    private void Start()
    {
        _voids = GameObject.Find("Voids");
        ToggleVoids();
        MakeGrid();
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
            MakeGrid();

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
            ToggleVoids();

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");
    }

    private void ToggleVoids()
    {
        _toggleVoids = !_toggleVoids;

        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = _toggleVoids;
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var voxel in _grid.GetVoxels())
            Drawing.DrawCube(voxel.Center, _grid.VoxelSize * 0.5f, 0.8f);

        foreach (var voxel in _flowPath)
            Drawing.DrawCube(voxel.Center, _grid.VoxelSize, 0);
    }

    List<Voxel> _flowPath = new List<Voxel>();

    void MakeGrid()
    {
        var bounds = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .Select(c=>c.bounds)
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);
        _grid = Grid3d.MakeGridWithBounds(bounds, voxelSize);

       //  shortest path to test if connectivity is working
        var faces = _grid.GetFaces().Where(f => f.IsActive).ToList();
        var graphEdges = faces.Select(f => new TaggedEdge<Voxel, Face>(f.Voxels[0], f.Voxels[1], f));
        var graph = graphEdges.ToUndirectedGraph<Voxel, TaggedEdge<Voxel, Face>>();

        var start = _grid.GetVoxels().ElementAt(2);
        var end = _grid.GetVoxels().ElementAt(250);

        var shortest = graph.ShortestPathsDijkstra(_ => 1, start);

        shortest(end, out var path);
        var current = start;
        _flowPath.Add(current);

        foreach(var face in path)
        {
            current = face.GetOtherVertex(current);
            _flowPath.Add(current);
        }
    }
}

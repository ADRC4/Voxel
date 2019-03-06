using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;
using QuickGraph.Algorithms;

public class VoxelGrowth : MonoBehaviour
{
    // UI
    [SerializeField]
    GUISkin _skin;

    bool _toggleVoids = true;
    bool _toggleTransparency = false;
    string _voxelSize = "0.8";
    Rect _windowRect = new Rect(20, 20, 150, 160);

    // grid
    Grid3d _grid = null;
    GameObject _voids;
    Mesh[] _meshes;

    List<(Voxel, float)> _orderedVoxels = new List<(Voxel, float)>();
    int _animatedCount;
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
            GrowVoxels();

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
        {
            _toggleVoids = !_toggleVoids;

            foreach (var r in _voids.GetComponentsInChildren<Renderer>())
                r.enabled = _toggleVoids;
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var (voxel, f) in _orderedVoxels.Take(_animatedCount))
            Drawing.DrawCube(voxel.Center, _grid.VoxelSize, f);
    }

    void GrowVoxels()
    {
        _animatedCount = 0;
        MakeGrid();

        if (_animation != null) StopCoroutine(_animation);
        _animation = StartCoroutine(GrowthAnimation());
    }

    void MakeGrid()
    {
        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);
        _grid = Grid3d.MakeGridWithVoids(colliders, voxelSize);

        var faces = _grid.GetFaces().Where(f => f.IsActive);
        var graphEdges = faces.Select(f => new TaggedEdge<Voxel, Face>(f.Voxels[0], f.Voxels[1], f));
        var graph = graphEdges.ToUndirectedGraph<Voxel, TaggedEdge<Voxel, Face>>();

        var bottomSlab = _grid
                            .GetVoxels()
                            .Where(v => v.IsActive && v.Index.y == 0)
                            .ToList();

        var bottomCentroid = _grid.BBox.center;
        bottomCentroid.y = 0;

        var startVoxel = bottomSlab.MinBy(v => (v.Center - bottomCentroid).sqrMagnitude);
        var shortest = graph.ShortestPathsDijkstra(e => 1.0, startVoxel);

        _orderedVoxels = _grid
                          .GetVoxels()
                          .Where(v => v.IsActive)
                          .Select(v => (v, shortest(v, out var path) ? path.Count() + Random.value * 0.9f : float.MaxValue))
                          .OrderBy(p => p.Item2)
                          .Select(p => (p.v, p.Item2 / 30f))
                          .ToList();
    }

    IEnumerator GrowthAnimation()
    {
        while (_animatedCount < _orderedVoxels.Count)
        {
            _animatedCount++;
            yield return new WaitForSeconds(0.001f);
        }
    }
}

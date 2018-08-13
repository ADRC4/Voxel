using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using QuickGraph;

public class VoxelGrowth : MonoBehaviour
{
    [SerializeField]
    GUISkin _skin;

    Grid3d _grid = null;
    GameObject _voids;
    bool _toggleVoids = true;
    bool _toggleTransparency = false;
    string _voxelSize = "0.8";

    List<Voxel> _orderedVoxels = new List<Voxel>();
    int _animatedCount;

    private void Awake()
    {
        _voids = GameObject.Find("Voids");
    }

    private void Start()
    {
        MakeGrid();
        ToggleVoids();
        StartCoroutine(GrowthAnimation());
    }

    void Update()
    {
        if (_grid == null) return;

        foreach (var voxel in _orderedVoxels.Take(_animatedCount))
            Drawing.DrawCube(voxel.Center, _grid.VoxelSize);
    }

    void OnGUI()
    {
        int i = 1;
        int s = 25;
        GUI.skin = _skin;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (GUI.Button(new Rect(s, s * i++, 100, 20), "Generate"))
        {
            MakeGrid();
        }

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
        {
            ToggleVoids();
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");
    }

    void MakeGrid()
    {
        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);
        _grid = Grid3d.MakeGridWithVoids(colliders, voxelSize);

        var faces = _grid.Faces.Where(f => f.IsActive);
        var graphEdges = faces.Select(f => new TaggedEdge<Voxel, Face>(f.Voxels[0], f.Voxels[1], f));
        var graph = graphEdges.ToUndirectedGraph<Voxel, TaggedEdge<Voxel, Face>>();

        var bottomSlab = _grid
                            .GetVoxels()
                            .Where(v => v.IsActive && v.Index.y == 0)
                            .ToList();

        Vector3 bottomCentroid = bottomSlab
                                .Select(v => v.Center)
                                .Average();

        var startVoxel = bottomSlab.MinBy(v => (v.Center - bottomCentroid).sqrMagnitude);
        var shortest = QuickGraph.Algorithms.AlgorithmExtensions.ShortestPathsDijkstra(graph, e => 1.0, startVoxel);

        _orderedVoxels = _grid
                          .GetVoxels()
                          .Where(v => v.IsActive)
                          .OrderBy(v =>
                          {
                              IEnumerable<TaggedEdge<Voxel, Face>> path;
                              return shortest(v, out path) ? path.Count() + Random.value * 0.9f : float.MaxValue;
                          })
                          .ToList();
    }

    IEnumerator GrowthAnimation()
    {
        while (_animatedCount < _orderedVoxels.Count)
        {
            _animatedCount++;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void ToggleVoids()
    {
        _toggleVoids = !_toggleVoids;

        foreach (var r in _voids.GetComponentsInChildren<Renderer>())
            r.enabled = _toggleVoids;
    }
}

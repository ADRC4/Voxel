using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class MainController : MonoBehaviour
{
    Grid3d _grid = null;
    GameObject _voids;
    Coroutine _liveUpdate;
    bool _toggleVoids = true;
    bool _toggleUpdate = false;
    bool _toggleTransparency = false;
    float _displacement = 250f;
    string _voxelSize = "0.8";

    [SerializeField]
    GUISkin _skin;

    void Awake()
    {
        _voids = GameObject.Find("Voids");
    }

    void OnGUI()
    {
        int i = 1;
        int s = 25;
        GUI.skin = _skin;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if (_toggleUpdate != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleUpdate, "Live update"))
        {
            _toggleUpdate = !_toggleUpdate;

            if (_toggleUpdate)
                _liveUpdate = StartCoroutine(LiveUpdate());
            else
                StopCoroutine(_liveUpdate);
        }

        if (_toggleVoids != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleVoids, "Show voids"))
        {
            _toggleVoids = !_toggleVoids;

            foreach (var r in _voids.GetComponentsInChildren<Renderer>())
                r.enabled = _toggleVoids;
        }

        _toggleTransparency = GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleTransparency, "Transparent");

        if (_grid != null)
            _displacement = GUI.HorizontalSlider(new Rect(s, s * i++, 200, 20), _displacement, 0, 500);

    }

    void Update()
    {
        if (_grid == null) return;
        _grid.DisplacementScale = _displacement;

        foreach (var voxel in _grid.Voxels)
        {
            if (voxel.IsActive)
            {
                Drawing.DrawMesh(voxel.Mesh, voxel.Color, _toggleTransparency);
            }
        }
    }

    IEnumerator LiveUpdate()
    {
        while (true)
        {
            if (task == null || task.IsCompleted) MakeGrid();
            yield return new WaitForSeconds(1.0f);
        }
    }

    Task task;

    void MakeGrid()
    {
        var bounds = _voids
            .GetComponentsInChildren<BoxCollider>()
           .Select(v => v.bounds)
           .ToArray();

        var voxelSize = float.Parse(_voxelSize);

        task = Task.Run(() =>
       {
           _grid = new Grid3d(bounds, voxelSize);
       }).ContinueWith(t =>
       {
           foreach (var v in _grid.Voxels) v.MeshUpdate();
       }, TaskScheduler.FromCurrentSynchronizationContext());
    }
}

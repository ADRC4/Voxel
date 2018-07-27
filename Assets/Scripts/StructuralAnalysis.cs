using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StructuralAnalysis : MonoBehaviour
{
    Grid3d _grid = null;
    GameObject _voids;
    Coroutine _liveUpdate;
    bool _toggleVoids = true;
    bool _toggleUpdate = false;
    bool _toggleTransparency = false;
    float _displacement = 250f;
    string _voxelSize = "0.8";
    // Task _task;

    [SerializeField]
    GUISkin _skin;

    void Awake()
    {
        _voids = GameObject.Find("Voids");
        Physics.queriesHitBackfaces = true;
    }

    void OnGUI()
    {
        int i = 1;
        int s = 25;
        GUI.skin = _skin;

        _voxelSize = GUI.TextField(new Rect(s, s * i++, 100, 20), _voxelSize);

        if(GUI.Button(new Rect(s, s * i++, 100, 20),"Generate"))
        {
            MakeGrid();
        }

        if (_toggleUpdate != GUI.Toggle(new Rect(s, s * i++, 100, 20), _toggleUpdate, "Auto update"))
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
        Drawing.DrawMesh(_toggleTransparency, _grid.Mesh);
    }

    IEnumerator LiveUpdate()
    {
        while (true)
        {
            MakeGrid();
            yield return new WaitForSeconds(2.0f);
        }
    }

    void MakeGrid()
    {
      //  if (_task != null && !_task.IsCompleted) return;

        var colliders = _voids
                      .GetComponentsInChildren<MeshCollider>()
                      .ToArray();

        var voxelSize = float.Parse(_voxelSize);

        _grid = new Grid3d(colliders, voxelSize);
        _grid.MakeMesh();
        _grid.Analysis();

        //_task = Task.Run(() =>
        //{
        //    _grid = new Grid3d(colliders, bounds,voxelSize);
        //}).ContinueWith(_ =>
        //{
        //    _grid.MakeMesh();
        //}, TaskScheduler.FromCurrentSynchronizationContext());
    }
}

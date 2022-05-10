using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Salesman : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private List<Transform> _points = new List<Transform>();
    [SerializeField] List<Transform> _pointsOriginal = new List<Transform>();
    [SerializeField] private List<Transform> _route = new List<Transform>();
    [SerializeField] Transform _currentTarget;
    [SerializeField] private GameObject _pointPrefab;
    [SerializeField] private int _pointAmount;
    [SerializeField] private Vector2 _positionMin;
    [SerializeField] private Vector2 _positionMax;
    [SerializeField] private float _reachDistanceThreshold = 0.7f;
    bool startupRunning = true;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        InstantiatePoints();
        _pointsOriginal = Copy(_points);
        yield return new WaitForEndOfFrame();
        StartCoroutine(TeleportToRandom());
        yield return new WaitForEndOfFrame();
        _route = CalculateRoute();
        _currentTarget = _route[0];
        yield return new WaitForEndOfFrame();
        startupRunning = false;
    }

    void Update()
    {
        if (startupRunning) return;
        transform.LookAt(_currentTarget);
        transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);
    }

    List<Transform> Copy(List<Transform> original)
    {
        List<Transform> copy = new List<Transform>();
        foreach (Transform t in original)
        {
            copy.Add(t);
        }
        return copy;
    }

    IEnumerator TeleportToRandom()
    {
        GetComponentInChildren<TrailRenderer>(includeInactive: true).Clear();
        GetComponentInChildren<TrailRenderer>(includeInactive: true).emitting = false;
        Transform selected = _points[Random.Range(0, _points.Count)];
        transform.position = selected.position;
        selected.gameObject.GetComponent<MeshRenderer>().material.color = this.gameObject.GetComponent<MeshRenderer>().material.color;
        yield return new WaitForEndOfFrame();
        GetComponentInChildren<TrailRenderer>(includeInactive: true).emitting = true;
    }

    void ResetPoints()
    {
        foreach (Transform t in _pointsOriginal)
        {
            Destroy(t.gameObject);
        }
        _pointsOriginal.Clear();
    }

    void InstantiatePoints()
    {
        for (int i=0; i < _pointAmount; i++)
        {
            _points.Add(Instantiate(_pointPrefab, new Vector3(Random.Range(_positionMin.x, _positionMax.x),
                                                _pointPrefab.transform.position.y,
                                                Random.Range(_positionMin.y, _positionMax.y)), _pointPrefab.transform.rotation).transform);
        }
        _pointsOriginal = Copy(_points);
        
        CameraBehaviour.Instance.FocusPosition = GetCenterPoint();
    }

    Vector3 GetCenterPoint()
    {
        Vector3 total = Vector3.zero;

        foreach (Transform t in _points)
        {
            total += t.position;
        }

        return total / _points.Count;
    }

    List<Transform> CalculateRoute()
    {
        List<Transform> route = new List<Transform>();
        Transform first = GetClosest(this.transform);
        route.Add(first);
        _points.Remove(first);

        while (_points.Count > 0)
        {
            Transform closest = GetClosest(route[route.Count - 1]);
            route.Add(closest);
            _points.Remove(closest);
        }

        Transform last = Instantiate(_pointPrefab, first.position, first.rotation).transform;
        last.gameObject.name = "Point (LAST)";
        last.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
        _pointsOriginal.Add(last);
        route.Add(last);

        return route;
    }

    Transform GetClosest(Transform referencePoint)
    {
        Transform closest = null;
        float smallestDistance = Mathf.Infinity;

        foreach (Transform t in _points)
        {
            float distance = Vector3.Distance(referencePoint.position, t.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closest = t;
            }
        }

        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == _currentTarget)
        {
            _currentTarget.gameObject.GetComponent<MeshRenderer>().material = this.gameObject.GetComponent<MeshRenderer>().material;
            if (_route.IndexOf(_currentTarget) != _route.Count - 1)
            {
                _currentTarget = _route[_route.IndexOf(_currentTarget) + 1];
            }
            else
            {
                ResetPoints();
                InstantiatePoints();
                StartCoroutine(TeleportToRandom());
                _route = CalculateRoute();
                _currentTarget = _route[0];
            }
        }
    }
}

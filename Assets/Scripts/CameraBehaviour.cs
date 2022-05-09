using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _salesman;
    [SerializeField][Range(0.0f, 1.0f)] private float _focusInterpolationFactor = 0.5f;
    [SerializeField] private float RotationSpeed = 5;
    [SerializeField] private float LookAtSpeed = 1;
    [SerializeField] private Vector3 _additionalRotationOffset;
    public Vector3 FocusPosition = Vector3.zero;
    public static CameraBehaviour Instance;
    private Transform _pointer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _pointer = new GameObject("Pointer").transform;
    }

    void LateUpdate()
    {
        _pointer.position = transform.position;
        _pointer.LookAt(Vector3.Lerp(FocusPosition, _salesman.position, _focusInterpolationFactor));
        transform.rotation = Quaternion.Slerp(transform.rotation, _pointer.rotation, LookAtSpeed * Time.deltaTime);
        transform.RotateAround(FocusPosition, Vector3.up, RotationSpeed * Time.deltaTime);
    }
}

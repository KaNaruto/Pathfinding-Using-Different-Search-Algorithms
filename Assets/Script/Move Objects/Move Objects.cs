using UnityEngine;

public class MoveObjects : MonoBehaviour
{
    GameObject _hitObject;
    [SerializeField] LayerMask targetMask;
    private Vector3 _offset;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100, targetMask))
                {
                    _hitObject = hit.collider.gameObject;
                    _offset = _hitObject.transform.position - hit.point;
                }
            }
        }
        if (_hitObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                if (Camera.main != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                    if (groundPlane.Raycast(ray, out var rayDistance))
                    {
                        Vector3 point = ray.GetPoint(rayDistance) + _offset;
                        _hitObject.transform.position = new Vector3(point.x,5,point.z);
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var position = _hitObject.transform.position;
                position = new Vector3(position.x, 0.5f, position.z);
                _hitObject.transform.position = position;
                _hitObject = null;
            }
        }
    }

    
}

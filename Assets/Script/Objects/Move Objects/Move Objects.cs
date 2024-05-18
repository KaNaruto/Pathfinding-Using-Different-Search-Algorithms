using UnityEngine;

public class MoveObjects : MonoBehaviour
{
    private GameObject _selectedObject;
    [SerializeField] private LayerMask targetMask;
    private Vector3 _dragOffset;

    // Update is called once per frame
    void Update()
    {
        if (!FindObjectOfType<Menu>().isFinished)
        {
            HandleMouseInput();
        }
    }

    // Handle mouse input for object selection and dragging
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectObject();
        }

        if (_selectedObject != null)
        {
            if (Input.GetMouseButton(0))
            {
                DragObject();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ReleaseObject();
            }
        }
    }

    // Select the object under the mouse cursor
    private void SelectObject()
    {
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, targetMask))
            {
                _selectedObject = hit.collider.gameObject;
                _dragOffset = _selectedObject.transform.position - hit.point;
            }
        }
    }

    // Drag the selected object to follow the mouse cursor
    private void DragObject()
    {
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance) + _dragOffset;
                _selectedObject.transform.position = new Vector3(point.x, 5, point.z);
            }
        }
    }

    // Release the selected object and save its data
    private void ReleaseObject()
    {
        var position = _selectedObject.transform.position;
        position = new Vector3(position.x, 0.5f, position.z);
        _selectedObject.transform.position = position;
        FindObjectOfType<UnitManager>().SaveData();
        _selectedObject = null;
    }
}


using System.Collections;
using Script.Pathfinding;
using UnityEngine;


public class Unit : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] float speed = 5;
    [SerializeField] private float turnDistance = 1;
    [SerializeField] private float turnSpeed = 1;
    [SerializeField] private float pathUpdateMoveThreshold;
    [SerializeField] private float pathUpdateTime;
    public  bool canMove;
    private Rigidbody _rigidbody;
    private Path _path;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        StartCoroutine(UpdatePath());
    }

    
    private void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            _path = new Path(waypoints, transform.position, turnDistance);
            StopCoroutine(nameof(StartMoving));
            StartCoroutine(nameof(StartMoving));
        }
    }

    IEnumerator StartMoving()
    {
        while (true)
        {
            if (canMove)
            {
                StopCoroutine(nameof(FollowPath));
                StartCoroutine(nameof(FollowPath));
                yield break;
            }

            yield return null;
        }
    }
    

    public void OnNewPathFindAlgorithm()
    {
        StopCoroutine(UpdatePath());
        StartCoroutine(UpdatePath());
    }
    IEnumerator UpdatePath()
    {
        var position = target.position;
        PathRequestManager.RequestPath(new PathRequest(transform.position, position, OnPathFound));

        float sqrMoveThreshhold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = position;
        while (true)
        {
            yield return new WaitForSeconds(pathUpdateTime);
            if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshhold)
            {
                position = target.position;
                PathRequestManager.RequestPath((new PathRequest(transform.position, position, OnPathFound)));
                targetPosOld = position;
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(_path.LookPoints[0]);

        while (followingPath)
        {
            var position = transform.position;
            Vector2 position2D = new Vector2(position.x, position.z);
            while (_path.TurnBoundaries[pathIndex].HasCrossedLine(position2D))
            {
                if (pathIndex == _path.FinishLineIndex)
                {
                    FindObjectOfType<Menu>().start = false;
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;
            }

            if (followingPath)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_path.LookPoints[pathIndex] - transform.position);
                _rigidbody.MoveRotation(
                    Quaternion.Lerp(_rigidbody.rotation, targetRotation, Time.deltaTime * turnSpeed));
                _rigidbody.MovePosition(_rigidbody.position + _rigidbody.transform.forward * Time.deltaTime * speed);
            }

            yield return null;
        }
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
            _path.DrawWithGizmos();
    }
}
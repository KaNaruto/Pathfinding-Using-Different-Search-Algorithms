
using System.Diagnostics;
using Script.Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown algorithms;
    [SerializeField] private TextMeshProUGUI elapsedTime;

    public bool start;
    private readonly Stopwatch _stopwatch = new Stopwatch();

    private void Update()
    {
        
        if (start)
        {
            long elapsedTimeInMs = _stopwatch.ElapsedMilliseconds;
            float elapsedTimeInS = elapsedTimeInMs / 1000.0f;
            string formattedTime = elapsedTimeInS.ToString("F2");
            elapsedTime.text = formattedTime + " \nseconds";
        }
        else
            _stopwatch.Stop();
            
        
    }

    public void OnStart()
    {
        if(!start)
        {
            algorithms.interactable = false;
            Unit[] units = FindObjectsOfType<Unit>();
            foreach (Unit unit in units)
            {
                unit.canMove = true;
            }

            start = true;
            _stopwatch.Restart();
        }
    }

    public void OnReset()
    {
        SceneManager.LoadScene("Algorithm");
    }

    public void OnPause()
    {
        Time.timeScale = 0;
        _stopwatch.Stop();
    }

    public void OnResume()
    {
        Time.timeScale = 1;
        _stopwatch.Start();
    }

    public void OnNewAlgorithm(int value)
    {
        PathRequestManager.Instance.SetAlgorithm(value);
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            unit.OnNewPathFindAlgorithm();
        }
    }
    
}
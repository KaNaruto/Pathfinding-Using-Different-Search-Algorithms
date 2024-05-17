using System.Diagnostics;
using Script.Map;
using Script.Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Grid = Script.Pathfinding.Grid;
using Slider = UnityEngine.UI.Slider;


public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown algorithms;
    [SerializeField] private TextMeshProUGUI elapsedTime;

    [SerializeField] private TMP_InputField nodeRadiusInput;

    [SerializeField] private Slider obstacleRateInput;
    [SerializeField] private TMP_InputField xSizeInput;
    [SerializeField] private TMP_InputField ySizeInput;
    [SerializeField] private TMP_InputField seedInput;

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
        if (!start)
        {
            algorithms.interactable = false;
            nodeRadiusInput.interactable = false;
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
        FindNewPath();
    }

    public void OnNewRadius(string radius)
    {
        float nodeRadius = float.Parse(radius);
        FindObjectOfType<Grid>().GridProperties(nodeRadius);
        FindNewPath();
    }

    private void FindNewPath()
    {
        Unit[] units = FindObjectsOfType<Unit>();
        foreach (Unit unit in units)
        {
            unit.OnNewPathFindAlgorithm();
        }
    }

    public void DisplayGrizmos(bool value)
    {
        FindObjectOfType<Grid>().displayGridGizmos = value;
    }

    public void OnObstacleRateChanged()
    {
        CreateNewMap();
    }

    public void OnXSizeChanged(string size)
    {
        if (size.Length == 0)
        {
            xSizeInput.text = "0";
        }

        CreateNewMap();
    }

    public void OnYSizeChanged(string size)
    {
        if (size.Length == 0)
        {
            ySizeInput.text = "0";
        }

        CreateNewMap();
    }

    public void OnSeedChanged(string seed)
    {
        if (seed.Length == 0)
        {
            seedInput.text = "0";
        }

        CreateNewMap();
    }

    private void CreateNewMap()
    {
        int x = int.Parse(xSizeInput.text);
        int y = int.Parse(ySizeInput.text);
        int seed = int.Parse(seedInput.text);
        float obstacleRate = obstacleRateInput.value;

        FindObjectOfType<MapGenerator>().SetProperties(x, y, seed, obstacleRate);

        FindObjectOfType<Grid>().GridProperties(x, y);
        FindNewPath();
    }
}
using System;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Script.Map;
using Script.Pathfinding;
using Grid = Script.Pathfinding.Grid;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown algorithmDropdown;
    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private TMP_InputField nodeRadiusInput;
    [SerializeField] private Slider obstacleRateSlider;
    [SerializeField] private TMP_InputField xSizeInput;
    [SerializeField] private TMP_InputField ySizeInput;
    [SerializeField] private TMP_InputField seedInput;
    [SerializeField] private CanvasGroup optionsPanel;
    public bool isFinished;
    private bool _isStarted;

    private readonly Stopwatch _stopwatch = new Stopwatch();

    private void Start()
    {
        MapData? mapData = SaveSystem.LoadMapData();
        if (mapData != null)
        {
            InitializeSettings(mapData.Value);
        }
    }

    private void InitializeSettings(MapData mapData)
    {
        xSizeInput.text = mapData.xSize.ToString();
        ySizeInput.text = mapData.ySize.ToString();
        obstacleRateSlider.value = mapData.obstacleRate;
        seedInput.text = mapData.seed.ToString();
    }

    private void Update()
    {
        if (isFinished)
        {
            UpdateElapsedTime();
        }
        else
        {
            _stopwatch.Stop();
        }
    }

    private void UpdateElapsedTime()
    {
        long elapsedTimeInMs = _stopwatch.ElapsedMilliseconds;
        float elapsedTimeInSeconds = elapsedTimeInMs / 1000.0f;
        elapsedTimeText.text = $"{elapsedTimeInSeconds:F2} seconds";
    }

    public void OnStart()
    {
        if (!isFinished)
        {
            FindObjectOfType<UnitManager>().SaveData();
            SetInteractable(false);
            EnableUnitMovement(true);
            isFinished = true;
            _isStarted = true;
            _stopwatch.Restart();
        }
    }

   

    public void OnReset()
    {
        if(!_isStarted)
            FindObjectOfType<UnitManager>().SaveData();
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
        FindObjectOfType<UnitManager>().FindNewPath();
    }

    public void OnNewRadius(string radius)
    {
        float nodeRadius = float.Parse(radius);
        FindObjectOfType<Grid>().GridProperties(nodeRadius);
        FindObjectOfType<UnitManager>().FindNewPath();
    }

    public void DisplayGizmos(bool value)
    {
        FindObjectOfType<Grid>().displayGridGizmos = value;
    }

    public void OnObstacleRateChanged()
    {
        CreateNewMap();
    }

    public void OnXSizeChanged(string size)
    {
        if (string.IsNullOrEmpty(size))
        {
            xSizeInput.text = "0";
        }
        CreateNewMap();
    }

    public void OnYSizeChanged(string size)
    {
        if (string.IsNullOrEmpty(size))
        {
            ySizeInput.text = "0";
        }
        CreateNewMap();
    }

    public void OnSeedChanged(string seed)
    {
        if (string.IsNullOrEmpty(seed))
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
        float obstacleRate = obstacleRateSlider.value;

        var mapGenerator = FindObjectOfType<MapGenerator>();
        var grid = FindObjectOfType<Grid>();

        mapGenerator.SetProperties(x, y, seed, obstacleRate);
        grid.GridProperties(x, y);
    }

    public void AddUnit()
    {
        FindObjectOfType<UnitManager>().AddUnit();
    }

    public void RemoveUnit()
    {
        FindObjectOfType<UnitManager>().RemoveUnit();
    }

    private void SetInteractable(bool interactable)
    {
        algorithmDropdown.interactable = interactable;
        optionsPanel.interactable = interactable;
    }

    private void EnableUnitMovement(bool canMove)
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            unit.canMove = canMove;
        }
    }
}

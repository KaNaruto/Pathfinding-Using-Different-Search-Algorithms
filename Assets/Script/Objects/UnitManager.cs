using System.Collections.Generic;
using System.Linq;
using Script.Map;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private GameObject targetPrefab;
    private Stack<Unit> _units;
    private Transform _target;

    private void Awake()
    {
        InitializeTarget();
        LoadUnitData();
        InitializeUnits();
    }

    // Initialize the target object
    private void InitializeTarget()
    {
        GameObject target = Instantiate(targetPrefab, new Vector3(0, 0.5f, 0), Quaternion.identity);
        _target = target.transform;
        FindObjectOfType<MapGenerator>().target = _target;
    }

    // Load saved unit data and instantiate units if available
    private void LoadUnitData()
    {
        _units = new Stack<Unit>();
        UnitData? unitData = SaveSystem.LoadUnitData();
        if (unitData != null)
        {
            _target.position = unitData.Value.targetPosition;
            InstantiateUnitsAtStart(unitData.Value);
        }
    }

    // Initialize units in the scene
    private void InitializeUnits()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Unit unit = transform.GetChild(i).gameObject.GetComponent<Unit>();
            _units.Push(unit);
        }
    }

    // Instantiate units at start based on saved data
    private void InstantiateUnitsAtStart(UnitData unitData)
    {
        foreach (UnitData.UnitInfo unitInfo in unitData.units)
        {
            Unit instantiatedUnit = Instantiate(unitPrefab, unitInfo.position, Quaternion.identity);
            instantiatedUnit.transform.parent = transform;
            instantiatedUnit.target = _target;
        }
    }

    // Save current unit data
    public void SaveData()
    {
        List<UnitData.UnitInfo> unitsInfo =
            _units.Select(u => new UnitData.UnitInfo { position = u.transform.position }).ToList();
        UnitData unitData = new UnitData(unitsInfo, _target.position);
        SaveSystem.SaveUnitData(unitData);
    }

    // Add a new unit to the scene
    public void AddUnit()
    {
        Unit unit = Instantiate(unitPrefab, transform.position, Quaternion.identity);
        unit.transform.parent = transform;
        unit.target = _target;
        _units.Push(unit);
        SaveData();
    }

    // Remove a unit from the scene
    public void RemoveUnit()
    {
        if (_units.Count > 0)
        {
            Unit unit = _units.Pop();
            Destroy(unit.gameObject);
            SaveData();
        }
    }

    // Find a new path for all units
    public void FindNewPath()
    {
        if (_units != null)
            foreach (Unit unit in _units)
            {
                unit.FindNewPath();
            }
    }
}
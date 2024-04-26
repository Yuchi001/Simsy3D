using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using SideClasses;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float decreaseSpeed = 5;
    
    private readonly List<NeedTracker> _needTrackers = new();

    private NeedTracker _objective;

    private bool _canChangeTarget = false;
    private bool _isCloseToObjective = false;
    private float _timer = 0f;

    private NavMeshAgent _agent;
    
    public void Setup(List<NeedObject> needTuples)
    {
        foreach (var needObject in needTuples)
        {
            _needTrackers.Add(new NeedTracker()
            {
                value = needObject.maxValue,
                needObject = needObject,
            });
        }

        _objective = _needTrackers[0];

        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _isCloseToObjective =
            Vector3.Distance(transform.position, _objective.needObject.target.transform.position) <= 2;

        ManageStats();
        
        if (_isCloseToObjective)
        {
            var objectiveTracker =
                _needTrackers.FirstOrDefault(n => n.needObject.needType == _objective.needObject.needType);

            if (objectiveTracker == default) return;
            objectiveTracker.value += decreaseSpeed * 4 * Time.deltaTime;

            _canChangeTarget = objectiveTracker.value >= objectiveTracker.needObject.maxValue;
            
            return;
        }

        _agent.destination = _objective.needObject.target.transform.position;
    }

    private void ManageStats()
    {
        _timer += Time.deltaTime;
        var smallestValue = _objective;
        foreach (var tracker in _needTrackers)
        {
            var isTargetObjective = _objective.needObject.needType == tracker.needObject.needType && _isCloseToObjective;
            tracker.value -= isTargetObjective ? 0 : decreaseSpeed * Time.deltaTime;

            if (tracker.value / tracker.needObject.maxValue < smallestValue.value / smallestValue.needObject.maxValue) smallestValue = tracker;
        }

        if ((!_canChangeTarget &&
             !(smallestValue.value / smallestValue.needObject.maxValue <= 0.5f))
            || _timer < 2f) return;

        _canChangeTarget = false;
        _objective = smallestValue;
        _timer = 0;
    }

    public float GetNeedValue(ENeedType needType)
    {
        var needTracker = _needTrackers.FirstOrDefault(n => n.needObject.needType == needType);
        if (needTracker != null) return needTracker.value / needTracker.needObject.maxValue;

        Debug.LogError("Need not found!");
        return -1;
    }

    private class NeedTracker
    {
        public float value;
        public NeedObject needObject;
    }
}
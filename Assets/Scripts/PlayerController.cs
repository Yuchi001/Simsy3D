﻿using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using SideClasses;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float decreaseSpeed = 5;
    [SerializeField] private Animator animator;
    
    private readonly List<NeedTracker> _needTrackers = new();

    private NeedTracker _objective;

    private bool _isDead = false;
    private bool _canChangeTarget = false;
    private bool _isCloseToObjective = false;
    private float _timer = 0f;

    private NavMeshAgent _agent;

    private AudioManager _audioManager => AudioManager.Instance;

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
        if (_isDead) return;
        
        var objectiveTracker =
            _needTrackers.FirstOrDefault(n => n.needObject.needType == _objective.needObject.needType);
        if (objectiveTracker == default) return;
        
        var objectivePos = objectiveTracker.needObject.target.transform.position;
        _agent.isStopped = Vector3.Distance(transform.position, objectivePos) <= 2;
        _agent.destination = objectivePos;
        
        animator.SetBool("Walking", !_agent.isStopped);
        
        ManageStats();

        if (!_agent.isStopped) return;
        
        objectiveTracker.value += decreaseSpeed * 4 * Time.deltaTime;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(objectiveTracker.needObject.animationName))
        {
            var soundType = objectiveTracker.needObject.activitySoundType;
            if(!_audioManager.IsPlaying(soundType)) _audioManager.PlaySound(soundType,  true);
            animator.SetTrigger(objectiveTracker.needObject.animationName);
        }
        
        _canChangeTarget = objectiveTracker.value >= objectiveTracker.needObject.maxValue;

        var targetTransform = objectiveTracker.needObject.posAndRot;
        if (targetTransform == null) return;
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }

    private void ManageStats()
    {
        _timer += Time.deltaTime;
        var smallestValue = _objective;
        foreach (var tracker in _needTrackers)
        {
            var isTargetObjective = _objective.needObject.needType == tracker.needObject.needType && _isCloseToObjective;
            tracker.value -= isTargetObjective ? 0 : decreaseSpeed * Time.deltaTime;

            if (tracker.value <= 0)
            {
                Die();
                return;
            }

            if (tracker.value / tracker.needObject.maxValue < smallestValue.value / smallestValue.needObject.maxValue) smallestValue = tracker;
        }

        if ((!_canChangeTarget &&
             !(smallestValue.value / smallestValue.needObject.maxValue <= 0.5f))
            || _timer < 2f) return;

        _audioManager.StopPlayingSound(_objective.needObject.activitySoundType);
        
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

    private void Die()
    {
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
        
        animator.SetBool("Dead", _isDead = true);
        animator.SetTrigger("Die");
        
        GameManager.Instance.OnPlayerDeath();
    }

    private class NeedTracker
    {
        public float value;
        public NeedObject needObject;
    }
}
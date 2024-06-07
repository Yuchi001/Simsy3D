using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using SideClasses;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float fillNeedPercentageGap = 0.6f;
    [SerializeField] private float decreaseSpeed = 5;
    [SerializeField] private float increaseSpeed = 20;
    [SerializeField] private Animator animator;
    
    private readonly List<NeedTracker> _needTrackers = new();

    private NeedTracker _objective;

    private bool _isDead = false;

    private NavMeshAgent _agent;

    private Stack<NeedTracker> _needQueue = new();

    private bool IsIdle => animator.GetBool("Idle");

    private static AudioManager AudioManager => AudioManager.Instance;

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

        _objective = null;

        _agent = GetComponent<NavMeshAgent>();
        
        animator.SetBool("Idle", true);
        animator.SetTrigger("GoIdle");
    }

    private void Update()
    {
        if (_isDead) return;

        ManageStats();
        if (IsIdle || _objective == null)
        {
            _agent.isStopped = true;
            return;
        }
        
        var objectiveTracker =
            _needTrackers.FirstOrDefault(n => n.needObject.needType == _objective.needObject.needType);
        if (objectiveTracker == default) return;
        
        var objectivePos = objectiveTracker.needObject.target.transform.position;
        _agent.isStopped = Vector3.Distance(transform.position, objectivePos) <= 2;
        _agent.destination = objectivePos;
        
        animator.SetBool("Walking", !_agent.isStopped);

        if (!_agent.isStopped && !IsIdle) return;

        objectiveTracker.value += increaseSpeed * Time.deltaTime;
        objectiveTracker.value = Mathf.Clamp(objectiveTracker.value, 0, objectiveTracker.needObject.maxValue);

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(objectiveTracker.needObject.animationName))
        {
            var soundType = objectiveTracker.needObject.activitySoundType;
            if(!AudioManager.IsPlaying(soundType)) AudioManager.PlaySound(soundType,  true);
            animator.SetTrigger(objectiveTracker.needObject.animationName);
        }
        
        var targetTransform = objectiveTracker.needObject.posAndRot;
        if (targetTransform == null) return;
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }

    private void ManageStats()
    {
        foreach (var tracker in _needTrackers)
        {
            var isTargetObjective = !IsIdle && _objective.needObject.needType == tracker.needObject.needType;
            tracker.value -= isTargetObjective ? 0 : decreaseSpeed * Time.deltaTime;
            
            if (tracker.IsCritical()) _needQueue.Push(tracker);
            
            if (tracker.value <= 0) Die();
        }

        if (IsIdle)
        {
            if (_needQueue.Count <= 0) return;
            
            if (_objective != null) AudioManager.StopPlayingSound(_objective.needObject.activitySoundType);
            _objective = _needQueue.Pop();
            animator.SetBool("Idle", false);
            return;
        }

        var currentPercentage = _objective.GetCurrentPercentage();
        if (currentPercentage <= fillNeedPercentageGap) return;

        if (_needQueue.Count > 0)
        {
            if (_objective != null) AudioManager.StopPlayingSound(_objective.needObject.activitySoundType);
            _objective = _needQueue.Pop();
            animator.SetBool("Idle", false);
            return;
        }

        if (currentPercentage < 0.95f) return;

        if (_objective != null) AudioManager.StopPlayingSound(_objective.needObject.activitySoundType);
        if(!animator.GetBool("Idle")) animator.SetTrigger("GoIdle");
        animator.SetBool("Idle", true);
        _objective = null;
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

        public float GetCurrentPercentage()
        {
            return value / needObject.maxValue;
        }

        public bool IsCritical()
        {
            return GetCurrentPercentage() < needObject.triggerPercentage;
        }
    }
}
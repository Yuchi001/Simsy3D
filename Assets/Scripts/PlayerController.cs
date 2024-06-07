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
        // Zrobienie listy needow
        foreach (var needObject in needTuples)
        {
            _needTrackers.Add(new NeedTracker()
            {
                value = needObject.maxValue,
                needObject = needObject,
            });
        }

        // Objective jest na null bo postac na poczatku jest w stanie idle wiec nie ma zadnego celu
        _objective = null;

        _agent = GetComponent<NavMeshAgent>();
        
        animator.SetBool("Idle", true);
        animator.SetTrigger("GoIdle");
    }

    private void Update()
    {
        if (_isDead) return;

        ManageStats();
        
        // Jezeli postac nie ma co robic albo jest w stanie "Idle" to oznacza ze musi stac
        if (IsIdle || _objective == null)
        {
            _agent.isStopped = true;
            return;
        }
        
        // Aktualny tracker
        var objectiveTracker =
            _needTrackers.FirstOrDefault(n => n.needObject.needType == _objective.needObject.needType);
        if (objectiveTracker == default) return;
        
        var objectivePos = objectiveTracker.needObject.target.transform.position;
        _agent.isStopped = Vector3.Distance(transform.position, objectivePos) <= 2;
        _agent.destination = objectivePos;
        
        // Wlacz/wylacz animacje chodzenia w zaleznosci czy postac stoi czy nie
        animator.SetBool("Walking", !_agent.isStopped);

        if (!_agent.isStopped && !IsIdle) return;

        objectiveTracker.value += increaseSpeed * Time.deltaTime;
        objectiveTracker.value = Mathf.Clamp(objectiveTracker.value, 0, objectiveTracker.needObject.maxValue);

        // Jesli nie ma granej animacji aktualnej czynnosci to znaczy ze postac dopiero zaczyna robic dana czynnosc wiec pojawiam dzwiek i triggeruje animacje
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(objectiveTracker.needObject.animationName))
        {
            var soundType = objectiveTracker.needObject.activitySoundType;
            if(!AudioManager.IsPlaying(soundType)) AudioManager.PlaySound(soundType,  true);
            animator.SetTrigger(objectiveTracker.needObject.animationName);
        }
        
        // jesli aktualny need ma okreslona pozycje i rotacje to ja ustawiam
        var targetTransform = objectiveTracker.needObject.posAndRot;
        if (targetTransform == null) return;
        
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }

    private void ManageStats()
    {
        // Dla kazdego trackera zmniejsz jego wartosc (chyba ze jest objectivem), sprawdz czy jego stan jest krytyczny i czy postac nie umarla
        foreach (var tracker in _needTrackers)
        {
            var isTargetObjective = !IsIdle && _objective.needObject.needType == tracker.needObject.needType;
            tracker.value -= isTargetObjective ? 0 : decreaseSpeed * Time.deltaTime;
            
            // Dodaj need do kolejki jesli nie ma go ani w kolejce ani nie jest glownym objectivem poki co
            if (tracker.IsCritical() && 
                !IsPresentInStack(tracker) && 
                !IsMainObjective(tracker)) 
                _needQueue.Push(tracker);
            
            if (tracker.value <= 0) Die();
        }

        // Jezeli postac jest w stanie "Idle" i kolejka nie jest pusta ustaw objective na pierwszy need z brzegu kolejki
        if (IsIdle)
        {
            if (_needQueue.Count <= 0) return;
            
            if (_objective != null) AudioManager.StopPlayingSound(_objective.needObject.activitySoundType);
            _objective = _needQueue.Pop();
            animator.SetBool("Idle", false);
            return;
        }

        // Jezeli aktualny need nie jest zrobiony do minimum to rob go dalej
        var currentPercentage = _objective.GetCurrentPercentage();
        if (currentPercentage <= fillNeedPercentageGap) return;

        // Jezeli kolejka nie jest pusta to ustaw objective na pierwszy need z brzegu kolejki
        if (_needQueue.Count > 0)
        {
            if (_objective != null) AudioManager.StopPlayingSound(_objective.needObject.activitySoundType);
            _objective = _needQueue.Pop();
            animator.SetBool("Idle", false);
            return;
        }

        // Jezeli aktualny need nie jest zrobiony prawie do maxa do rob go dalej
        if (currentPercentage < 0.95f) return;

        // Tutaj ostatecznie ustawiamy "Idle" postaci bo nie ma co robic
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

    private bool IsPresentInStack(NeedTracker tracker)
    {
        return _needQueue.FirstOrDefault(n => n.needObject.needType == tracker.needObject.needType) != default;
    }

    private bool IsMainObjective(NeedTracker tracker)
    {
        return _objective != null && _objective.needObject.needType == tracker.needObject.needType;
    }

    private void Die()
    {
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
        
        animator.SetBool("Dead", _isDead = true);
        animator.SetTrigger("Die");
        
        GameManager.Instance.OnPlayerDeath();
    }

    // Tutaj jest wartosc samego needa jak i jego glowne dane
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
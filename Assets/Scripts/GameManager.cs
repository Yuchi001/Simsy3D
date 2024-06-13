using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using SideClasses;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField, Range(0.1f, 2f)] private float needMaxValueScale = 1f;

    [SerializeField] private float deathMenuDelay = 3f;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPos;
    [SerializeField] private List<NeedObject> needObjects;
    [SerializeField] private List<NeedDisplay> needDisplays;
    
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        var player = Instantiate(playerPrefab, playerSpawnPos.position, Quaternion.identity);
        var playerScript = player.GetComponent<PlayerController>(); 
        needObjects.ForEach(n => n.maxValue *= needMaxValueScale);
        playerScript.Setup(needObjects);

        foreach (var needDisplay in needDisplays)
        {
            needDisplay.Setup(playerScript);
        }
        
        AudioManager.Instance.SetTheme(EThemeType.Ambient1);
    }

    public void OnPlayerDeath()
    {
        StartCoroutine(DelayDeathMenu());
    }

    private IEnumerator DelayDeathMenu()
    {
        yield return new WaitForSeconds(deathMenuDelay);
        deathUI.SetActive(true);
    }

    public void LoadScene(ESceneType sceneType)
    {
        SceneManager.LoadScene((int)sceneType);
    }
} 
using System;
using System.Collections.Generic;
using SideClasses;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPos;
        [SerializeField] private List<NeedObject> needObjects;
        [SerializeField] private List<NeedDisplay> needDisplays;

        private void Start()
        {
            var player = Instantiate(playerPrefab, playerSpawnPos.position, Quaternion.identity);
            var playerScript = player.GetComponent<PlayerController>(); 
            playerScript.Setup(needObjects);

            foreach (var needDisplay in needDisplays)
            {
                needDisplay.Setup(playerScript);
            }
        }
    }
}
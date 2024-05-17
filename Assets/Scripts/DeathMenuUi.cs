using Enums;
using UnityEngine;

public class DeathMenuUi : MonoBehaviour
{
    public void ReturnToMenu()
    {
        GameManager.Instance.LoadScene(ESceneType.Menu);
    }       
}
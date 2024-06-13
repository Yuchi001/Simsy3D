using Enums;
using UnityEngine;

public class DeathMenuUi : MonoBehaviour
{
    [SerializeField] private GameObject needsUi;

    private void Awake()
    {
        needsUi.SetActive(false);
    }

    public void ReturnToMenu()
    {
        GameManager.Instance.LoadScene(ESceneType.Menu);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
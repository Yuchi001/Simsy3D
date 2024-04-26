using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeedDisplay : MonoBehaviour
{
    [SerializeField] private ENeedType needType;
    [SerializeField] private TextMeshProUGUI slotText;
    [SerializeField] private Image slotImage;

    private PlayerController _player;

    public void Setup(PlayerController player)
    {
        _player = player;
        slotText.text = needType.ToString();
    }

    private void Update()
    {
        slotImage.fillAmount = _player.GetNeedValue(needType);
    }
}
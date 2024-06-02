using Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Options"), Space(5)]
        [SerializeField] private float maxScale = 1.3f;
        [SerializeField] private float animTime = 0.2f;
        
        [Space(10)]
        
        [Header("Events"), Space(5)]
        [SerializeField] private UnityEvent onHoverEvent;
        [SerializeField] private UnityEvent onClickEvent;
        

        public void OnPointerEnter(PointerEventData eventData)
        {
            var finishScale = new Vector3(maxScale, maxScale, 1);
            transform.LeanScale(finishScale, animTime).setEaseOutBack();
            onHoverEvent?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.LeanScale(Vector3.one, animTime).setEaseOutBack();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySoundOneShoot(ESoundType.ButtonClick);
            transform.LeanScale(Vector3.one, animTime).setEaseOutBack().setOnComplete(() => onClickEvent?.Invoke());
        }
    }
}
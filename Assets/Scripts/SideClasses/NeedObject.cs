using DefaultNamespace;
using Enums;
using UnityEngine;

namespace SideClasses
{
    [System.Serializable]
    public class NeedObject
    {
        public string animationName;
        public ENeedType needType;
        public ESoundType activitySoundType;
        public GameObject target;
        public Transform posAndRot;
        public float maxValue;
        [Range(0f, 1f)] public float triggerPercentage;
    }
}
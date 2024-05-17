using DefaultNamespace;
using UnityEngine;

namespace SideClasses
{
    [System.Serializable]
    public class NeedObject
    {
        public string animationName;
        public ENeedType needType;
        public GameObject target;
        public Transform posAndRot;
        public float maxValue;
    }
}
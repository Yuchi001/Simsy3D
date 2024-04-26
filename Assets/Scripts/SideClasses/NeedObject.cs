using DefaultNamespace;
using UnityEngine;

namespace SideClasses
{
    [System.Serializable]
    public class NeedObject
    {
        public ENeedType needType;
        public GameObject target;
        public float maxValue;
    }
}
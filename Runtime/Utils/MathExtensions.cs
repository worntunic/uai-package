using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public static class MathExtensions {
        public static int RoundOutToInt(this float number)
        {
            if (number >= 0) {
                return Mathf.CeilToInt(number);
            } else
            {
                return Mathf.FloorToInt(number);
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace UAI.Edit.Extensions
{
    public class LimitedFloatField : FloatField
    {
        public float maxValue = 0;
        public float minValue = 1;

        public LimitedFloatField(string label) : base(label)
        {

        }
        public LimitedFloatField(int maxCharacters): base(maxCharacters)
        {

        }
        public LimitedFloatField(float minValue, float maxValue): base()
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public LimitedFloatField(string label, float minValue, float maxValue) : base(label)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        public override float value {
            get => base.value;
            set {
                value = Mathf.Clamp(value, minValue, maxValue);
                base.value = value;
            }
        }
        /*public override void SetValueWithoutNotify(float newValue)
        {
            newValue = Mathf.Clamp(newValue, minValue, maxValue);
            base.SetValueWithoutNotify(newValue);
        }*/
    }
}
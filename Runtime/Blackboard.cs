using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace SimpleAI {
    public class Blackboard {
        [StructLayout(LayoutKind.Explicit)]
        struct Value {
            [FieldOffset(0)]
            public int AsInt;
            [FieldOffset(0)]
            public float AsFloat;
        }


        List<BlackboardKey> Keys = new List<BlackboardKey>();
        List<Value> Values = new List<Value>();

        public void SetFloat(BlackboardKey key, float val) {
            Assert.IsNotNull(key);

            var newValue = new Value() { AsFloat = val };

            for (int i = 0; i < Keys.Count; ++i) {
                if (Keys[i] == key) {
                    Values[i] = newValue;
                    return;
                }
            }

            Keys.Add(key);
            Values.Add(newValue);
        }

        public float GetFloat(BlackboardKey key) {
            for (int i = 0; i < Keys.Count; ++i) {
                if (Keys[i] == key)
                    return Values[i].AsFloat;
            }
            return default(float);
        }
    }
}
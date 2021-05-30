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

        Dictionary<BlackboardKey, Value> Data = new Dictionary<BlackboardKey, Value>();

        public void SetFloat(BlackboardKey key, float val) {
            Assert.IsNotNull(key);

            Data[key] = new Value() { AsFloat = val };
        }

        public float GetFloat(BlackboardKey key) {
            if (!Data.TryGetValue(key, out Value val))
                return 0;

            return val.AsFloat;
        }
    }
}
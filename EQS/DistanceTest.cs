using System;
using UnityEngine;

namespace SimpleAI.EQS {
    public enum DistanceTestMode {
        PreferGreater,
        PreferLower,
        PeferExact
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp")]
    [Serializable]
    public class DistanceTest : ITest {
        public QueryContext To;
        [Range(1, 1000)]
        public float MaxDistance = 100;
        public DistanceTestMode Mode = DistanceTestMode.PreferGreater;

        public float RuntimeCost => 1;

        public float Run(ref Item item, QueryRunContext ctx) {
            var from = item.Point;
            var to = ctx.Resolve(To);
            var distance = (to - from).magnitude;
            if (Mode == DistanceTestMode.PeferExact) {
                var a = Mathf.Clamp01(Mathf.Abs(distance - MaxDistance) / MaxDistance);
                return 1f - a;
            }
            else {
                var a = Mathf.Clamp01(distance / MaxDistance);
                if (Mode == DistanceTestMode.PreferLower) {
                    a = 1f - a;
                }
                return a;
            }
        }
    }
}
using System;
using UnityEngine;

namespace SimpleAI.EQS {
    public enum LinecastTestMode {
        RequireHit,
        RequireMiss
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp")]
    [Serializable]
    public class LinecastTest : ITest {
        public QueryContext To;
        public LayerMask LinecastMask;
        public LinecastTestMode Mode = LinecastTestMode.RequireHit;
        public Vector3 Offset;

        public float RuntimeCost => 10;

        public float Run(ref Item item, QueryRunContext ctx) {
            var from = item.Point + Offset;
            var to = ctx.Resolve(To) + Offset;
            
            var hit = Physics.Linecast(from, to, LinecastMask);
            if (Mode == LinecastTestMode.RequireHit) {
                hit = !hit;
            }
            return hit ? 0f : 1f;
        }
    }
}
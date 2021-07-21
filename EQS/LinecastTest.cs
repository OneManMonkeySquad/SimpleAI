using System;
using UnityEngine;

namespace SimpleAI.EQS {
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp")]
    [Serializable]
    public class LinecastTest : ITest {
        public enum Score {
            None,
            Closest,
            Furthest
        }

        public enum Require {
            Hit,
            Miss
        }

        public QueryContext To;
        public LayerMask LinecastMask;
        public Vector3 Offset;

        [Header("Filter")]
        public Require Mode = Require.Hit;

        [Header("Score")]
        public Score ScoreMode = Score.None;

        public float RuntimeCost => 10;

        public float Run(ref Item item, QueryRunContext ctx) {
            var from = item.Point + Offset;
            var to = ctx.Resolve(To) + Offset;

            var hit = Physics.Linecast(from, to, out RaycastHit hitInfo, LinecastMask);
            if ((hit && Mode == Require.Miss) || (!hit && Mode == Require.Hit))
                return 0;

            if (ScoreMode == Score.None)
                return 1;

            var maxDist = (from - to).magnitude;

            if (ScoreMode == Score.Closest)
                return 1 - (hitInfo.distance / maxDist);

            return hitInfo.distance / maxDist;
        }
    }
}
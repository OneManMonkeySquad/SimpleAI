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
            var fromWithOffset = item.Point + Offset;
            var tos = ctx.Resolve(To);

            var c = 1f;
            foreach (var to in tos) {
                var toWithOffset = to + Offset;

                var hit = Physics.Linecast(fromWithOffset, toWithOffset, out RaycastHit hitInfo, LinecastMask);
                if ((hit && Mode == Require.Miss) || (!hit && Mode == Require.Hit))
                    return 0;

                if (ScoreMode != Score.None) {
                    var maxDist = (fromWithOffset - toWithOffset).magnitude;
                    if (ScoreMode == Score.Closest) {
                        c *= 1 - (hitInfo.distance / maxDist);
                    } else {
                        c *= hitInfo.distance / maxDist;
                    }
                }
            }
            return c;
        }
    }
}
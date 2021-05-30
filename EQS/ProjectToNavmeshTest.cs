using System;
using UnityEngine.AI;

namespace SimpleAI.EQS {
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp")]
    [Serializable]
    public class ProjectToNavmeshTest : ITest {
        public float MaxDistance = 1;

        public float RuntimeCost => 4;

        public float Run(ref Item item, QueryRunContext ctx) {
            var isValid = NavMesh.SamplePosition(item.Point, out NavMeshHit hit, MaxDistance, NavMesh.AllAreas);
            if (!isValid)
                return 0;

            item.Point = hit.position;
            return 1;
        }
    }
}
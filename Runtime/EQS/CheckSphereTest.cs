using System;
using UnityEngine;

namespace SimpleAI.EQS {
    [Serializable]
    public class CheckSphereTest : ITest {
        public enum ModType {
            RequireHit,
            RequireMiss
        }

        public LayerMask PhysicsMask;
        public Vector3 Offset;
        public float Radius = 0.5f;

        [Header("Filter")]
        public ModType Mode = ModType.RequireHit;

        public float RuntimeCost => 9;

        public float Run(ref Item item, ResolvedQueryRunContext ctx) {
            var fromWithOffset = item.Point + Offset;

            var hit = Physics.CheckSphere(fromWithOffset, Radius, PhysicsMask);
            if ((hit && Mode == ModType.RequireMiss) || (!hit && Mode == ModType.RequireHit))
                return 0;
            return 1;
        }
    }
}
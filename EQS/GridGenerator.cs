using System;
using UnityEngine;

namespace SimpleAI.EQS {
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, null, "Assembly-CSharp")]
    [Serializable]
    public class GridGenerator : IGenerator {
        [Range(1, 20)]
        public int Radius = 4;
        [Range(0.1f, 10f)]
        public float Padding = 1;

        public int GenerateItemsNonAlloc(QueryContext around, QueryRunContext ctx, Item[] items) {
            int num = 0;

            var p = ctx.Resolve(around);
            for (int x = -Radius; x <= Radius; ++x) {
                for (int y = -Radius; y <= Radius; ++y) {
                    if (num >= items.Length) {
                        Debug.LogWarning("Exhausted number of items");
                        break;
                    }

                    items[num].Point = p + new Vector3(x * Padding, 0, y * Padding);
                    ++num;
                }
            }
            return num;
        }
    }
}
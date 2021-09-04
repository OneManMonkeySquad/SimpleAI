using System;
using UnityEngine;

namespace SimpleAI.EQS {
    /// <summary>
    /// Generates points around a center point in a donut shape.
    /// </summary>
    [Serializable]
    public class DonutGenerator : IGenerator {
        [Range(0, 50)]
        public int MinRadius = 4;
        [Range(0, 50)]
        public int MaxRadius = 6;
        [Range(1, 5)]
        public int RadiusIncrements = 2;
        [Range(15, 90)]
        public float AngleIncrements = 30;

        public int GenerateItemsNonAlloc(QueryContext around, QueryRunContext ctx, Item[] items) {
            int num = 0;

            var centers = ctx.Resolve(around);
            foreach (var center in centers) {
                for (int radius = MinRadius; radius <= MaxRadius; radius += RadiusIncrements) {
                    for (float angle = 0; angle < 360; angle += AngleIncrements) {
                        if (num >= items.Length) {
                            Debug.LogWarning("Exhausted number of items");
                            return num;
                        }

                        items[num].Point = center + Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * radius;
                        ++num;
                    }
                }
            }
            return num;
        }
    }
}
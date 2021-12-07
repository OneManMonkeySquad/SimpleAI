using UnityEngine;

namespace SimpleAI.EQS {
    /// <summary>
    /// Generates points in a box or grid shape around a center point.
    /// </summary>
    public class GridGenerator : IGenerator {
        [Range(1, 20)]
        public int Radius = 4;
        [Range(0.1f, 10f)]
        public float Padding = 1;

        public int GenerateItemsNonAlloc(QueryContext around, ResolvedQueryRunContext ctx, Item[] items) {
            int num = 0;

            var centers = ctx.Resolve(around);
            foreach (var center in centers) {
                for (int x = -Radius; x <= Radius; ++x) {
                    for (int y = -Radius; y <= Radius; ++y) {
                        if (num >= items.Length) {
                            Debug.LogWarning("Exhausted number of items");
                            return num;
                        }

                        items[num].Point = center + new Vector3(x * Padding, 0, y * Padding);
                        ++num;
                    }
                }
            }
            return num;
        }
    }
}
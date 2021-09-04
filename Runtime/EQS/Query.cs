using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CareBoo.Serially;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI.EQS {
    public enum QueryContext {
        Querier,
        Target
    }

    public struct Item {
        public Vector3 Point;
        /// [0,1]
        public float Score;
    }

    public enum QueryRunMode {
        /// <summary>
        /// Return the absolute best result.
        /// </summary>
        Best,
        /// <summary>
        /// Return one of the best 25% of all results.
        /// </summary>
        RandomBest25Pct,
        /// <summary>
        /// Return all results (probably for debugging).
        /// </summary>
        All
    }

    public struct QueryRunContext {
        public Vector3[] Querier;
        public Vector3[] Target;

        public Vector3[] Resolve(QueryContext ctx)
            => ctx switch {
                QueryContext.Querier => Querier,
                QueryContext.Target => Target,
                _ => throw new Exception("case missing")
            };
    }

    public delegate void QueryExecuteDone(Item item);

    [CreateAssetMenu(menuName = "AI/Environment Query")]
    public class Query : ScriptableObject {
        public QueryContext Around;
        [SerializeReference]
        [ShowSerializeReference]
        public IGenerator Generator;
        [SerializeReference]
        [ShowSerializeReference]
        public List<ITest> Tests;

        public Task<Item> ExecuteAsync(QueryRunMode mode, QueryRunContext ctx) {
            Assert.IsTrue(mode == QueryRunMode.Best || mode == QueryRunMode.RandomBest25Pct);

            var tcs = new TaskCompletionSource<Item>();
            ExecuteAsync(mode, ctx, item => tcs.TrySetResult(item));
            return tcs.Task;
        }

        public void ExecuteAsync(QueryRunMode mode, QueryRunContext ctx, QueryExecuteDone done) {
            var job = new QuerySystem.Job() {
                Query = this,
                Mode = mode,
                Ctx = ctx,
                Done = done
            };
            QuerySystem.Main.AddQuery(job);
        }

        public void Execute(QueryRunMode mode, QueryRunContext ctx, QueryExecuteDone done) {
            QuerySystem.Main.Execute(this, mode, ctx, done);
        }

        void OnValidate() {
            if (Tests != null) {
                // Make sure less complex tests run first so there's a chance for an early break
                Tests = Tests.OrderBy(t => t != null ? t.RuntimeCost : 1000).ToList();
            }
        }
    }
}
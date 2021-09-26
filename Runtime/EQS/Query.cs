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
        Target,
        Group1,
        Group2
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
        public Vector3 Querier;
        public Vector3 Target;
        public Vector3[] Group1;
        public Vector3[] Group2;

        public IEnumerable<Vector3> Resolve(QueryContext ctx) {
            if (ctx == QueryContext.Querier) {
                yield return Querier;
            } else if (ctx == QueryContext.Target) {
                yield return Target;
            } else if (ctx == QueryContext.Group1) {
                if (Group1 == null)
                    yield break;

                foreach (var entry in Group1)
                    yield return entry;
            } else if (ctx == QueryContext.Group2) {
                if (Group2 == null)
                    yield break;

                foreach (var entry in Group2)
                    yield return entry;
            } else
                throw new Exception("case missing");
        }
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
            QuerySystem.Instance.AddQuery(job);
        }

        public void Execute(QueryRunMode mode, QueryRunContext ctx, QueryExecuteDone done) {
            QuerySystem.Instance.Execute(this, mode, ctx, done);
        }

        void OnValidate() {
            if (Tests != null) {
                // Make sure less complex tests run first so there's a chance for an early break
                Tests = Tests.OrderBy(t => t != null ? t.RuntimeCost : 1000).ToList();
            }
        }
    }
}
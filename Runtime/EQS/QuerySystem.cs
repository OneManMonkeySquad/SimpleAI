using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI.EQS {
    public interface IEQSLogger {
        void LogQuery(Query query, QueryRunMode mode, ResolvedQueryRunContext ctx, Span<Item> items, int? bestIdx);
    }

    public class QuerySystem : MonoBehaviour {
        public struct Job {
            public Query Query;
            public QueryRunMode Mode;
            public QueryRunContext Ctx;
            public QueryExecuteDone Done;
        }

        static QuerySystem s_instance;
        public static QuerySystem Instance {
            get {
                if (s_instance == null) {
                    var goOuter = new GameObject("QuerySystem") {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    s_instance = goOuter.AddComponent<QuerySystem>();
                }
                return s_instance;
            }
            internal set {
                s_instance = value;
            }
        }

        public static IEQSLogger ActiveLogger;

        Queue<Job> _jobs = new Queue<Job>();

        public void QueueJob(Job job) {
            _jobs.Enqueue(job);
        }

        Item[] _items = new Item[64];
        List<Item> _tempListItems = new List<Item>();
        public void Execute(Query query, QueryRunMode mode, QueryRunContext ctx, QueryExecuteDone done) {
            if (query.Generator == null)
                return;

            var resolvedCtx = ctx.Resolve();

            var num = query.Generator.GenerateItemsNonAlloc(query.Around, resolvedCtx, _items);
            for (var i = 0; i < num; ++i) {
                var totalScore = 1f;
                foreach (var test in query.Tests) {
                    if (test == null)
                        continue;

                    var score = test.Run(ref _items[i], resolvedCtx);
                    Assert.IsTrue(score >= 0f && score <= 1f);
                    totalScore *= score;
                    if (totalScore < 0.01f)
                        break;
                }

                _items[i].Score = totalScore;
            }

            var validItems = new Span<Item>(_items, 0, num);

            switch (mode) {
                case QueryRunMode.Best: {
                        var bestScore = 0f;
                        var bestIdx = 0;
                        for (int i = 0; i < num; ++i) {
                            var item = _items[i];
                            if (item.Score > bestScore) {
                                bestScore = item.Score;
                                bestIdx = i;
                            }
                        }
                        var best = _items[bestIdx];
                        ActiveLogger?.LogQuery(query, mode, resolvedCtx, validItems, bestIdx);
                        done(best);
                        break;
                    }
                case QueryRunMode.RandomBest25Pct: {
                        var bestScore = 0f;
                        for (int i = 0; i < num; ++i) {
                            var item = _items[i];
                            if (item.Score > bestScore) {
                                bestScore = item.Score;
                            }
                        }

                        var threshold = Mathf.Max(bestScore * 0.75f, 0);

                        _tempListItems.Clear();
                        for (int i = 0; i < num; ++i) {
                            var item = _items[i];
                            if (item.Score >= threshold) {
                                _tempListItems.Add(item);
                            }
                        }

                        var bestIdx = UnityEngine.Random.Range(0, _tempListItems.Count);
                        var best = _tempListItems[bestIdx];
                        ActiveLogger?.LogQuery(query, mode, resolvedCtx, validItems, bestIdx);
                        done(best);
                        break;
                    }
                case QueryRunMode.All: {
                        ActiveLogger?.LogQuery(query, mode, resolvedCtx, validItems, null);
                        for (int i = 0; i < num; ++i) {
                            var item = _items[i];
                            done(item);
                        }
                        break;
                    }
                default:
                    throw new Exception("case missing");
            }
        }

        void Awake() {
            s_instance = this;
        }

        void Update() {
            if (_jobs.Count == 0)
                return;

            // One per frame
            var job = _jobs.Dequeue();
            Execute(job.Query, job.Mode, job.Ctx, job.Done);
        }
    }
}
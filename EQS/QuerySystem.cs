using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI.EQS {
    public class QuerySystem : MonoBehaviour {
        public struct Job {
            public Query Query;
            public QueryRunMode Mode;
            public QueryRunContext Ctx;
            public QueryExecuteDone Done;
        }

        static QuerySystem main;
        public static QuerySystem Main {
            get {
                if (main == null) {
                    var go = new GameObject("QuerySystem");
                    main = go.AddComponent<QuerySystem>();
                }
                return main;
            }
            internal set {
                main = value;
            }
        }

        Queue<Job> jobs = new Queue<Job>();
        Item[] items = new Item[64];

        public void AddQuery(Job job) {
            jobs.Enqueue(job);
        }

        public void Execute(Query query, QueryRunMode mode, QueryRunContext ctx, QueryExecuteDone done) {
            var num = query.Generator.GenerateItemsNonAlloc(query.Around, ctx, items);
            for (var i = 0; i < num; ++i) {
                var totalScore = 1f;
                foreach (var test in query.Tests) {
                    if (test == null)
                        continue;

                    var score = test.Run(ref items[i], ctx);
                    Assert.IsTrue(score >= 0f && score <= 1f);
                    totalScore *= score;
                    if (totalScore < 0.01f)
                        break;
                }

                items[i].Score = totalScore;
            }

            switch (mode) {
                case QueryRunMode.Best:
                    var bestScore = 0f;
                    var bestIdx = 0;
                    for (int i = 0; i < num; ++i) {
                        var item = items[i];
                        if (item.Score > bestScore) {
                            bestScore = item.Score;
                            bestIdx = i;
                        }
                    }
                    done(items[bestIdx]);
                    break;

                case QueryRunMode.All:
                    for (int i = 0; i < num; ++i) {
                        var item = items[i];
                        done(item);
                    }
                    break;

                default:
                    throw new Exception("case missing");
            }
        }

        void Update() {
            if (jobs.Count == 0)
                return;

            // One per frame
            var job = jobs.Dequeue();
            Execute(job.Query, job.Mode, job.Ctx, job.Done);
        }
    }
}
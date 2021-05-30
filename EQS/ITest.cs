using UnityEngine;

namespace SimpleAI.EQS {
    public interface ITest {
        /// <summary>
        /// Approximation on how computationally intensive the test is.
        /// Used to sort tests at design time. 
        /// </summary>
        float RuntimeCost { get; }

        /// <summary>
        /// Run the test and return a score between [0,1] with 1 being the best score.
        /// </summary>
        float Run(ref Item item, QueryRunContext ctx);
    }
}
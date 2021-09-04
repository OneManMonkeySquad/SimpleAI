namespace SimpleAI.EQS {
    public interface IGenerator {
        int GenerateItemsNonAlloc(QueryContext around, QueryRunContext ctx, Item[] items);
    }
}
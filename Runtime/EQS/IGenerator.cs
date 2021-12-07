namespace SimpleAI.EQS {
    public interface IGenerator {
        int GenerateItemsNonAlloc(QueryContext around, ResolvedQueryRunContext ctx, Item[] items);
    }
}
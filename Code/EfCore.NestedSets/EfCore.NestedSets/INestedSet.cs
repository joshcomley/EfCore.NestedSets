namespace EfCore.NestedSets
{
    public interface INestedSet<TKey, TNullableKey>
    {
        TKey Id { get; set; }
        TNullableKey ParentId { get; set; }
        int Level { get; set; }
        int Left { get; set; }
        int Right { get; set; }
        bool Moving { get; set; }
        TKey RootId { get; set; }
    }
}
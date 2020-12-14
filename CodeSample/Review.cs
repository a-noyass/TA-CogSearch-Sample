using Azure.Search.Documents.Indexes;

namespace CodeSample
{
    public class Review
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; }
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string ProductId { get; set; }
        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string Sentiment { get; set; }
        [SearchableField]
        public Aspect[] Aspects { get; set; }
    }
}

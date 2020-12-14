using Azure.Search.Documents.Indexes;

namespace CodeSample
{
    public class Opinion
    {
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string Text { get; set; }
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string Sentiment { get; set; }
        [SimpleField(IsFilterable = true)]
        public bool IsNegated { get; set; }
    }
}
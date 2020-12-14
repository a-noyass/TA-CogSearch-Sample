using Azure.Search.Documents.Indexes;

namespace CodeSample
{
    public class Aspect
    {
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string Name { get; set; }
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        public string Sentiment { get; set; }
        [SearchableField]
        public Opinion[] Opinions { get; set; }
    }
}

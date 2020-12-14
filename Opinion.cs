using Azure.Search.Documents.Indexes;

namespace ConsoleApp1
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
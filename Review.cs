using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
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

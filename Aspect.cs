using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
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

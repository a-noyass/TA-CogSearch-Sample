using Azure;
using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeSample
{
    /*
     * Environment Variables Used:
     *     MSREAD_ENDPOINT
     *     MSREAD_KEY
     *     COGNITIVE_SEARCH_KEY
     *     COGNITIVE_SEARCH_ENDPOINT
     */
    class Program
    {
        static async Task Main(string[] args)
        {
            var reviewText = "The quality of the pictures are good, but the body is not durable";
            var productId = "1"; // ID of the product reviewed
            var reviewId = "1"; // ID of the review used as the search document ID
            var indexName = "sample-index";

            // Cognitive Search credentials
            var searchKey = Environment.GetEnvironmentVariable("COGNITIVE_SEARCH_KEY");
            var searchEndpoint = Environment.GetEnvironmentVariable("COGNITIVE_SEARCH_ENDPOINT");
            Uri searchUri = new Uri(searchEndpoint);
            AzureKeyCredential searchCredential = new AzureKeyCredential(searchKey);

            // Initialize Search Index client
            var searchIndexClient = new SearchIndexClient(searchUri, searchCredential);
            // Create Index
            await CreateIndex(indexName, searchIndexClient);

            // Initialize Search client
            var searchClient = new SearchClient(searchUri, indexName, searchCredential);

            // TA credentials
            var textAnalyticsKey = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_KEY");
            var textAnalyticsEndpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
            var textAnalyticsCredentials = new AzureKeyCredential(textAnalyticsKey);
            var textAnalyticsUri = new Uri(textAnalyticsEndpoint);

            // Initialize TA client
            var textAnalyticsClient = new TextAnalyticsClient(textAnalyticsUri, textAnalyticsCredentials);

            // Enable opinion mining
            var options = new AnalyzeSentimentOptions() { IncludeOpinionMining = true };

            // Call TA analyze sentiment api
            var sentimentResponse = await textAnalyticsClient.AnalyzeSentimentAsync(reviewText, language: "en", options: options);

            // Map to review search document
            Review review = CreateReviewDocument(productId, reviewId, sentimentResponse);

            // Upload document
            var batch = IndexDocumentsBatch.Create(IndexDocumentsAction.Upload(review));
            try
            {
                IndexDocumentsResult result = await searchClient.IndexDocumentsAsync(batch);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static Review CreateReviewDocument(string productId, string reviewId, Response<DocumentSentiment> sentimentResponse)
        {
            var aspects = new List<Aspect>();
            foreach (var sentence in sentimentResponse.Value.Sentences)
            {
                if (sentence.MinedOpinions.Count > 0)
                {
                    aspects.AddRange(sentence.MinedOpinions.Select(minedOpinion => new Aspect
                    {
                        Name = minedOpinion.Aspect.Text,
                        Sentiment = minedOpinion.Aspect.Sentiment.ToString(),
                        Opinions = minedOpinion.Opinions.Select(opinion => new Opinion
                        {
                            Sentiment = opinion.Sentiment.ToString(),
                            Text = opinion.Text,
                            IsNegated = opinion.IsNegated
                        }).ToArray()
                    }));
                }
            }
            var review = new Review
            {
                Sentiment = sentimentResponse.Value.Sentiment.ToString(),
                ProductId = productId,
                Id = reviewId,
                Aspects = aspects.ToArray()
            };
            return review;
        }

        private static async Task CreateIndex(string indexName, SearchIndexClient adminClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Review));

            var definition = new SearchIndex(indexName, searchFields);

            await adminClient.CreateOrUpdateIndexAsync(definition);
        }
    }
}

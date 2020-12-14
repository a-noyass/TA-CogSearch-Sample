using Azure;
using Azure.AI.TextAnalytics;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /*
     * Environment Variables Used:
     *     MSREAD_ENDPOINT
     *     MSREAD_KEY
     *     TEXT_ANALYTICS_ENDPOINT
     *     TEXT_ANALYTICS_KEY
     */
    class Program
    {
        static async Task Main(string[] args)
        {
            var reviewText = "The quality of the pictures are good, but the body is not durable";
            var productId = "321";
            var reviewId = "321";
            var indexName = "sample-index";

            // Intiialize search clients
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

            // Initialize TA client
            var textAnalyticsKey = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_KEY");
            var textAnalyticsEndpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
            var textAnalyticsCredentials = new AzureKeyCredential(textAnalyticsKey);
            var textAnalyticsUri = new Uri(textAnalyticsEndpoint);
            var textAnalyticsClient = new TextAnalyticsClient(textAnalyticsUri, textAnalyticsCredentials);

            // Enable opinion mining
            var options = new AnalyzeSentimentOptions() { IncludeOpinionMining = true };

            // Call TA analyze sentiment api
            var sentimentResponse = await textAnalyticsClient.AnalyzeSentimentAsync(reviewText, language: "en", options: options);

            // map opinions
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

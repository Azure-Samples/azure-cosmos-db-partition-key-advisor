using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PartitionKeyAdvisor.Models;
using Microsoft.Azure.Documents;
using System.Net;
using System.Web;
using Microsoft.Azure.Documents.Client;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace PartitionKeyAdvisor.Controllers
{
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.SqlServer.Server;
    using Models;
    using Nancy.Json;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class HomeController : Controller {
       
        public ActionResult Index() {
            return View();
        }

        //normalizes any input dataset 
        public static double[] NormalizedValues(double[] dataset) {
            double[] normalizedValues = new double[dataset.Length];
            for (int i = 0; i < dataset.Length; i++){
                var element = ((dataset[i] - dataset.Min()) / (dataset.Max() - dataset.Min()))*100;
                normalizedValues[i] = Math.Round(element,2);
            }
            return normalizedValues;
        }

        //calculates inverse normalization of values 
        public static double[] InverseNormalizedValues(double[] dataset) {
            double[] normalizedValues = new double[dataset.Length];
            for (int i = 0; i < dataset.Length; i++){
                var element = ((dataset.Max() - dataset[i]) / (dataset.Max() - dataset.Min()))*100;
                normalizedValues[i] = Math.Round(element,2);
            }
            return normalizedValues;
        }

        //formula used to recommend a partition key based on storage distribution and # of distinct values
        public static double[] RecommendationFormula(double[] datasetNormalizederr, double[] datasetInverseNormalized, double[]overallUniqueness ) {
            
            double[] results = new double[datasetNormalizederr.Length];
            for (int i = 0; i < datasetNormalizederr.Length; i++) {
                var element = (0.25 * datasetNormalizederr[i]) + (0.25 * datasetInverseNormalized[i]) + (0.5*overallUniqueness[i]);
                results[i] = Math.Round(element,2);
            }
            return results;
        }

        //formula used to calculate the uniformity of storage distribution
        public double RMSE(int[] actual) {
            double differenceSquared = 0;
            double expected = actual.Average();
            for (int i = 0; i < actual.Length; i++){
                differenceSquared += Math.Pow((actual[i] - expected), 2);
            }
            double error = Math.Sqrt(differenceSquared / actual.Length);
            return Math.Round(error,2);
        }

        //groups collection and creates a count by filter/candidate partition key
        public Dictionary<string, int> GroupedDictionary(Dictionary<Document, Document> documentDictionary, string filter) {
            return documentDictionary.Values.GroupBy(x => x.GetPropertyValue<string>(filter)).ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<string, int> DistinctnessOverTime(Dictionary<Document, Document> documentDictionary, string filter, string time = "_ts"){
            return documentDictionary.Values.GroupBy(x => x.GetPropertyValue<string>(time), y=> y.GetPropertyValue<string>(filter)).ToDictionary(g => g.Key, g => g.Distinct().Count());
        }

        public async Task<ActionResult> WriteHeavy(FormModel model) {
            string _endpoint = model.ConnectionString;
            string _primaryKey = model.ReadOnlyKey;

            try
            {
                using (DocumentClient client = new DocumentClient(new Uri(_endpoint), _primaryKey))
                {
                    Database targetDatabase = new Database { Id = model.Database };
                    targetDatabase = await client.CreateDatabaseIfNotExistsAsync(targetDatabase);
                    await client.OpenAsync();
                    var collLink = UriFactory.CreateDocumentCollectionUri(model.Database, model.Collection);
                    FeedOptions options = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

                    //reads all documents within a collection
                    var doc = (await client.ReadDocumentCollectionAsync(collLink)).Resource;
                    var documents = client.CreateDocumentQuery(doc.SelfLink, options).AsDocumentQuery();
                    var documentList = new List<Document>();

                    while (documents.HasMoreResults)
                    {
                        foreach (Document i in await documents.ExecuteNextAsync())
                        {
                            documentList.Add(i);
                        }
                    }
                    Dictionary<string, double> viewDataDictionaryforDcount = new Dictionary<string, double>();
                    var documentDictionary = documentList.ToDictionary(x => x);

                    var listOfDocumentDictionaries = new List<Dictionary<string, int>>();
                    var listOfDistinctValues = new List<Dictionary<string, int>>();
                    var list = documentList[0].ToString();
                    var valueTypesThatGroupPoorly = new[] {"Object", "Array"};
                    var dictionaryOfSchema = JObject.Parse(list)
                                                    .Properties()
                                                    .Where(p => !valueTypesThatGroupPoorly.Contains(p.Value.Type.ToString()))
                                                    .Select(p => p.Name)
                                                    .ToList();

                    var filterArray = new string[] { model.Filter1, model.Filter2, model.Filter3 };

                    //groups values by distinctness per property for schema discovery
                    var overallUniquenessResults = new int[3];
                    foreach (var i in dictionaryOfSchema)
                    {
                        var groupValues = documentDictionary.Values.GroupBy(x => x.GetPropertyValue<string>(i)).Distinct().Count();
                        viewDataDictionaryforDcount.Add(i, groupValues);
                        if (i.Equals(filterArray[0]))
                        {
                            overallUniquenessResults[0] = groupValues;
                        }
                        else if (i.Equals(filterArray[1]))
                        {
                            overallUniquenessResults[1] = groupValues;
                        }
                        else if (i.Equals(filterArray[2]))
                        {
                            overallUniquenessResults[2] = groupValues;
                        }
                    }

                    //calculates Distinctness/sec and cardinality per filter/candidate partition key inputed by user

                    for (var filter = 0; filter < filterArray.Length; filter++)
                    {
                        var res = GroupedDictionary(documentDictionary, filterArray[filter]);
                        var distinctValuesOverTime = DistinctnessOverTime(documentDictionary, filterArray[filter]);
                        listOfDocumentDictionaries.Add(res);
                        listOfDistinctValues.Add(distinctValuesOverTime);
                    }

                    var count = 0;
                    var errorListStorageDistribution = new double[listOfDocumentDictionaries.Count];
                    var errorListDistinctValues = new double[listOfDistinctValues.Count];


                    //calculates the uniformity of the storage distribution 
                    foreach (var i in listOfDocumentDictionaries)
                    {
                        var errorResult1 = RMSE(i.Values.ToArray());
                        errorListStorageDistribution[count] = errorResult1;
                        count++;

                    }


                    var count1 = 0;
                    //calculates the uniformity of the distinctness over time distribution based on the RMSE formula
                    foreach (var i in listOfDistinctValues.ToArray())
                    {
                        var errorResult = RMSE(i.Values.ToArray());
                        errorListDistinctValues[count1] = errorResult;
                        count1++;
                    }

                    var overallUniquenessRes = NormalizedValues((overallUniquenessResults.ToArray()).Select(Convert.ToDouble).ToArray());
                    var storageResults = InverseNormalizedValues(errorListStorageDistribution);
                    var distinctValuesResults = NormalizedValues(errorListDistinctValues);
                    ViewData["RecommendationResults"] = RecommendationFormula(storageResults, overallUniquenessRes, distinctValuesResults);
                    ViewData["StorageResults"] = storageResults;
                    ViewData["OverallUniquenessResult"] = overallUniquenessRes;

                    ViewData["RecommendationResultsDistinctValues"] = distinctValuesResults;

                    ViewData["listOfDocumentDictionaries"] = listOfDocumentDictionaries;
                    ViewData["listOfDistinctValues"] = listOfDistinctValues;

                    ViewData["DataDictionary"] = viewDataDictionaryforDcount;
                    ViewData["filterArray"] = filterArray;

                }
            }catch (Exception ex)
            {
                return View("Error");
            }
            return View();

        }


        public IActionResult Privacy(){
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(){
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}

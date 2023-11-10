using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Json;
using RestSharp;
using Skymey_main_lib.Models.ExchangesData.Polygon;
using Skymey_main_lib.Models.Prices.Polygon;
using Skymey_stock_polygon_exchanges.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skymey_stock_polygon_exchanges.Actions.GetExchanges
{
    public class GetExchanges
    {
        private RestClient _client;
        private RestRequest _request;
        private MongoClient _mongoClient;
        private ApplicationContext _db;
        private string _apiKey;
        public GetExchanges()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            _apiKey = config.GetSection("ApiKeys:Polygon").Value;
            _client = new RestClient("https://api.polygon.io/v3/reference/exchanges?asset_class=stocks&apiKey=" + _apiKey);
            _request = new RestRequest("https://api.polygon.io/v3/reference/exchanges?asset_class=stocks&apiKey=" + _apiKey, Method.Get);
            _mongoClient = new MongoClient("mongodb://127.0.0.1:27017");
            _db = ApplicationContext.Create(_mongoClient.GetDatabase("skymey"));
        }
        public void GetExchangesFromPolygon()
        {
            try
            {
                #region DIA
                _request.AddHeader("Content-Type", "application/json");
                var r = _client.Execute(_request).Content;
                ExchangesPolygonQuery tp = new JavaScriptSerializer().Deserialize<ExchangesPolygonQuery>(r);
                #endregion

                foreach (var ticker in tp.results)
                {
                    Console.WriteLine(ticker.name);
                    var ticker_find = (from i in _db.ExchangesList where i.name == ticker.name select i).FirstOrDefault();

                    if (ticker_find == null)
                    {
                        ticker._id = ObjectId.GenerateNewId();
                        ticker.Update = DateTime.UtcNow;
                        _db.ExchangesList.Add(ticker);
                    }
                }
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
            }
        }
        public void Dispose()
        {
        }
        ~GetExchanges()
        {

        }
    }
}

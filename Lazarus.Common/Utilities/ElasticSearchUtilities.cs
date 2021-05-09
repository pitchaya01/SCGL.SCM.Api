using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
 


namespace Lazarus.Common.Utilities
{

    public class ElasticSearchUtilities
    {
        /// <summary>
        /// Insert Index Object ลง Es เป็นการทำ Indexใหม่ทุกครั้ง 
        /// </summary>
        /// <param name="obj"> Object ที่ส่งมาต้อง มี Field Id เป็น Primary Key </param>
        public static  void Insert(string json, string indexName)
        {

            try
            {
                var url = AppConfigUtilities.GetAppConfig<string>("ElasticsearchUrl");
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 0, 0, 5000);
                client.BaseAddress = new Uri(url);
       
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //var json = "{\"id\": \"1\",\"message\": \"hello\"}";
                Console.WriteLine($"LOG:Request  to ES");
                HttpRequestMessage request = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"{indexName.ToLower()}/ _doc/");
                request.Content = new StringContent(json,
                                                    Encoding.UTF8,
                                                    "application/json");
                Console.WriteLine($"LOG:Requested  to ES");
                client.SendAsync(request)
                      .ContinueWith(responseTask =>
                      {
                          Console.WriteLine("Response: {0}", responseTask.Result);
                      }).Wait();
                Console.WriteLine($"LOG:{indexName.ToLower()}  to ES");
            }
            catch (Exception e)
            {
                Console.WriteLine($"LOG:Requested  to ES Error-{e.GetMessageError()}");
            }



        }

    }
}

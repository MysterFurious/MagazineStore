using MagazineStore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MagazineStore.Helpers
{
    class ApiHelper
    {
        private string BaseUrl { get; set; }
        private HttpClient HttpClient { get; set; }
        private string Token { get; set; }
        private List<string> Categories { get; set; }
        private List<Magazine> Magazines { get; set; }
        private List<Subscriber> Subscribers { get; set; }
        private Answer Answer { get; set; }

        public ApiHelper()
        {
            this.BaseUrl = "http://magazinestore.azurewebsites.net";
        }

        public string RunChallenge()
        {
            using (this.HttpClient = new HttpClient())
            {
                this.GetToken();

                Parallel.Invoke(
                    () => this.GetCategoriesAndMagazines(),
                    () => this.GetSubscribers()
                );
                                
                this.GenerateAnswer();
                return this.GetResults();
            }
        }

        private void GetToken()
        {
            string endpoint = "api/token";
            ResponseBase response = JsonConvert.DeserializeObject<ResponseBase>(this.ExecuteGet(endpoint));
            this.ValidateResponse(response, endpoint);
            this.Token = response.token;
        }

        private void GetCategoriesAndMagazines()
        {
            this.GetCategories();
            this.GetMagazines();
        }

        private void GetCategories()
        {
            string endpoint = String.Format("api/categories/{0}", this.Token);
            CategoryResponse response = JsonConvert.DeserializeObject<CategoryResponse>(this.ExecuteGet(endpoint));
            this.ValidateResponse(response, endpoint);
            this.Categories = response.data;
        }

        private void GetMagazines()
        {
            this.Magazines = new List<Magazine>();
            Parallel.ForEach(this.Categories, new ParallelOptions { MaxDegreeOfParallelism = 10 }, this.AppendMagazinesByCategory);
        }

        private void AppendMagazinesByCategory(string category)
        {
            string endpoint = String.Format("api/magazines/{0}/{1}", this.Token, category);
            MagazineResponse response = JsonConvert.DeserializeObject<MagazineResponse>(this.ExecuteGet(endpoint));
            this.ValidateResponse(response, endpoint);
            this.Magazines.AddRange(response.data);
        }

        private void GetSubscribers()
        {
            string endpoint = String.Format("api/subscribers/{0}", this.Token);
            SubscriberResponse response = JsonConvert.DeserializeObject<SubscriberResponse>(this.ExecuteGet(endpoint));
            this.ValidateResponse(response, endpoint);
            this.Subscribers = response.data;
        }

        private void GenerateAnswer()
        {
            /*
            Making a few assumptions here:
                The list of categories returned by the API is unique
                The list of magazines for each subscriber is unique
                All subscriber magazines will exist in the list of magazines returned
            */

            this.Answer = new Answer();

            foreach (Subscriber subscriber in this.Subscribers)
            {
                List<string> suscriberCategories = new List<string>();

                foreach (int magazineId in subscriber.magazineIds)
                {
                    Magazine magazine = this.Magazines.First(i => i.id == magazineId);

                    if (!suscriberCategories.Contains(magazine.category))
                    {
                        suscriberCategories.Add(magazine.category);

                        if (suscriberCategories.Count == this.Categories.Count)
                        {
                            this.Answer.subscribers.Add(subscriber.id);
                            break;
                        }
                    }
                }
            }
        }

        private string ExecuteGet(string endpoint)
        {
            using (HttpResponseMessage responseMessage = this.HttpClient.GetAsync(String.Format("{0}/{1}", this.BaseUrl, endpoint)).Result)
            {
                if (!responseMessage.IsSuccessStatusCode) throw new HttpRequestException(responseMessage.ReasonPhrase);
                return responseMessage.Content.ReadAsStringAsync().Result;
            }
        }

        private string GetResults()
        {
            string endpoint = String.Format("api/answer/{0}", this.Token);
            AnswerResponseResponse response = JsonConvert.DeserializeObject<AnswerResponseResponse>(this.ExecutePost(endpoint, JsonConvert.SerializeObject(this.Answer)));
            this.ValidateResponse(response, endpoint);
            return JsonConvert.SerializeObject(response.data);
        }

        private string ExecutePost(string endpoint, string content)
        {
            using (
                HttpResponseMessage responseMessage = this.HttpClient.PostAsync(
                    String.Format("{0}/{1}", this.BaseUrl, endpoint),
                    new StringContent(
                        content, 
                        Encoding.UTF8, 
                        "application/json"
                    )
                ).Result
            )
            {
                if (!responseMessage.IsSuccessStatusCode) throw new HttpRequestException(responseMessage.ReasonPhrase);
                return responseMessage.Content.ReadAsStringAsync().Result;
            }
        }

        private void ValidateResponse(ResponseBase response, string endpoint)
        {
            if (!response.success) throw new Exception(String.Format("Error when calling the '{0}' endpoint: {1}", endpoint, response.message));
        }
    }
}

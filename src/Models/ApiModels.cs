using System.Collections.Generic;

namespace MagazineStore.Models
{
    class ResponseBase
    {
        public bool success { get; set; }
        public string token { get; set; }
        public string message { get; set; }
    }

    class CategoryResponse : ResponseBase
    {
        public List<string> data { get; set; }
    }

    class SubscriberResponse : ResponseBase
    {
        public List<Subscriber> data { get; set; }
    }

    class Subscriber
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<int> magazineIds { get; set; }
    }

    class MagazineResponse : ResponseBase
    {
        public List<Magazine> data { get; set; }
    }

    class Magazine
    {
        public int id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
    }

    class Answer
    {
        public Answer()
        {
            this.subscribers = new List<string>();
        }

        public List<string> subscribers { get; set; }
    }

    class AnswerResponseResponse : ResponseBase
    {
        public AnswerResponse data { get; set; }
    }

    class AnswerResponse
    {
        public string totalTime { get; set; }
        public bool answerCorrect { get; set; }
        public List<string> shouldBe { get; set; }
    }
}

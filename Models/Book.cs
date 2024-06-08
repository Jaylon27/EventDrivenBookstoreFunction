using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EventDrivenBookstoreFunction.Models
{

public class Book
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        
    }
}
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace EventDrivenBookstoreFunction.Models
{
    public class Subscriber
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("preferredGenres")]
        public List<string> PreferredGenres { get; set; }

        public Subscriber()
        {
            PreferredGenres = new List<string>();
            
        }
    }
}
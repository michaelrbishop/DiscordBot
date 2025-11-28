using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BishHouse2.Services
{
    public class DadJokeService : IDadJokeService
    {
        private readonly HttpClient _httpClient;

        public DadJokeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetRandomDadJoke()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://icanhazdadjoke.com/");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.UserAgent.ParseAdd("BishHouse2App/1.0 (Testing Discord bot)");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var dadJoke = JsonSerializer.Deserialize<DadJokeResponse>(json);

            return dadJoke?.Joke ?? "I don't have any jokes. Fuck you.";
        }


        internal class DadJokeResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("joke")]
            public string Joke { get; set; }
            [JsonPropertyName("status")]
            public int Status { get; set; }
        }
    }
}

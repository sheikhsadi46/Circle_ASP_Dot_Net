using Circle_MVC.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Circle_MVC.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Register(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7158/api/auth/register", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> Login(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7158/api/auth/login", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> EditUser(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("https://localhost:7158/api/auth/edit", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<User> GetUser(int id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7158/api/auth/user/{id}");
            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User>(userData);
            }
            return null;
        }
    }
}


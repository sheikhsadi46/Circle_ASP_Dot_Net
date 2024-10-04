using Circle_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Circle_MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7158/api/"); 
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction("Index", "Home"); 
            }
            return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/login", content);
            string responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(responseBody);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetInt32("UserId", apiResponse.Id); 

                return RedirectToAction("Index", "Home"); 
            }

            ViewBag.Message = "Invalid username or password";
            return View();
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            
            if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
            {
                ViewBag.Message = "Username and password cannot be empty.";
                return View();
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/register", content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Message = "Registration failed";
            return View();
        }

        // POST: Auth/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        
        // GET: Auth/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            
            var userId = HttpContext.Session.GetInt32("UserId");
            
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            
            System.Diagnostics.Debug.WriteLine($"Fetching user with ID: {userId.Value}");
            var response = await _httpClient.GetAsync($"auth/user/{userId.Value}");
            System.Diagnostics.Debug.WriteLine($"GetUser response: {response.StatusCode}, Body: {await response.Content.ReadAsStringAsync()}");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(responseBody);
                return View(user);
            }

           
            ViewBag.Message = "User not found.";
            return View(new User()); 
        }


        // POST: Auth/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(User updatedUser, string currentPassword)
        {
            if (updatedUser == null || string.IsNullOrWhiteSpace(updatedUser.Username))
            {
                ViewBag.Message = "Username cannot be empty.";
                return View(updatedUser);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login");
            }

            var response = await _httpClient.GetAsync($"auth/user/{userId.Value}");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Message = "User not found.";
                return View(new User());
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var existingUser = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(responseBody);

            if (existingUser.Password != currentPassword)
            {
                ViewBag.Message = "Current password is incorrect.";
                return View(updatedUser);
            }

            if (string.IsNullOrEmpty(updatedUser.Password))
            {
                updatedUser.Password = existingUser.Password;
            }

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(updatedUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var updateResponse = await _httpClient.PutAsync("auth/edit", content);
            if (updateResponse.IsSuccessStatusCode)
            {
                ViewBag.Message = "User updated successfully.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Update failed. Please try again.";
            return View(updatedUser);
        }

        // GET: Auth/Products
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var response = await _httpClient.GetAsync("Auth/product/list");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(responseBody);
                return View(products);
            }

            ViewBag.Message = "Unable to fetch products.";
            return View(new List<Product>());
        }

        // POST: Auth/Purchase
        [HttpPost]
        public async Task<IActionResult> Purchase(int productId, int quantity)
        {
            var product = await GetProductById(productId);

            if (product == null || quantity > product.Stock)
            {
                ViewBag.Message = "Invalid purchase request. Check stock or product ID.";
                return RedirectToAction("Products");
            }
            product.Stock -= quantity;
            ViewBag.Message = "Purchase successful!";
            return RedirectToAction("Products");
        }

        private async Task<Product> GetProductById(int productId)
        {
            var response = await _httpClient.GetAsync($"product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(responseBody);
            }

            return null;
        }

    }
}

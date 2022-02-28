using CallApiWithTocken.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace CallApiWithTocken.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[Authorize]
        public async Task<IActionResult> Get()
        {
            var temps = await gettemp();
            return View(temps);
        }
        public async Task<IActionResult> LoginUser(UserInfo user)
        {
            using (var httpclient=new HttpClient())
            {
                StringContent stringContent=new StringContent(JsonConvert.SerializeObject(user),Encoding.UTF8,"application/json");
                using (var response = await httpclient.PostAsync("http://localhost:5000/api/Authenticate/login", stringContent))
                {
                    String token = await response.Content.ReadAsStringAsync();
                    if (token == null)
                    {
                       
                        ViewBag.Error = "no credential";
                        return Redirect("~/Home/Index");
                    }
                    string x = token.Substring(10);
                    HttpContext.Session.SetString("JWTtoken", token.Substring(10, x.Length-2));

                }
                return Redirect("~/Home/Get");
            }
        }
        //[Authorize]
        public async Task<List<WeatherForecast>> gettemp()
        {
            var accesstoken = HttpContext.Session.GetString("JWTtoken");
            var url = "http://localhost:5000/api/WeatherForecast";
            // use as test
            CallApi(url, accesstoken);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
            string jsonstr=await client.GetStringAsync(url);
            var res = JsonConvert.DeserializeObject<List<WeatherForecast>>(jsonstr).ToList();
            return res;

        }
        //test to clear
        static string CallApi(string url, string token)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    //var t = JsonConvert.DeserializeObject<Token>(token);

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                }
                var response = client.GetAsync(url).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace mkdiscordbot
{
    internal class WeatherService
    {
        public class WeatherInfo
        {
            public Coord coord { get; set; }
            public Weather[] weather { get; set; }
            public string _base { get; set; }
            public Main main { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public Rain rain { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }

            public class Coord
            {
                public float lon { get; set; }
                public float lat { get; set; }
            }

            public class Main
            {
                public float temp { get; set; }
                public int pressure { get; set; }
                public int humidity { get; set; }
                public float temp_min { get; set; }
                public float temp_max { get; set; }
            }

            public class Wind
            {
                public float speed { get; set; }
                public int deg { get; set; }
            }

            public class Clouds
            {
                public int all { get; set; }
            }

            public class Rain
            {
                public int _3h { get; set; }
            }

            public class Sys
            {
                public int type { get; set; }
                public int id { get; set; }
                public float message { get; set; }
                public string country { get; set; }
                public int sunrise { get; set; }
                public int sunset { get; set; }
            }

            public class Weather
            {
                public int id { get; set; }
                public string main { get; set; }
                public string description { get; set; }
                public string icon { get; set; }
            }
        }

        public static WeatherInfo GetWeatherInfo(ulong sid,ulong cityid)
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?id={cityid}&appid={P.I[sid].S.OpenWeatherMapAPIKey}";
            string json = (new System.Net.WebClient()).DownloadString(url);
            return JsonConvert.DeserializeObject<WeatherInfo>(json);
        }

        public static string Wstr(ulong sid, ulong cityid)
        {
            WeatherInfo wi = GetWeatherInfo(sid,cityid);

            return wi.weather[0].main;
        }

        public static string WstrUser(ulong sid, ulong id)
        {
            WeatherInfo wi = GetWeatherInfo(sid,P.I[sid].U[id].CityId);

            return wi.weather[0].main;
        }

        public static bool UserHasLocInfo(ulong sid,ulong id)
        {
            if (!P.I[sid].U.ContainsKey(id))
                return false;
            else if (P.I[sid].U[id].CityId == 0)
                return false;
            else return true;
        }

        public static void SetUserLoc(ulong sid,ulong id, ulong loc)
        {
            if (!P.I[sid].U.ContainsKey(id))
                P.I[sid].U.Add(id,new UserDictionary(id));
            P.I[sid].U[id].CityId = loc;
        }

        public static string WeatherInfoSource()
            => "Data from OpenWeatherMap";
    }
}
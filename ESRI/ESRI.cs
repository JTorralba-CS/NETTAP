using Newtonsoft.Json.Linq;

namespace ESRI
{
    public static class GEOCODE
    {
        //ESRI
        //private const string GeocodeUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";

        //AUTH
        private const string GeocodeUrl = "https://maps.eptc911.org/server/rest/services/TACMap/MultiRole/GeocodeServer/findAddressCandidates";

        public static async Task<(double? Latitude, double? Longitude)> GetLATLON(string address, string city)
        {
            using var client = new HttpClient();

            // Build request URL
            //var requestUrl = $"{GeocodeUrl}?f=json&singleLine={Uri.EscapeDataString(address)}&maxLocations=1";

            var requestUrl = $"{GeocodeUrl}?Address={Uri.EscapeDataString(address)}&City={Uri.EscapeDataString(city)}&maxLocations=1&f=json";

            Console.WriteLine($"{requestUrl}\r\n");

            HttpResponseMessage response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JObject data = JObject.Parse(json);

            var candidates = data["candidates"];
            if (candidates != null && candidates.HasValues)
            {
                var location = candidates[0]["location"];
                double lat = location["y"]?.Value<double>() ?? 0;
                double lon = location["x"]?.Value<double>() ?? 0;
                return (lat, lon);
            }

            return (null, null);
        }

    }
}

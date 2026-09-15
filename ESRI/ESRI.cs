using Newtonsoft.Json.Linq;

namespace ESRI
{
    public static class GEOCODE
    {
        private const string GeocodeUrl = "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer/findAddressCandidates";

        public static async Task<(double? Latitude, double? Longitude)> GetLATLON(string address)
        {
            using var client = new HttpClient();

            // Build request URL
            var requestUrl = $"{GeocodeUrl}?f=json&singleLine={Uri.EscapeDataString(address)}&maxLocations=1";

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

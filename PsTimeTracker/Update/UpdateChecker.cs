using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSTimeTracker.Update
{
    public class UpdateChecker
    {
        public VersionInfo NewVersionInfo { get; private set; }

        public async Task<bool> IsUpdateAvailable()
        {
            string json = await GetFileFromWeb();
            NewVersionInfo = DeserializeVersionInfo(json);

            if (new Version(NewVersionInfo.Version) > App.Version)
                return true;
            else
                return false;
        }

        private VersionInfo DeserializeVersionInfo(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<VersionInfo>(json);
            }
            catch (Exception)
            {
                return new VersionInfo() { Description = string.Empty, Version = "0.0.0" };
            }
        }

        private async Task<string> GetFileFromWeb()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://raw.githubusercontent.com/mortuusars/PhotoshopTimeTracker/master/PsTimeTracker/version.json");
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}

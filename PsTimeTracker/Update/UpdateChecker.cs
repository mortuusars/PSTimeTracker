using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PSTimeTracker.Update
{
    public class UpdateChecker
    {
        public async Task<(bool updateAvailable, VersionInfo versionInfo)> CheckAsync()
        {
            string json = await GetFileFromWeb();
            var versionInfo = JsonSerializer.Deserialize<VersionInfo>(json);

            if (new Version(versionInfo.Version) > App.Version)
                return (true, versionInfo);
            else
                return (false, null);
        }

        private async Task<string> GetFileFromWeb()
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
    }
}

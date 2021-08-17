using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gotoesUpload
{
    public class Downloader
    {
        private ActivitySettings activitySettings;
        public event EventHandler FileWasDownload;
        public Downloader(ActivitySettings activitySettings)
        {
            this.activitySettings = activitySettings;
        }

        public async void DownloadMergedFile(string activityNumber)
        {
            var httpHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };
            HttpClient httpClient = new HttpClient(httpHandler);
            var url_link = $"https://gotoes.org/strava/upload.php?f={activityNumber}";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url_link);
                response.EnsureSuccessStatusCode();
                string html_page = response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                UIState.ShowMessage($"Exception happened: {e}");
            }
            

            //todo Parse html Here
            //todo create UI for parsed data


            //todo set form from activity settings
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("lat[2]", "Y"),
                new KeyValuePair<string, string>("ele[2]", "Y"),
                new KeyValuePair<string, string>("hr[1]", "Y"),
                new KeyValuePair<string, string>("cad[1]", "Y"),
                new KeyValuePair<string, string>("outputFormat", "GPX"),
                new KeyValuePair<string, string>("spoofGPS", "0_--No+GPS+SELECTED--"),
                new KeyValuePair<string, string>("ActivitySport", "Running"),
                new KeyValuePair<string, string>("ops", "OTP"),
                new KeyValuePair<string, string>("dtp", "0"),
                new KeyValuePair<string, string>("iHR", ""),
                new KeyValuePair<string, string>("discardHR", ""),
                new KeyValuePair<string, string>("suppliedName", ""),
                new KeyValuePair<string, string>("f", activityNumber),
                new KeyValuePair<string, string>("isPatreon", ""),
                new KeyValuePair<string, string>("sortedFileNames", $"{activitySettings.FileNames[1].Split("\\")[^1]}_1,{activitySettings.FileNames[0].Split("\\")[^1]}_2"),
                new KeyValuePair<string, string>("timeZoneAdjustmentFactor", "21600"),
            });

            try
            {
                var upload_resp = await httpClient.PostAsync("https://gotoes.org/strava/upload.php", formContent);
                var statusCode = upload_resp.StatusCode;
                if (statusCode != HttpStatusCode.Found)
                {
                    File.AppendAllText("log.txt", $"status code of upload post request: {statusCode}");
                }

                var download_url =
                    $"https://gotoes.org/strava/downloader.php?sts=&d=downloads&s=Running&f={activityNumber}.gpx&aba=&suppliedName=";
                HttpResponseMessage response1 = await httpClient.GetAsync(download_url);
                response1.EnsureSuccessStatusCode();

                var downloadedFileName = response1.Content.Headers.ContentDisposition?.FileName ?? string.Empty;

                if (downloadedFileName == "")
                {
                    downloadedFileName = "error.html";
                }

                downloadedFileName = downloadedFileName.Replace("\"", "");

                Stream streamToReadFrom = await response1.Content.ReadAsStreamAsync();

                httpClient.Dispose();
                var directoryToSave = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\";
                Stream file = File.Create(directoryToSave + downloadedFileName);
                streamToReadFrom.CopyTo(file);
                file.Close();

                FileWasDownload?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                UIState.ShowMessage($"Exception happened: {e}");
            }

        }
    }
}

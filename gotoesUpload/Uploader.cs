using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace gotoesUpload
{
    public class Uploader
    {
        private UIState uiState;
        public Uploader(UIState _uiState)
        {
            uiState = _uiState;
        }

        public event EventHandler FilesWereUploaded;
        public async Task LoadFiles(string[] files)
        {
            var filesContent = new List<byte[]>();
            foreach (var filename in files)
            {
                filesContent.Add(await File.ReadAllBytesAsync(filename));
            }
            try
            {
                await UploadFiles(filesContent, files);
            }
            catch (Exception e)
            {
                UIState.ShowMessage($"Exception happened: {e}");
            }
        }

        private async Task<string> UploadFiles(List<byte[]> filesContent, string[] names)
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            foreach (var file_bytes in Enumerable.Zip(filesContent, names))
            {
                form.Add(new ByteArrayContent(file_bytes.First, 0, file_bytes.First.Length), "files[]",
                    file_bytes.Second);
            }

            HttpResponseMessage response = await httpClient.PostAsync("https://gotoes.org/strava/upload.php", form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string html = response.Content.ReadAsStringAsync().Result;

            var splits = html.Split("=");
            var activityNumber = splits[3].Split("&")[0];

            FilesWereUploaded?.Invoke(this, new FilesWereUploadedEventArgs()
            {
                ActivityNumber = activityNumber
            });

            return activityNumber;
        }

        public class FilesWereUploadedEventArgs : EventArgs
        {
            public string ActivityNumber { get; set; }
        }
    }
}
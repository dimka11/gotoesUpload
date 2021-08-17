using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;


namespace gotoesUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                DropAreaLabel.Content = "";
                foreach (var file in files)
                {
                    DropAreaLabel.Content = DropAreaLabel.Content + file.Split("\\")[file.Split("\\").Length - 1] + "\n";
                }
                
                LoadFiles(files);
            }
        }

        private void LoadFiles(string[] files)
        {
            var filesContent = new List<byte[]>();
            foreach (var filename in files)
            {
                filesContent.Add(File.ReadAllBytes(filename));
            }
            UploadFiles(filesContent, files);
        }

        private async void UploadFiles(List<byte[]> filesContent, string[] names)
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            foreach (var file_bytes in Enumerable.Zip(filesContent, names))
            {
                form.Add(new ByteArrayContent(file_bytes.First, 0, file_bytes.First.Length), "files[]", file_bytes.Second);
            }

            HttpResponseMessage response = await httpClient.PostAsync("https://gotoes.org/strava/upload.php", form);

            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            string html = response.Content.ReadAsStringAsync().Result;

            var splits = html.Split("=");
            var activity_number = splits[3].Split("&")[0];
            var url_link = $"https://gotoes.org/strava/upload.php?f={activity_number}";
            DownloadMergedFile(url_link, activity_number, names);
        }

        private async void DownloadMergedFile(string url_link, string activity_number, string[] names)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url_link);

            response.EnsureSuccessStatusCode();
            string html = response.Content.ReadAsStringAsync().Result;


            // Request to upload url
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
                new KeyValuePair<string, string>("f", activity_number),
                new KeyValuePair<string, string>("isPatreon", ""),
                new KeyValuePair<string, string>("sortedFileNames", $"{names[1].Split("\\")[^1]}_1,{names[0].Split("\\")[^1]}_2"),
                new KeyValuePair<string, string>("timeZoneAdjustmentFactor", "21600"),
            });
            var upload_resp = await httpClient.PostAsync("https://gotoes.org/strava/upload.php", formContent);
            var statusCode = upload_resp.StatusCode;
            if (statusCode != HttpStatusCode.MovedPermanently)
            {
                File.AppendAllText("log.txt", $"status code of upload post request: {statusCode}");
                // throw new Exception($"invalid status code: {statusCode}");
            }

            var download_url =
                $"https://gotoes.org/strava/downloader.php?sts=&d=downloads&s=Running&f={activity_number}.gpx&aba=&suppliedName=";
            response = await httpClient.GetAsync(download_url);
            upload_resp.EnsureSuccessStatusCode();

            var downloadedFileName = response.Content.Headers.ContentDisposition?.FileName ?? string.Empty;

            if (downloadedFileName == "")
            {
                downloadedFileName = "error.html";
            }

            downloadedFileName = downloadedFileName.Replace("\"", "");

            Stream streamToReadFrom = await response.Content.ReadAsStreamAsync();

            httpClient.Dispose();
            Stream file = File.Create(downloadedFileName);
            streamToReadFrom.CopyTo(file);
            file.Close();
            MessageBox.Show("Файл загружен");
            DropAreaLabel.Content = "Drop Files Here";
        }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Test message");
        }
    }

}

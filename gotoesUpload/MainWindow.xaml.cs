using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;


namespace gotoesUpload
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window
    {
        public UIState UiState;
        public Uploader Uploader;
        public Downloader Downloader;
        public ActivitySettings ActivitySettings;
        private string[] _filesFromDrop;
        public MainWindow()
        {
            InitializeComponent();
            UiState = new UIState(this);
            Uploader = new Uploader(UiState);
            Uploader.FilesWereUploaded += AfterUpload;
            ActivitySettings = new ActivitySettings(UiState);
            Downloader = new Downloader(ActivitySettings);
            Downloader.FileWasDownload += AfterDownload;
        }

        public void AfterUpload(object sender, EventArgs e)
        {
            UiState.UpdateDropArea("Files were uploaded, wait...");
            Downloader.DownloadMergedFile((e as Uploader.FilesWereUploadedEventArgs).ActivityNumber);
        }

        public void AfterDownload(object sender, EventArgs e)
        {
            UIState.ShowMessage("File was downloaded");
            UiState.UpdateDropArea("Drop Files Here");
        }

        private void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            var files = UiState.OnDrop(sender, e);
            _filesFromDrop = files;
            ActivitySettings.FileNames = files;
            UiState.GetState().AutoUpload = (bool)CheckBoxAutoUpload.IsChecked;
            if (UiState.GetState().AutoUpload)
            {
                StartUploading(files);
            }
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UiState.UpdateState().ActivityType = ((sender as ListBox).SelectedItem as ListBoxItem).Content.ToString();
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (UiState is not null)
            {
                UiState.UpdateState().AutoUpload = (bool)((CheckBox)sender).IsChecked;
            }
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (UiState is not null)
            {
                UiState.UpdateState().AutoUpload = (bool)((CheckBox)sender).IsChecked;
            }
        }

        public async void StartUploading(string[] files)
        {
            if (files.Length != 0) if (UiState.CheckFileExtension(files)) await Uploader.LoadFiles(files);
                else UIState.ShowMessage("Should be only gpx or tcx files");
        }

        private void ButtonUpload_OnClick(object sender, RoutedEventArgs e)
        {
            StartUploading(_filesFromDrop);
        }
    }
}

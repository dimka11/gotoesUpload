using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace gotoesUpload
{
    public class UIState
    {
        private MainWindow _window;
        private UiStateSettings _uiStateSettings;

        public UIState(MainWindow window)
        {
            _window = window;
            _uiStateSettings = new UiStateSettings()
            {
                AutoUpload = true,
                ActivityType = "Running"
            };
        }
        public string[] OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                var old = UpdateDropArea("");
                foreach (var file in files)
                {
                    old = UpdateDropArea(old + file.Split("\\")[file.Split("\\").Length - 1] + "\n");
                }

                return files;
            }

            return Array.Empty<string>();
        }

        public string UpdateDropArea(string content)
        {
            _window.DropAreaLabel.Content = content;
            return _window.DropAreaLabel.Content.ToString();
        }

        public static void ShowMessage(string content)
        {
            MessageBox.Show(content);
        }

        public bool CheckFileExtension(string[] files)
        {
            foreach (var file in files)
            {
                var extension = file.Split("\\")[^1].Split(".")[^1];
                if (extension is not "gpx" and not "tcx") return false;
            }
            return true;
        }

        public class UiStateSettings
        {
            public bool AutoUpload { get; set; }
            public string ActivityType { get; set; }
        }

        public UiStateSettings UpdateState()
        {
            return _uiStateSettings;
        }

        public UiStateSettings GetState()
        {
            return _uiStateSettings;
        }
    }
}

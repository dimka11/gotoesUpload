using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gotoesUpload
{
    public class ActivitySettings
    {
        private UIState uiState;
        public ActivitySettings(UIState uiState)
        {
            this.uiState = uiState;
            ActivityType = uiState.GetState().ActivityType;
        }

        public string[] FileNames { get; set; }

        public string ActivityType { get; set; }

        public void Update()
        {

        }
    }
}

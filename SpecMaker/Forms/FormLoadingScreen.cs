using System;
using System.Windows.Forms;

namespace SpecMaker
{
    public partial class FormLoadingScreen : Form
    {
        public IProgress<ProgressHelper> ProgressReporter { get; private set; }
        public FormLoadingScreen()
        {
            InitializeComponent();
            ProgressReporter = new Progress<ProgressHelper>(UpdateStatusTextAndProgressBar);
        }
        private void UpdateStatusTextAndProgressBar(ProgressHelper p)
        {
            (string text, int percent) = p;
            if (text != string.Empty) toolStripStatusLabelText.Text = text;
            progressBar.Value = percent;
        }
    }
}

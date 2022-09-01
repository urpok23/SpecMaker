using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Text.Unicode;
using System.Text.Encodings.Web;

namespace SpecMaker
{
    public partial class FormProjectInfoScreen : Form
    {
        // if layout problem occurs, find proper combination of ProjInfoScreen.AutoSize and pictureBox.SizeMode
        private static string ProjectInfoConfigPath { get; } = ProjectInfoDataSchema.ProjectInfoFilePath;
        private AppSettingsSchema AppSettings { get; }
        public ProjectInfoDataSchema ProjectInfo { get; private set; }
        public FormProjectInfoScreen(AppSettingsSchema appSettigns)
        {
            AppSettings = appSettigns;

            InitializeComponent();
            InitializeCustomProperties();
        }
        private void InitializeCustomProperties()
        {
            ProjectInfo = Utilities.ReadConfig<ProjectInfoDataSchema>(ProjectInfoConfigPath);
            UpdateProjectInfoOnScreen(ProjectInfo);

            FillPersons();
            InitFileDialogProperties();
        }

        private void FillPersons()
        {
            ComboBox[] comboBoxes = new[]
            {
                comboBoxDesigner,
                comboBoxChecker,
                comboBoxTeamLead,
                comboBoxNormController,
                comboBoxApprover
            };
            foreach (ComboBox comBox in comboBoxes)
            {
                comBox.Items.AddRange(AppSettings.SupportedPersons);
            }
        }
        private void InitFileDialogProperties()
        {
            Customize(openFileDialogImportProjectInfo);
            Customize(saveFileDialogExportProjectInfo);

            static void Customize (FileDialog fd)
            {
                fd.InitialDirectory = Application.StartupPath;
                fd.Filter = "Project Info (*.json)|*json";
                fd.FilterIndex = 1;
                fd.RestoreDirectory = true;
            }
        }

        private void WriteProjectInfo()
        {
            ProjectInfo.Designer        = comboBoxDesigner.Text;
            ProjectInfo.Checker         = comboBoxChecker.Text;
            ProjectInfo.TeamLead        = comboBoxTeamLead.Text;
            ProjectInfo.NormController  = comboBoxNormController.Text;
            ProjectInfo.Approver        = comboBoxApprover.Text;
            ProjectInfo.ProjectCode     = textBoxProjectCode.Text;
            ProjectInfo.Stage           = textBoxProjectStage.Text;
            ProjectInfo.Plant           = textBoxPlant.Text;
            ProjectInfo.Customer        = textBoxCustomer.Text;
            ProjectInfo.Date            = textBoxDate.Text;
        }
        private void UpdateProjectInfoOnScreen(ProjectInfoDataSchema pi)
        {
            comboBoxDesigner.Text           = pi.Designer;
            comboBoxChecker.Text            = pi.Checker;
            comboBoxTeamLead.Text           = pi.TeamLead;
            comboBoxNormController.Text     = pi.NormController;
            comboBoxApprover.Text           = pi.Approver;
            textBoxProjectCode.Text         = pi.ProjectCode;
            textBoxProjectStage.Text        = pi.Stage;
            textBoxPlant.Text               = pi.Plant;
            textBoxCustomer.Text            = pi.Customer;
            textBoxDate.Text                = pi.Date;
        }
        private ProjectInfoDataSchema GetProjectInfoFromScreen()
        {
            return new ProjectInfoDataSchema()
            {
                Designer = comboBoxDesigner.Text,
                Checker = comboBoxChecker.Text,
                TeamLead = comboBoxTeamLead.Text,
                NormController = comboBoxNormController.Text,
                Approver = comboBoxApprover.Text,
                ProjectCode = textBoxProjectCode.Text,
                Stage = textBoxProjectStage.Text,
                Plant = textBoxPlant.Text,
                Customer = textBoxCustomer.Text,
                Date = textBoxDate.Text,
            };
    }
        private async Task WriteProjectInfoToDisk1()
        {
            string fileName = $"{ProjectInfo.ProjectCode[..4]} project info.json";
            byte[] b = JsonSerializer.SerializeToUtf8Bytes(ProjectInfo);
            await using FileStream f = File.Create(fileName);
            await f.WriteAsync(b);
        }
        private async Task WriteProjectInfoToDisk2(string filePath, ProjectInfoDataSchema pi)
        {
            await using FileStream f = File.Create(filePath);

            var options = new JsonSerializerOptions
            {
                // don't escape cyrillic symbols for readability
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };
            await JsonSerializer.SerializeAsync(f, pi, options);
        }

        private void buttonImportProjectInfo_Click(object sender, EventArgs e)
        {
            using var ofd = openFileDialogImportProjectInfo;
            if (ofd.ShowDialog(owner: this) != DialogResult.OK) return;

            var pi = Utilities.ReadConfig<ProjectInfoDataSchema>(ofd.FileName);
            UpdateProjectInfoOnScreen(pi);
            //WriteProjectInfo();
        }

        private async void buttonExportProjectInfo_Click(object sender, EventArgs e)
        {
            using var sfd = saveFileDialogExportProjectInfo;
            sfd.AddExtension = true;
            sfd.DefaultExt = ".json";
            if (sfd.ShowDialog(owner: this) != DialogResult.OK) return;

            await WriteProjectInfoToDisk2(sfd.FileName, GetProjectInfoFromScreen());
        }

        private void buttonCancelChanges_Click(object sender, EventArgs e)
        {
            //UpdateProjectInfoOnScreen(ProjectInfo);
            this.Close();
            //this.Dispose();
        }
        private async void buttonApplyChanges_Click(object sender, EventArgs e)
        {
            WriteProjectInfo();
            //await WriteProjectInfoToDisk1();
            await WriteProjectInfoToDisk2(ProjectInfoConfigPath, ProjectInfo);
        }

    }
}

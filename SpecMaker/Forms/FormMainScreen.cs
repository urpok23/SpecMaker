using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpecMaker
{
    public partial class FormMainScreen : Form
    {
        private FormProjectInfoScreen ProjectInfoScreen { get; }
        public AppSettingsSchema AppSettings { get; }
        public SpecSettignsSchema SpecSettings { get; }

        public FormMainScreen()
        {
            InitializeComponent();
            InitializeFileDialog();

            AppSettings = Utilities.ReadConfig<AppSettingsSchema>(AppSettingsSchema.PathToAppSettings);
            SpecSettings = Utilities.ReadConfig<SpecSettignsSchema>(SpecSettignsSchema.PathToSpecSettings);
            ProjectInfoScreen = new FormProjectInfoScreen(AppSettings);
# if DEBUG
            tbExcelFormFilePath.Text = @"C:\MyFolder\csharp\SpecMaker\SpecMaker\Resources\ExcelTemplates\0342-Спецификация арматуры+Антон.xlsx";
#endif
        }

        private void InitializeFileDialog()
        {
#if DEBUG
            openFileDialogSelectExcelForm.InitialDirectory = @"C:\MyFolder\csharp\SpecMaker\SpecMaker\Resources\ExcelTemplates";
#else
            openFileDialogSelectExcelForm.InitialDirectory = Application.StartupPath;
#endif
            openFileDialogSelectExcelForm.Filter = "Excel files (*.xls, *.xlsx, *.xlsm)|*.xls;*.xlsx;*.xlsm";
            openFileDialogSelectExcelForm.FilterIndex = 1;
            openFileDialogSelectExcelForm.RestoreDirectory = true;
        }

        private void bOpenExcelForm_Click(object sender, EventArgs e)
        {
            if (openFileDialogSelectExcelForm.ShowDialog() == DialogResult.OK)
            {
                tbExcelFormFilePath.Text = openFileDialogSelectExcelForm.FileName;
                statusLabel.Text = "Проверьте данные о проекте и выгрузите спецификацию";
            }
        }

        private async void bMakeSpec_Click(object sender, EventArgs e)
        {
            try
            {
                using FormLoadingScreen loadingScreen = new ();

                //validate screen state here...
                if (string.IsNullOrEmpty(tbExcelFormFilePath.Text)) 
                    throw new ArgumentException( $"Выберите путь к файлу Excel-формы." );

                var c = new SpecCreator
                (
                    excelFormPath: tbExcelFormFilePath.Text,
                    projInfo: ProjectInfoScreen.ProjectInfo,
                    appSettings: AppSettings,
                    specSettings: SpecSettings,
                    ProgressReporter: loadingScreen.ProgressReporter
                );
                loadingScreen.Show();
                this.Enabled = false;

                await Task.Run( () => c.MakeSpecificationAsync() );
                statusLabel.Text = "Готово";
            }

            catch (AggregateException ex)
            {
                int i = 0;
                var res = new StringBuilder();
                foreach (var inner_ex in ex.InnerExceptions)
                {
                    res.Append($"{++i}) {inner_ex.Message}\n\n");
                }
                MessageBox.Show(res.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (System.Reflection.TargetInvocationException ex)
            {
                MessageBox.Show(ex.InnerException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Enabled = true;
                this.Focus();
            }
        }

        private void bChangeProjectInfo_Click(object sender, EventArgs e)
        {   
            var res = ProjectInfoScreen.ShowDialog(owner: this);
            //if (res == DialogResult.Cancel)
            //MessageBox.Show($"{res}");
        }
    }
}
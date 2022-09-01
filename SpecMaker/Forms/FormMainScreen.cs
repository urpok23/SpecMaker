using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpecMaker
{
    public partial class FormMainScreen : Form
    {
        public AppSettingsSchema AppSettings 
        { 
            get => Utilities.ReadConfig<AppSettingsSchema>(AppSettingsSchema.PathToAppSettings);
        }
        public SpecSettignsSchema SpecSettings 
        {
            get => Utilities.ReadConfig<SpecSettignsSchema>(SpecSettignsSchema.PathToSpecSettings);
        }        
        public ProjectInfoDataSchema ProjectInfo
        {
            get => Utilities.ReadConfig<ProjectInfoDataSchema>(ProjectInfoDataSchema.ProjectInfoFilePath);
        }

        public FormMainScreen()
        {
            InitializeComponent();
            InitializeFileDialog();
# if DEBUG
            tbExcelFormFilePath.Text = @"C:\MyFolder\csharp\SpecMaker\SpecMaker\Resources\ExcelTemplates\Пример формы.xlsx";
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
                //validate screen state here...
                if (string.IsNullOrEmpty(tbExcelFormFilePath.Text)) 
                    throw new ArgumentException( $"Выберите путь к файлу Excel-формы." );
                
                if ( ! File.Exists(tbExcelFormFilePath.Text)) 
                    throw new ArgumentException( $"Файл {tbExcelFormFilePath.Text} не существует." );

                using FormLoadingScreen loadingScreen = new();

                var c = new SpecCreator
                (
                    excelFormPath: tbExcelFormFilePath.Text,
                    projInfo: ProjectInfo,
                    appSettings: AppSettings,
                    specSettings: SpecSettings,
                    progressReporter: loadingScreen.ProgressReporter
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
                foreach (var innerEx in ex.InnerExceptions)
                {
                    res.Append($"{++i}) {innerEx.Message}\n\n");
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
            using FormProjectInfoScreen ProjectInfoScreen = new(AppSettings);
            var res = ProjectInfoScreen.ShowDialog(owner: this);
            //if (res == DialogResult.Cancel)
            //MessageBox.Show($"{res}");
        }
    }
}
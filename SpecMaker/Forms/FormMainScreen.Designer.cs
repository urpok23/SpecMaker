using System.Windows.Forms;

namespace SpecMaker
{
    partial class FormMainScreen
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbExcelFormFilePath = new System.Windows.Forms.TextBox();
            this.openFileDialogSelectExcelForm = new System.Windows.Forms.OpenFileDialog();
            this.bOpenExcelForm = new System.Windows.Forms.Button();
            this.bMakeSpec = new System.Windows.Forms.Button();
            this.bChangeProjectInfo = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbExcelFormFilePath
            // 
            this.tbExcelFormFilePath.Location = new System.Drawing.Point(12, 12);
            this.tbExcelFormFilePath.Name = "tbExcelFormFilePath";
            this.tbExcelFormFilePath.PlaceholderText = "Выберите файл Excel-формы...";
            this.tbExcelFormFilePath.ReadOnly = true;
            this.tbExcelFormFilePath.Size = new System.Drawing.Size(231, 27);
            this.tbExcelFormFilePath.TabIndex = 1;
            // 
            // bOpenExcelForm
            // 
            this.bOpenExcelForm.Location = new System.Drawing.Point(249, 10);
            this.bOpenExcelForm.Name = "bOpenExcelForm";
            this.bOpenExcelForm.Size = new System.Drawing.Size(94, 29);
            this.bOpenExcelForm.TabIndex = 2;
            this.bOpenExcelForm.Text = "Открыть";
            this.bOpenExcelForm.UseVisualStyleBackColor = true;
            this.bOpenExcelForm.Click += new System.EventHandler(this.bOpenExcelForm_Click);
            // 
            // bMakeSpec
            // 
            this.bMakeSpec.Location = new System.Drawing.Point(12, 100);
            this.bMakeSpec.Name = "bMakeSpec";
            this.bMakeSpec.Size = new System.Drawing.Size(331, 29);
            this.bMakeSpec.TabIndex = 3;
            this.bMakeSpec.Text = "Сделать спецификацию";
            this.bMakeSpec.UseVisualStyleBackColor = true;
            this.bMakeSpec.Click += new System.EventHandler(this.bMakeSpec_Click);
            // 
            // bChangeProjectInfo
            // 
            this.bChangeProjectInfo.Location = new System.Drawing.Point(12, 65);
            this.bChangeProjectInfo.Name = "bChangeProjectInfo";
            this.bChangeProjectInfo.Size = new System.Drawing.Size(331, 29);
            this.bChangeProjectInfo.TabIndex = 4;
            this.bChangeProjectInfo.Text = "Изменить данные о проекте";
            this.bChangeProjectInfo.UseVisualStyleBackColor = true;
            this.bChangeProjectInfo.Click += new System.EventHandler(this.bChangeProjectInfo_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 140);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(356, 26);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(211, 20);
            this.statusLabel.Text = "Выберите файл Excel-формы";
            // 
            // FormMainScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(356, 166);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.bChangeProjectInfo);
            this.Controls.Add(this.bMakeSpec);
            this.Controls.Add(this.bOpenExcelForm);
            this.Controls.Add(this.tbExcelFormFilePath);
            this.MaximizeBox = false;
            this.Name = "FormMainScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Спецификация";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TextBox tbExcelFormFilePath;
        private OpenFileDialog openFileDialogSelectExcelForm;
        private Button bOpenExcelForm;
        private Button bMakeSpec;
        private Button bChangeProjectInfo;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statusLabel;
    }
}
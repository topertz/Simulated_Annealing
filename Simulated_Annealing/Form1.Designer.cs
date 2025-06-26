namespace Simulated_Annealing
{
    partial class Form1
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
            buttonLoadFile = new Button();
            buttonCalculate = new Button();
            listBoxResults = new ListBox();
            pictureBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // buttonLoadFile
            // 
            buttonLoadFile.Location = new Point(79, 72);
            buttonLoadFile.Name = "buttonLoadFile";
            buttonLoadFile.Size = new Size(157, 30);
            buttonLoadFile.TabIndex = 0;
            buttonLoadFile.Text = "Load File";
            buttonLoadFile.UseVisualStyleBackColor = true;
            // 
            // buttonCalculate
            // 
            buttonCalculate.Location = new Point(79, 124);
            buttonCalculate.Name = "buttonCalculate";
            buttonCalculate.Size = new Size(157, 39);
            buttonCalculate.TabIndex = 1;
            buttonCalculate.Text = "Calculate Distance";
            buttonCalculate.UseVisualStyleBackColor = true;
            // 
            // listBoxResults
            // 
            listBoxResults.FormattingEnabled = true;
            listBoxResults.ItemHeight = 20;
            listBoxResults.Location = new Point(363, 49);
            listBoxResults.Name = "listBoxResults";
            listBoxResults.Size = new Size(320, 344);
            listBoxResults.TabIndex = 2;
            // 
            // pictureBox
            // 
            pictureBox.Location = new Point(75, 197);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(256, 197);
            pictureBox.TabIndex = 3;
            pictureBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pictureBox);
            Controls.Add(listBoxResults);
            Controls.Add(buttonCalculate);
            Controls.Add(buttonLoadFile);
            Name = "Form1";
            Text = "Simulated Annealing";
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button buttonLoadFile;
        private Button buttonCalculate;
        private ListBox listBoxResults;
        private PictureBox pictureBox;
    }
}
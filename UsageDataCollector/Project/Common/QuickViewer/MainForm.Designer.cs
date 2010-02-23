namespace QuickViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FileContents = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // FileContents
            // 
            this.FileContents.AllowDrop = true;
            this.FileContents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileContents.Location = new System.Drawing.Point(0, 0);
            this.FileContents.Multiline = true;
            this.FileContents.Name = "FileContents";
            this.FileContents.ReadOnly = true;
            this.FileContents.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.FileContents.Size = new System.Drawing.Size(650, 386);
            this.FileContents.TabIndex = 0;
            this.FileContents.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.FileContents.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 386);
            this.Controls.Add(this.FileContents);
            this.Name = "MainForm";
            this.Text = "Xml.Gz Quick Viewer";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox FileContents;
    }
}


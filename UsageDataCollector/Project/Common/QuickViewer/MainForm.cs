using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.UsageDataCollector.ServiceLibrary.Import;
using ICSharpCode.UsageDataCollector.Contracts;
using System.Runtime.Serialization;
using System.IO;

namespace QuickViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] fileDrop = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (null != fileDrop)
                {
                    string filename = fileDrop.GetValue(0).ToString();

                    UsageDataMessage currentMessage = FileImporter.ReadMessage(filename);

                    DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(UsageDataMessage));
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        dataContractSerializer.WriteObject(memoryStream, currentMessage);
                        byte[] data = new byte[memoryStream.Length];
                        Array.Copy(memoryStream.GetBuffer(), data, data.Length);
                        string message = Encoding.UTF8.GetString(data);

                        FileContents.Text = message;
                    }
                }
            }
            catch (Exception ex)
            {
                FileContents.Text = ex.ToString();
            }
        }


        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;

        }
    }
}

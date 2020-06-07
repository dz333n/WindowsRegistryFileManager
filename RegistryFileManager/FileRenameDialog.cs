using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RegistryFileManager
{
    public partial class FileRenameDialog : Form
    {
        RegistryFileManager Files;
        string FileName;

        public FileRenameDialog(RegistryFileManager files, string fileName)
        {
            InitializeComponent();

            Files = files;
            FileName = fileName;

            tbNewName.Text = fileName;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAccept_Click(object sender, EventArgs e)
        {
            Files.FileRenameAsync(FileName, tbNewName.Text);
            this.Close();
        }

        private void FileRenameDialog_Load(object sender, EventArgs e)
        {
            tbNewName.Focus();
        }
    }
}

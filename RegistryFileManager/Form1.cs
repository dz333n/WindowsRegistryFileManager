using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RegistryFileManager
{
    public partial class Form1 : Form
    {
        public RegFiles Files = new RegFiles(Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("Registry File Manager"));

        public Form1()
        {
            InitializeComponent();

            linkLabel1.Text = Files.Key.ToString();

            Files.FileAdded += Files_FileAdded;
            Files.FileRemoved += Files_FileRemoved;

            Files.Start();
        }

        private void Files_FileRemoved(RegFiles sender, string fileName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Files_FileRemoved(sender, fileName)));
                return;
            }

            foreach (var thing in listView1.Items)
            {
                var item = thing as ListViewItem;

                if (item.Text == fileName)
                {
                    listView1.Items.Remove(item);
                    break;
                }
            }
        }

        private void Files_FileAdded(RegFiles sender, string fileName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Files_FileAdded(sender, fileName)));
                return;
            }

            ListViewItem i = new ListViewItem();
            i.Text = fileName;

            listView1.Items.Add(i);
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Title = "Add file to registry";
                if (d.ShowDialog() == DialogResult.OK) 
                    Files.AddFileAsync(new System.IO.FileInfo(d.FileName)); 
            }
        }

        private void btnDelAll_Click(object sender, EventArgs e)
        {
            Files.DeleteAllFiles();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            Files.Start();
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (listView1.FocusedItem.Bounds.Contains(e.Location))
                {
                    fileContext.Show(Cursor.Position);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;
        }

        private void copyToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;

            using (SaveFileDialog s = new SaveFileDialog())
            {
                if (s.ShowDialog() == DialogResult.OK)
                    Files.WriteFileAsync(item.Text, s.FileName);
            }
        }

        private void moveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;

            using (SaveFileDialog s = new SaveFileDialog())
            {
                if (s.ShowDialog() == DialogResult.OK)
                    Files.MoveFileFromRegistry(item.Text, s.FileName);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;

            using (SaveFileDialog s = new SaveFileDialog())
            {
                if (s.ShowDialog() == DialogResult.OK)
                    Files.DeleteFile(item.Text);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(linkLabel1.Text);
            MessageBox.Show("Copied");
        }
    }
}

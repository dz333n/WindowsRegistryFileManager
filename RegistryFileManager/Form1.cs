using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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

            Files.FileAddStart += Files_FileAddStart;
            Files.FileAddEnd += Files_FileAddEnd;
            Files.FileRemoved += Files_FileRemoved;
            Files.FileWriteStart += Files_FileWriteStart;
            Files.FileWriteEnd += Files_FileWriteEnd;

            Files.Start();
        }

        private void Files_FileAddEnd(RegFiles sender, string fileName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Files_FileAddEnd(sender, fileName)));
                return;
            }

            ListViewItem i = new ListViewItem();
            i.Text = fileName;

            listView1.Items.Add(i);

            SetCursor(Cursors.Default);
        }

        private void Files_FileAddStart(RegFiles sender, string fileName)
            => SetCursor(Cursors.AppStarting);

        private void Files_FileWriteEnd(RegFiles sender, string fileName, string localPath)
            => SetCursor(Cursors.Default);

        private void Files_FileWriteStart(RegFiles sender, string fileName, string localPath)
            => SetCursor(Cursors.AppStarting);

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

            new Thread(() =>
            {
                var temp = System.IO.Path.GetTempPath();
                var path = System.IO.Path.Combine(temp, item.Text);

                Files.WriteFile(item.Text, path);

                SetCursor(Cursors.AppStarting);

                Process.Start(path);

                SetCursor(Cursors.Default);
            }).Start();
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

        public void SetCursor(Cursor c)
        {
            if(InvokeRequired)
            {
                Invoke(new Action(() => SetCursor(c)));
                return;
            }

            this.Cursor = c;
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
            Files.DeleteFile(item.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(linkLabel1.Text);
            MessageBox.Show("Copied");
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.FocusedItem.Bounds.Contains(e.Location))
                openToolStripMenuItem_Click(null, null);
        }
    }
}

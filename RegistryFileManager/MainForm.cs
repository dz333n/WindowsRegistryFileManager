using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RegistryFileManager
{
    public partial class MainForm : Form
    {
        #region NET20_Things
        public delegate void Action();
        public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
        public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
        public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        public delegate TResult Func<TResult>();
        public delegate TResult Func<T, TResult>(T arg);
        public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
        public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
        public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        #endregion

        private RegistryFileManager regFiles = new RegistryFileManager(Registry.CurrentUser.CreateSubKey("Software").CreateSubKey("Registry File Manager"));

        public MainForm()
        {
            InitializeComponent();

            linkLabel1.Text = regFiles.Key.ToString();

            // Events

            regFiles.FileAddEnd += Files_FileAddEnd;
            regFiles.FileAddStart += Files_FileAddStart;

            regFiles.FileRemoveBegin += Files_FileRemoveBegin;
            regFiles.FileRemoveEnd += Files_FileRemoveEnd;

            regFiles.FileCopyRegToLocalBegin += Files_FileCopyRegToLocalBegin;
            regFiles.FileCopyRegToLocalEnd += Files_FileCopyRegToLocalEnd;

            regFiles.FileMoveRegToLocalBegin += Files_FileMoveRegToLocalBegin;
            regFiles.FileMoveRegToLocalEnd += Files_FileMoveRegToLocalEnd;

            regFiles.Start();
        }

        private void Files_FileMoveRegToLocalEnd(RegistryFileManager sender, string fileName, string localPath, Exception ex = null)
            => BasicEndHandler(ex);

        private void Files_FileMoveRegToLocalBegin(RegistryFileManager sender, string fileName, string localPath)
            => BasicBeginHandler(true);

        private void Files_FileCopyRegToLocalEnd(RegistryFileManager sender, string fileName, string localPath, Exception ex = null)
            => BasicEndHandler(ex);

        private void Files_FileCopyRegToLocalBegin(RegistryFileManager sender, string fileName, string localPath)
            => BasicBeginHandler(true);

        private void Files_FileRemoveEnd(RegistryFileManager sender, string fileName, Exception exception = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Files_FileRemoveEnd(sender, fileName, exception)));
                return;
            }

            BasicEndHandler(exception);

            if (exception == null)
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

        private void Files_FileRemoveBegin(RegistryFileManager sender, string fileName)
            => BasicBeginHandler();

        private void Files_FileAddEnd(RegistryFileManager sender, string fileName, Exception ex)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Files_FileAddEnd(sender, fileName, ex)));
                return;
            }

            BasicEndHandler(ex);

            if (ex == null)
            {
                ListViewItem i = new ListViewItem();
                i.Text = fileName;

                listView1.Items.Add(i);
            }
        }

        private void BasicBeginHandler(bool enabled = false)
        {
            SetCursor(Cursors.AppStarting);
            // this.Enabled = enabled;
        }

        private void BasicEndHandler(params object[] things)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => BasicEndHandler(things)));
                return;
            }

            SetCursor(Cursors.Default);
            // this.Enabled = true;

            foreach (var param in things)
            {
                if (param is Exception && param != null)
                    MessageBox.Show("Error:\r\n" + param.ToString());
            }
        }

        private void Files_FileAddStart(RegistryFileManager sender, string fileName)
            => SetCursor(Cursors.AppStarting);
        
        private void btnAddFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Title = "Add files to registry";
                d.Multiselect = true;
                if (d.ShowDialog() == DialogResult.OK) 
                    regFiles.AddFilesAsync(d.FileNames); 
            }
        }

        private void btnDelAll_Click(object sender, EventArgs e)
        {
            regFiles.DeleteAllFilesAsync();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            regFiles.Start();
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
                try
                {
                    regFiles.CopyFileFromRegToLocal(item.Text, path);

                    SetCursor(Cursors.AppStarting);

                    Process.Start(path);
                }
                catch (Exception ex)
                {
                    BasicEndHandler(ex);
                }
                finally
                {
                    SetCursor(Cursors.Default);
                }
            }).Start();
        }

        private void copyToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;

            using (SaveFileDialog s = new SaveFileDialog())
            {
                if (s.ShowDialog() == DialogResult.OK)
                    regFiles.CopyFileFromRegToLocalAsync(item.Text, s.FileName);
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
                    regFiles.MoveFileFromRegToLocalAsync(item.Text, s.FileName);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;
            regFiles.DeleteFileAsync(item.Text);
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

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = listView1.SelectedItems[0] as ListViewItem;

            new FileRenameDialog(regFiles, item.Text).ShowDialog();
        }
    }
}

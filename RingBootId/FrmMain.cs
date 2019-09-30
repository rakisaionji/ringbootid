using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RingBootId
{
    public partial class FrmMain : Form
    {
        private RingBootId currentData;

        public FrmMain()
        {
            InitializeComponent();
        }

        private bool ReadBootId(string path)
        {
            var encoding = Encoding.UTF8;
            short vmajor, vminor; // Version
            short yy, mm, dd, hh, mi, ss; // Date

            if (File.Exists(path))
            {
                using (var rd = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    // Check signature
                    rd.BaseStream.Seek(0x8, SeekOrigin.Begin);
                    var sign = encoding.GetString(rd.ReadBytes(4));
                    if (!sign.Equals("BTID"))
                    {
                        MessageBox.Show("Invalid Boot ID file");
                        return false;
                    }
                    rd.ReadByte();
                    currentData = new RingBootId();
                    var count = rd.ReadByte();
                    if (count > 2) count = 2;
                    currentData.AppInfo = new RingAppInfo[count];
                    // Read basic info
                    rd.BaseStream.Seek(0x10, SeekOrigin.Begin);
                    currentData.AppId = encoding.GetString(rd.ReadBytes(4));
                    for (int i = 0; i < count; i++)
                    {
                        var appif = new RingAppInfo();
                        yy = rd.ReadInt16();
                        mm = rd.ReadByte();
                        dd = rd.ReadByte();
                        hh = rd.ReadByte();
                        mi = rd.ReadByte();
                        ss = rd.ReadByte();
                        rd.ReadByte();
                        vminor = rd.ReadInt16();
                        vmajor = rd.ReadInt16();
                        appif.Date = new DateTime(yy, mm, dd, hh, mi, ss);
                        appif.Version = new Version(vmajor, vminor);
                        appif.AppId = currentData.AppId;
                        currentData.AppInfo[count - i - 1] = appif;
                        rd.ReadBytes(20); // Skip 20 bytes of unknown shit
                    }
                    // Read app name
                    byte buf;
                    var arr = new List<byte>();
                    rd.BaseStream.Seek(0x60, SeekOrigin.Begin);
                    do
                    {
                        buf = rd.ReadByte();
                        if (buf != 0) arr.Add(buf);
                        else break;
                    }
                    while (true);
                    currentData.AppName = encoding.GetString(arr.ToArray());
                    // That's all so far
                    return true;
                }
            }
            return false;
        }

        private void LoadBootId()
        {
            var path = txtFileName.Text.Trim();
            var read = ReadBootId(path);

            txtOutput.Clear();
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("STORAGE INFORMATION");
            sb.AppendLine();
            if (!read)
            {
                sb.AppendLine("--- ERROR ---");
                return;
            }
            sb.AppendFormat("[{0}]", currentData.AppName);
            sb.AppendLine();
            for (int i = 0; i < currentData.AppInfo.Length; i++)
            {
                sb.AppendFormat("{0}: {1}", i, currentData.AppInfo[i]);
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("- END -");
            sb.AppendLine();
            txtOutput.Text = sb.ToString();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            txtFileName.Clear();
            txtOutput.Clear();
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            var od = new OpenFileDialog();
            od.Title = "Open File";
            od.Filter = "Ringedge Bootid Header Files|*.bootid.header|All Files (*.*)|*.*";
            od.Multiselect = false;
            od.CheckFileExists = true;
            if (od.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var path = od.FileName;
                    txtFileName.Text = path;
                    LoadBootId();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }
}

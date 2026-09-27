using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace HandsyasLoader
{
    public class LoaderForm : Form
    {
        string serverName = "Handsyas";
        string ip = "26.104.112.58";
        int port = 9960;
        string clientExe = "Conquer_modified.exe";
        string clientDir = @"E:\Client";

        Label nameLabel;
        Label statusLabel;
        Label addrLabel;
        Button playButton;
        Button checkButton;
        Timer statusTimer;

        static string BaseDir
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        static string LogPath
        {
            get { return Path.Combine(BaseDir, "loader.log"); }
        }

        public static void Log(string msg)
        {
            try
            {
                File.AppendAllText(LogPath,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + msg + "\r\n",
                    Encoding.UTF8);
            }
            catch { }
        }

        public LoaderForm()
        {
            LoadIni();
            BuildUi();
            statusTimer = new Timer();
            statusTimer.Interval = 10000;
            statusTimer.Tick += (s, e) => RefreshStatus();
            statusTimer.Start();
            this.Load += (s, e) => RefreshStatus();
            Log("Loader started.");
        }

        void LoadIni()
        {
            try
            {
                string ini = Path.Combine(BaseDir, "loader.ini");
                if (!File.Exists(ini)) return;
                foreach (string raw in File.ReadAllLines(ini, Encoding.UTF8))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line[0] == '#' || line[0] == ';') continue;
                    int i = line.IndexOf('=');
                    if (i <= 0) continue;
                    string k = line.Substring(0, i).Trim().ToLowerInvariant();
                    string v = line.Substring(i + 1).Trim();
                    switch (k)
                    {
                        case "servername": if (v.Length > 0) serverName = v; break;
                        case "ip": if (v.Length > 0) ip = v; break;
                        case "port":
                            int p;
                            if (int.TryParse(v, out p) && p > 0 && p < 65536) port = p;
                            break;
                        case "clientexe": if (v.Length > 0) clientExe = v; break;
                        case "clientdir": if (v.Length > 0) clientDir = v; break;
                    }
                }
            }
            catch (Exception ex) { Log("LoadIni error: " + ex.Message); }
        }

        void BuildUi()
        {
            this.Text = serverName + " - Loader";
            this.Size = new Size(400, 260);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            nameLabel = new Label();
            nameLabel.Text = serverName;
            nameLabel.Font = new Font("Tahoma", 20, FontStyle.Bold);
            nameLabel.Location = new Point(0, 10);
            nameLabel.Size = new Size(384, 50);
            nameLabel.TextAlign = ContentAlignment.MiddleCenter;

            statusLabel = new Label();
            statusLabel.Text = "...";
            statusLabel.Font = new Font("Tahoma", 11, FontStyle.Bold);
            statusLabel.Location = new Point(0, 62);
            statusLabel.Size = new Size(384, 28);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;

            addrLabel = new Label();
            addrLabel.Text = ip + " : " + port;
            addrLabel.Font = new Font("Tahoma", 9);
            addrLabel.ForeColor = Color.Gray;
            addrLabel.Location = new Point(0, 92);
            addrLabel.Size = new Size(384, 20);
            addrLabel.TextAlign = ContentAlignment.MiddleCenter;

            playButton = new Button();
            playButton.Text = "Start Game";
            playButton.Font = new Font("Tahoma", 13, FontStyle.Bold);
            playButton.Location = new Point(92, 122);
            playButton.Size = new Size(200, 52);
            playButton.Click += OnPlay;

            checkButton = new Button();
            checkButton.Text = "Check Status";
            checkButton.Location = new Point(142, 182);
            checkButton.Size = new Size(100, 28);
            checkButton.Click += (s, e) => RefreshStatus();

            Controls.Add(nameLabel);
            Controls.Add(statusLabel);
            Controls.Add(addrLabel);
            Controls.Add(playButton);
            Controls.Add(checkButton);
        }

        bool ServerOnline()
        {
            try
            {
                using (TcpClient c = new TcpClient())
                {
                    IAsyncResult r = c.BeginConnect(ip, port, null, null);
                    if (!r.AsyncWaitHandle.WaitOne(1500)) return false;
                    c.EndConnect(r);
                    return c.Connected;
                }
            }
            catch (Exception ex)
            {
                Log("ServerOnline error: " + ex.Message);
                return false;
            }
        }

        void RefreshStatus()
        {
            bool on = ServerOnline();
            statusLabel.Text = on ? "Server Online" : "Server Offline";
            statusLabel.ForeColor = on ? Color.Green : Color.Red;
        }

        static void BackupServerDat(string clientDir)
        {
            try
            {
                string dat = Path.Combine(clientDir, "Server.dat");
                string bak = Path.Combine(clientDir, "Server.dat.loader_bak");
                if (File.Exists(dat) && !File.Exists(bak))
                {
                    File.Copy(dat, bak);
                    Log("Server.dat backed up to Server.dat.loader_bak");
                }
            }
            catch (Exception ex) { Log("BackupServerDat error: " + ex.Message); }
        }

        void OnPlay(object sender, EventArgs e)
        {
            try
            {
                string exe = Path.IsPathRooted(clientExe)
                    ? clientExe
                    : Path.Combine(clientDir, clientExe);
                if (!File.Exists(exe))
                {
                    string m = "Client file not found: " + exe;
                    Log(m);
                    MessageBox.Show(m, serverName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ServerOnline())
                {
                    DialogResult r = MessageBox.Show(
                        "Server is not responding on " + ip + ":" + port + ".\nStart the game anyway?",
                        serverName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                    Log("Starting offline (server unreachable).");
                }
                BackupServerDat(clientDir);
                Process.Start(new ProcessStartInfo(exe) { WorkingDirectory = clientDir });
                Log("Client launched: " + exe);
            }
            catch (Exception ex)
            {
                Log("OnPlay error: " + ex);
                MessageBox.Show("Could not start the game:\n" + ex.Message,
                    serverName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

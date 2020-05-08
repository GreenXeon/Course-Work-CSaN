using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;
using Google;
using Google.Maps;
using Google.Maps.Geocoding;
using System.Data;
using System.Data.SqlClient;

namespace TelegramBot1
{    class Program
    {
        static TelegramBotClient Bot;
        static Station[] stations = new Station[29];
        static void Main(string[] args)
        {
            Bot = new TelegramBotClient(Info.apiKey);

            StationsInitialize(ref stations);

            Bot.OnMessage += MessageReceive;
            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        public static void StationsInitialize(ref Station[] stations)
        {
            string connectionstr = Info.sqlPath;
            SqlConnection connection = new SqlConnection(connectionstr);
            connection.Open();

            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT * FROM [Stations]", connection);
              
            try
            {
                sqlReader = command.ExecuteReader();
                
                int count = 0;
                while(sqlReader.Read())
                {
                    string Name = sqlReader["Name"].ToString();
                    double X = Convert.ToDouble(sqlReader["X"]);
                    double Y = Convert.ToDouble(sqlReader["Y"]);
                    stations[count] = new Station(Name, X, Y);
                    Console.WriteLine(sqlReader["Name"].ToString() + " " +
                        sqlReader["X"].ToString() + " " + sqlReader["Y"].ToString());
                    count++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
            }
        }

        public static string[] ComputingStation(double x, double y)
        {
            double[] dists = new double[29];
            int count = 0;
            foreach(Station station in stations)
            {
                dists[count] = Math.Sqrt(Math.Pow(Math.Abs(x-stations[count].Y), 2) +
                    Math.Pow(Math.Abs(y - stations[count].X), 2));
                count++;
            }
            double minVal = dists.Min();
            int minIndex = Array.IndexOf(dists, minVal);
            string[] statInfo = new string[3];
            statInfo[0] = stations[minIndex].Name;
            statInfo[1] = stations[minIndex].X.ToString();
            statInfo[2] = stations[minIndex].Y.ToString();
            return statInfo;
        }

        private static async void MessageReceive(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;
            
            if (message.Type == MessageType.Location)
            {
                float y = message.Location.Latitude;
                float x = message.Location.Longitude;
                //Console.WriteLine($"X: {x}, Y: {y}");
                //await Bot.SendTextMessageAsync(message.From.Id, $"You are situated here: X = {x}, Y = {y}");
                //await Bot.SendLocationAsync(message.From.Id, y, x);
                string[] resStation = ComputingStation(x, y);
                string user;
                if (message.From.LastName == " ")
                    user = $"{message.From.FirstName} {message.From.LastName}";
                else
                    user = $"{message.From.FirstName}";
                    Console.WriteLine($"{user} определил(-а) ближайшую" +
                        $" станцию: {resStation[0]}");
                await Bot.SendTextMessageAsync(message.From.Id, $"{char.ConvertFromUtf32(0x1F4CD)}" +
                    $" Ближайшая станция метро - '{resStation[0].Trim()}'. " +
                    $"Её геолокация представлена ниже.");
                await Bot.SendLocationAsync(message.From.Id, float.Parse(resStation[1]), float.Parse(resStation[2]));
                await Bot.SendTextMessageAsync(message.From.Id, $"Нажмите для получения дополнительной информации.");

            }
            else
            if (message.Type == MessageType.Text)
            {
                string textmes;
                switch (message.Text)
                { 
                    case "/start":
                        textmes = "Для определения ближайшей станции отправьте Вашу геолокацию.";
                        await Bot.SendTextMessageAsync(message.From.Id, textmes);
                        break;
                    case "/info":
                        textmes = "Бот выполнен в рамках курсовой работы по дисциплине КСиС. \n" +
                            "Доступные функции: \n" +
                            "/start - начало работы с ботом. \n" +
                            "/info - информация и боте и командах.";
                        await Bot.SendTextMessageAsync(message.From.Id, textmes);
                        break;
                    default:
                        textmes = "Извините, я не понимаю Вас.";
                        await Bot.SendTextMessageAsync(message.From.Id, textmes);
                        break;
                }
                string name;
                if (message.From.LastName != " ")
                    name = $"{message.From.FirstName}";
                else
                    name = $"{message.From.FirstName} {message.From.LastName}";
                Console.WriteLine($"{name} отправил сообщение: {message.Text}");
            }

        }
    }
}

using AzureStorageLibrary.Services;
using System;
using System.Text;
using System.Threading.Tasks;

namespace QueueConsoleApp
{
    internal class Program
    {
        private async static Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            AzureStorageLibrary.ConnectionStrings.AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=udemyrealstorageaccount;AccountKey=rzzWG1uOtG1SCvsC1xOz63N9iavDE2IJnbofGMHAAl/wW7oLbKwpWRlhWgiAJBq/CcdGIob1A123cmVVjuC0eg==;EndpointSuffix=core.windows.net";

            AzQueue queue = new AzQueue("ornekkuyruk");

            //   string base64message = Convert.ToBase64String(Encoding.UTF8.GetBytes("fatih çakıroğlu"));

            //       queue.SendMessageAsync(base64message).Wait();

            var message = queue.RetrieveNextMessageAsync().Result;

            //string text = Encoding.UTF8.GetString(Convert.FromBase64String(message.MessageText));

            //Console.WriteLine("mesaj:" + text);

            await queue.DeleteMessage(message.MessageId, message.PopReceipt);

            Console.WriteLine("Messaj silindi");
        }
    }
}
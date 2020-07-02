using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AzureStorageLibrary;
using AzureStorageLibrary.Models;
using AzureStorageLibrary.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace WatermarkProcessFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public async static Task Run([QueueTrigger("watermarkqueue")] PictureWatermarkQueue myQueueItem, ILogger log)
        {
            ConnectionStrings.AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=udemyrealstorageaccount;AccountKey=rzzWG1uOtG1SCvsC1xOz63N9iavDE2IJnbofGMHAAl/wW7oLbKwpWRlhWgiAJBq/CcdGIob1A123cmVVjuC0eg==;EndpointSuffix=core.windows.net";
            IBlobStorage blobStorage = new BlobStorage();
            INoSqlStorage<UserPicture> noSqlStorage = new TableStorage<UserPicture>();

            foreach (var item in myQueueItem.Pictures)
            {
                using var stream = await blobStorage.DownloadAsync(item, EContainerName.pictures);

                using var memoryStream = AddWaterMark(myQueueItem.WatermarkText, stream);

                await blobStorage.UploadAsync(memoryStream, item, EContainerName.watermarkpictures);

                log.LogInformation($"{item} resmine watermark eklenmiştir.");
            }

            var userpicture = await noSqlStorage.Get(myQueueItem.UserId, myQueueItem.City);

            if (userpicture.WatermarkRawPaths != null)
            {
                myQueueItem.Pictures.AddRange(userpicture.WatermarkPaths);
            }

            userpicture.WatermarkPaths = myQueueItem.Pictures;

            await noSqlStorage.Add(userpicture);

            HttpClient httpClient = new HttpClient();

            var response = await httpClient.GetAsync("https://localhost:44332/api/Notifications/CompleteWatermarkProcess/" + myQueueItem.ConnectionId);

            log.LogInformation($"Client({myQueueItem.ConnectionId}) bilgilendirilmiştir");
        }

        public static MemoryStream AddWaterMark(string watermarkText, Stream PictureStream)
        {
            MemoryStream ms = new MemoryStream();

            using (Image image = Bitmap.FromStream(PictureStream))
            {
                using (Bitmap tempBitmap = new Bitmap(image.Width, image.Height))
                {
                    using (Graphics gph = Graphics.FromImage(tempBitmap))
                    {
                        gph.DrawImage(image, 0, 0);

                        var font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);

                        var color = Color.FromArgb(255, 0, 0);

                        var brush = new SolidBrush(color);

                        var point = new Point(20, image.Height - 50);

                        gph.DrawString(watermarkText, font, brush, point);

                        tempBitmap.Save(ms, ImageFormat.Png);
                    }
                }
            }

            ms.Position = 0;

            return ms;
        }
    }
}
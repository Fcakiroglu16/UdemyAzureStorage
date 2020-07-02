using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorageLibrary;
using AzureStorageLibrary.Models;
using AzureStorageLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MvcWebApp.Models;
using Newtonsoft.Json;

namespace MvcWebApp.Controllers
{
    public class PicturesController : Controller
    {
        public string UserId { get; set; } = "12345";
        public string City { get; set; } = "istanbul";
        private readonly INoSqlStorage<UserPicture> _noSqlStorage;
        private readonly IBlobStorage _blobStorage;

        public PicturesController(INoSqlStorage<UserPicture> noSqlStorage, IBlobStorage blobStorage)
        {
            _noSqlStorage = noSqlStorage;
            _blobStorage = blobStorage;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserId = UserId;
            ViewBag.City = City;

            List<FileBlob> fileBlobs = new List<FileBlob>();

            var user = await _noSqlStorage.Get(UserId, City);

            if (user != null)
            {
                user.Paths.ForEach(x =>
                {
                    fileBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{EContainerName.pictures}/{x}" });
                });
            }
            ViewBag.fileBlobs = fileBlobs;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IEnumerable<IFormFile> pictures)
        {
            List<string> picturesList = new List<string>();
            foreach (var item in pictures)
            {
                var newPictureName = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";

                await _blobStorage.UploadAsync(item.OpenReadStream(), newPictureName, EContainerName.pictures);

                picturesList.Add(newPictureName);
            }

            var isUser = await _noSqlStorage.Get(UserId, City);

            if (isUser != null)
            {
                picturesList.AddRange(isUser.Paths);
                isUser.Paths = picturesList;
            }
            else
            {
                isUser = new UserPicture();

                isUser.RowKey = UserId;
                isUser.PartitionKey = City;
                isUser.Paths = picturesList;
            }

            await _noSqlStorage.Add(isUser);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ShowWatermark()
        {
            List<FileBlob> fileBlobs = new List<FileBlob>();
            UserPicture userPicture = await _noSqlStorage.Get(UserId, City);

            userPicture.WatermarkPaths.ForEach(x =>
            {
                fileBlobs.Add(new FileBlob { Name = x, Url = $"{_blobStorage.BlobUrl}/{EContainerName.watermarkpictures}/{x}" });
            });

            ViewBag.fileBlobs = fileBlobs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddWatermark(PictureWatermarkQueue pictureWatermarkQueue)

        {
            var jsonString = JsonConvert.SerializeObject(pictureWatermarkQueue);

            string jsonStringBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            AzQueue azQueue = new AzQueue("watermarkqueue");

            await azQueue.SendMessageAsync(jsonStringBase64);

            return Ok();
        }
    }
}
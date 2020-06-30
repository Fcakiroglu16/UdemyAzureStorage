using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AzureStorageLibrary;
using AzureStorageLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;

namespace MvcWebApp.Controllers
{
    public class TableStoragesController : Controller
    {
        private readonly INoSqlStorage<Product> _noSqlStorage;

        public TableStoragesController(INoSqlStorage<Product> noSqlStorage)
        {
            _noSqlStorage = noSqlStorage;
        }

        public IActionResult Index()
        {
            ViewBag.products = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = false;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            product.RowKey = Guid.NewGuid().ToString();
            product.PartitionKey = "Kalemler";

            await _noSqlStorage.Add(product);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(string rowKey, string partitionKey)
        {
            var product = await _noSqlStorage.Get(rowKey, partitionKey);

            ViewBag.products = _noSqlStorage.All().ToList();
            ViewBag.IsUpdate = true;

            return View("Index", product);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Product product)
        {
            ViewBag.IsUpdate = true;

            await _noSqlStorage.Update(product);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string rowKey, string partitionKey)
        {
            await _noSqlStorage.Delete(rowKey, partitionKey);
            return RedirectToAction("Index");
        }
    }
}
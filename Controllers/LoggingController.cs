using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class LoggingController : AuthorizedController
    {
        [HttpGet]
        [Route("/logs")]
        public async Task<IActionResult> LogsPage()
        {
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"{DateTime.Today:yyyyMMdd}.txt");
            var lastLines = (await System.IO.File.ReadAllLinesAsync(logFilePath)).Reverse().Take(50).ToList();

            return View(lastLines);
        }

        [HttpGet]
        [Route("/logs/all")]
        public async Task<IActionResult> GetLogs()
        {
            var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"{DateTime.Today:yyyyMMdd}.txt");
            var fileBytes = await System.IO.File.ReadAllBytesAsync(logFilePath);
            return File(fileBytes, "application/text", "log.txt");
        }

        [HttpGet]
        [Route("/logs/errors")]
        public async Task<IActionResult> GetErrorLogs()
        {
            var logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var directoryInfo = new DirectoryInfo(logDirectoryPath);

            var lastLogfile = directoryInfo.EnumerateFiles("errors-*.txt").OrderBy(x => x.CreationTimeUtc).FirstOrDefault();
            if (lastLogfile == null) return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(lastLogfile.FullName);
            return File(fileBytes, "application/text", "errors.txt");
        }
    }
}

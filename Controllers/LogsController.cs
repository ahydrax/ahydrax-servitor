using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class LogsController : AuthorizedController
    {
        [HttpGet]
        [Route("/logs")]
        public async Task<IActionResult> GetLogs()
        {
            var logDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            var directoryInfo = new DirectoryInfo(logDirectoryPath);

            var lastLogfile = directoryInfo.EnumerateFiles("*.txt").OrderBy(x => x.CreationTimeUtc).FirstOrDefault();
            if (lastLogfile == null) return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(lastLogfile.FullName);
            return File(fileBytes, "application/text", "log.txt");
        }
    }
}

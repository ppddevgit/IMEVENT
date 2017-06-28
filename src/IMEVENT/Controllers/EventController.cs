using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using IMEVENT.Models.EventViewModels;
using IMEVENT.Data;
using IMEVENT.Services;
using System.Threading;
using NLog;

namespace IMEVENT.Controllers
{
    public class EventController : Controller
    {
        private IHostingEnvironment _environment;
        private  ApplicationDbContext _context;
        private readonly int MAX_RETRY = 4;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public EventController(ApplicationDbContext context,IHostingEnvironment environment)
        {
            _environment = environment;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(EventCreateViewModel model, ICollection<IFormFile> files)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            foreach (IFormFile file in files)
            {
                if (file.Length > 0)
                {
                    string[] paths = file.FileName.Split('\\');
                    string filePath = Path.Combine(uploads, paths[paths.Length - 1]);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        ExcelDataExtractor dataExtractor = new ExcelDataExtractor();
                        dataExtractor.Source = filePath;
                        dataExtractor.DBcontext = ApplicationDbContext.GetDbContext(); 
                     
                        Event e = new Event(model.EventName, dataExtractor);
                        e.EndDate = model.EndDate;
                        e.StartDate = model.StartDate;
                        e.Theme = model.Theme;
                        e.Place = model.Venue;
                        e.MingleAttendees = false;
                        e.Fee = model.Fee;
                        e.Type = EventTypeEnum.GRANDE_RETRAITE;
                        e.Persist();
                        Thread t = new Thread(() => ProcessEvent(e, dataExtractor.Source));
                        t.Start();                            
                    }
                }
            }
            return View();
        }

        public void ProcessEvent(Event e, String source)
        {
            bool keepTrying = true;
            int i = 0;
            while (keepTrying && i != MAX_RETRY)
            {
                try
                {
                    e.ExtractEventDetails(source);
                    keepTrying = false;
                }
                catch (IOException ev)
                {
                    i = i + 1;
                    if (i == MAX_RETRY)
                    {
                        logger.Log(LogLevel.Error, "Unable to create event detail " + ev.StackTrace);
                    }
                    Thread.Sleep(2000);
                }
            }

            //Test Data Matching
            Events.DataMatchingGenerator badge = new Events.DataMatchingGenerator(e);
            if (!badge.GenerateAllBadges())
            {
                logger.Log(LogLevel.Error, "Error Printing Badges Data");                
                return;
            }

            string uploadDir = Path.Combine(_environment.WebRootPath, "uploads");
            badge.PrintBadgesToFile(uploadDir, false, true);
        }

    }
}
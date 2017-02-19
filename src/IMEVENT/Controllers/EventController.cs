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

namespace IMEVENT.Controllers
{
    public class EventController : Controller
    {
        private IHostingEnvironment _environment;
        private  ApplicationDbContext _context;
        private readonly int MAX_RETRY = 4;
       
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
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    String[] paths = file.FileName.Split('\\');
                    String filePath = Path.Combine(uploads, paths[paths.Length - 1]);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                         await file.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        ExcelDataExtractor dataExtractor = new ExcelDataExtractor();
                        dataExtractor.Source = filePath;
                        dataExtractor.DBcontext = ApplicationDbContext.GetDbContext(); 
                     
                        Event e = new Event(model.EventName, dataExtractor );
                        e.persist();
                        Thread t = new Thread(()=>ProcessEvent(e, dataExtractor.Source));
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
                    Console.WriteLine(ev.InnerException);
                    Thread.Sleep(2000);
                }

            }
        }

    }
}
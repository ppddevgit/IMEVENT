using IMEVENT.SharedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class FreeRefectory : IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Table { get; set; }
        public int Place { get; set; }
        public RegimeEnum Type { get; set; }        
        public int EventId { get; set; }

        public FreeRefectory()
        {
        }

        public FreeRefectory(int eventId, string name, RegimeEnum type, string table, int place)
        {
            this.EventId = eventId;
            this.Type = type;            
            this.Name = name;
            this.Table = table;
            this.Place = place;
        }

        public static List<FreeRefectory> GetFreeRefectoryList(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeRefectories.Where(x => x.EventId == eventId).ToList();
        }

        public static List<FreeRefectory> GetFreeRefectoryListByName(int eventId, string name)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeRefectories.Where(x => x.EventId == eventId && x.Name == name).ToList();
        }

        public static FreeRefectory GetAFreeTableSeatByType(int eventId, RegimeEnum type, bool invalidate = true)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeRefectory sec = context.FreeRefectories.Where(x => x.EventId == eventId && x.Type == type).FirstOrDefault();
            if (sec != null && invalidate)
            {
                //Remove item in DB and update
                context.FreeRefectories.Remove(sec);
                context.SaveChanges();
            }
            
            return sec;
        }

        public static int GetIdByProperties(int eventId, string name, RegimeEnum type, string table, int place)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeRefectory sec = context.FreeRefectories.Where(x => x.EventId == eventId
            && x.Name == name && x.Type == type && x.Table == table && x.Place == place).FirstOrDefault();
            return (sec == null) ? 0 : sec.Id;
        }

        public int Persist()
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            Id = Convert.ToInt32(GetRecordID());

            if (Id != 0)
            {
                context.Entry(this).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
            else
            {
                context.FreeRefectories.Add(this);
            }

            context.SaveChanges();
            return Id;
        }

        public object GetRecordID()
        {
            return GetIdByProperties(EventId, Name, Type, Table, Place);
        }
    }
}

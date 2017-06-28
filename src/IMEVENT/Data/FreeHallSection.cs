using IMEVENT.Events;
using IMEVENT.SharedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class FreeHallSection : IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Place { get; set; }
        public HallSectionTypeEnum Type { get; set; }
        public int EventId { get; set; }        

        public FreeHallSection()
        {

        }

        public FreeHallSection(int eventId, HallSectionTypeEnum type, string name, int place)
        {
            this.EventId = eventId;
            this.Type = type;
            this.Name = name;
            this.Place = place;            
        }

        public static List<FreeHallSection> GetFreeSectionList(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeHallSections.Where(x => x.EventId == eventId).ToList();
        }

        public static List<FreeHallSection> GetFreeSectionListByType(int eventId, HallSectionTypeEnum type, bool invalidate = true)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeHallSections.Where(x => x.EventId == eventId && x.Type == type).ToList();
        }

        public static int GetIdByProperties(int eventId, string name, HallSectionTypeEnum type, int place)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeHallSection sec = context.FreeHallSections.Where(x => x.EventId == eventId 
            && x.Type == type && x.Name == name && x.Place == place).FirstOrDefault();            
            return (sec == null)? 0 : sec.Id;            
        }

        public static FreeHallSection GetAFreeSectionByType(int eventId, HallSectionTypeEnum type, bool invalidate = true)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeHallSection sec = context.FreeHallSections.Where(x => x.EventId == eventId && x.Type == type).FirstOrDefault();
            if(sec != null && invalidate)
            {
                //Mark item as used and update DB
                context.FreeHallSections.Remove(sec);
                context.SaveChanges();
            }
            
            return sec;
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
                context.FreeHallSections.Add(this);
            }

            context.SaveChanges();
            return Id;
        }

        public object GetRecordID()
        {
            return GetIdByProperties(EventId, Name, Type, Place);
        }
    }
}

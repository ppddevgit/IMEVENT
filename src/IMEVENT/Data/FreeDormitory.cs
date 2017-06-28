using IMEVENT.SharedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class FreeDormitory : IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Place { get; set; }
        public DormitoryTypeEnum Type { get; set; }
        public DormitoryCategoryEnum CatType { get; set; }
        public int EventId { get; set; }        

        public FreeDormitory()
        {
        }

        public FreeDormitory(int eventId, DormitoryTypeEnum type, DormitoryCategoryEnum catType, string name, int place)
        {
            this.EventId = eventId;
            this.Type = type;
            this.CatType = catType;
            this.Name = name;
            this.Place = place;            
        }

        public static List<FreeDormitory> GetFreeDormitoryList(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeDormitories.Where(x => x.EventId == eventId).ToList();
        }

        public static List<FreeDormitory> GetFreeDormitoryListByType(int eventId, DormitoryTypeEnum type, DormitoryCategoryEnum catType, 
            bool invalidate = true)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeDormitories.Where(x => x.EventId == eventId && x.Type == type && x.CatType == catType).ToList();
        }
        
        public static int GetIdByProperties(int eventId, string name, DormitoryTypeEnum type, DormitoryCategoryEnum catType, int place)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeDormitory sec = context.FreeDormitories.Where(x => x.EventId == eventId
            && x.Type == type && x.CatType == catType && x.Name == name && x.Place == place).FirstOrDefault();
            return (sec == null) ? 0 : sec.Id;
        }

        public static FreeDormitory GetAFreeDormitoryBedByType(int eventId, DormitoryTypeEnum type, DormitoryCategoryEnum catType, bool invalidate = true)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeDormitory sec = context.FreeDormitories.Where(x => x.EventId == eventId && x.Type == type && x.CatType == catType).FirstOrDefault();
            if (sec != null && invalidate)
            {
                //Mark item as used and update DB
                context.FreeDormitories.Remove(sec);
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
                context.FreeDormitories.Add(this);
            }

            context.SaveChanges();
            return Id;
        }

        public object GetRecordID()
        {
            return GetIdByProperties(EventId, Name, Type, CatType, Place);
        }
    }
}

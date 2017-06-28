using IMEVENT.SharedEnums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class FreeSharingGroup : IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public int GroupNbr { get; set; }
        public SharingGroupCategoryEnum Cat { get; set; }
        public int Table { get; set; }
        public int EventId { get; set; }
        public int RemainingSeats { get; set; }
        public int Capacity { get; set; }

        public FreeSharingGroup()
        {
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
                context.FreeSharingGroups.Add(this);
            }

            context.SaveChanges();
            return Id;
        }

        public object GetRecordID()
        {
            return GetIdByProperties(EventId, Cat, GroupNbr, Capacity);
        }

        public static int GetIdByProperties(int eventId, SharingGroupCategoryEnum cat, int group, int capacity)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            FreeSharingGroup sec = context.FreeSharingGroups.Where(x => x.EventId == eventId
            && x.Cat == cat && x.GroupNbr == group && x.Capacity == capacity).FirstOrDefault();
            return (sec == null) ? 0 : sec.Id;
        }

        public static List<FreeSharingGroup> GetFreeSharingGroupList(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.FreeSharingGroups.Where(x => x.EventId == eventId).ToList();
        }
    }
}

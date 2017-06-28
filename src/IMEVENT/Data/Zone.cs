using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    public class Zone:IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public String Label { get; set; }
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
                context.Zones.Add(this);
            }
            context.SaveChanges();
            return this.Id;
        }

        public static Dictionary<int, string> GetList()
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.Zones.Where(g => g.Id != 0).ToDictionary(x => x.Id, x => x.Label);
        }

        public static int GetIdRefectoryIdByName(ApplicationDbContext context, string label)
        {
            var zone = context.Zones.FirstOrDefault(d => d.Label.Equals(label));
            if (zone != null)
            {
                return zone.Id;
            }
            else return 0;
        }

        public object GetRecordID()
        {
            return Id;
        }
    }
}

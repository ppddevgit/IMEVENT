using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IMEVENT.Data
{
    public class SousZone:IObjectPersister
    {        
        [Key]
        public int Id { get; set; }        
        public string Label { get; set; }
        public int ZoneId { get; set; }

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
                context.SousZones.Add(this);
            }
           
            context.SaveChanges();
            return this.Id;
        }

        public static Dictionary<int, string> GetList()
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.SousZones.Where(g => g.Id != 0).ToDictionary(x => x.Id, x => x.Label);
        }

        public  int GetSousZoneIdByLabel(string label, int zoneId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            var zone = context.SousZones.FirstOrDefault(d => d.Label.Equals(label) && d.ZoneId == zoneId);
            if (zone != null)
            {
                return zone.Id;
            }
            else return 0;
        }

        public object GetRecordID()
        {
            return GetSousZoneIdByLabel(Label, ZoneId);
        }
    }
}

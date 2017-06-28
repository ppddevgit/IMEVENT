using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace IMEVENT.Data
{
    public class Group:IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public String Label { get; set; }
        public int SousZoneId { get; set; }
        public int ZoneId { get; set; }
        public int IdResponsable { get; set; }
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
                context.Groups.Add(this);
            }
           
            context.SaveChanges();
            return this.Id;
        }
        /// <summary>
        /// returns the group associated with the given label or 0 if there is no such lable in the database.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static int GetIdGroupIdByLabel( string label)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            var group = context.Groups.FirstOrDefault(d => d.Label.Equals(label));
            if (group != null)
            {
                return group.Id;
            }
            else return 0;
        }

        public static Group GetGroupById(int Id)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.Groups.FirstOrDefault(d => d.Id == Id);            
        }

        public static Dictionary<int, string> GetList()
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.Groups.Where(g => g.Id != 0).ToDictionary(x => x.Id, x => x.Label);
        }

        public object GetRecordID()
        {
            return GetIdGroupIdByLabel(Label);
        }
    }
}

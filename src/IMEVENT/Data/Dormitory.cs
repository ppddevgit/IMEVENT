using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.SharedEnums;

namespace IMEVENT.Data
{
    public class Dormitory:BaseSection,IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public DormitoryTypeEnum DormType { get; set; }
        public DormitoryCategoryEnum DormCategory { get; set; }

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
                context.Dorms.Add(this);
            }
               
            context.SaveChanges();
            Dormitory dorm = context.Dorms.FirstOrDefault(d => d.Name.Equals(this.Name));
            
            return dorm.Id;                     
        }

        public int GetIdByProperties(string name, DormitoryTypeEnum type, DormitoryCategoryEnum cat)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            Dormitory dorm = context.Dorms.FirstOrDefault(d => d.Name.Equals(name)
                            && d.DormType == type && d.DormCategory == cat);

            if (dorm != null)
            {
                return dorm.Id;
            }

            return 0;
        }

        public static Dictionary<int, Dormitory> GetDormitoryList(int eventID)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.Dorms.Where(x => x.EventId == eventID).ToDictionary(x => x.Id, x => x);
        }

        public object GetRecordID()
        {
            return  GetIdByProperties(Name, DormType, DormCategory); 
        }
    }
}

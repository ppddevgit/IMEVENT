using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.SharedEnums;
using IMEVENT.Services;

namespace IMEVENT.Data
{
    public class EventAttendee:IObjectPersister
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }

        public string Retreats { get; set; }
        public Int32 AmountPaid { get; set; }
        public string Remarks { get; set; }        
        public string UserId { get; set; }     
        public string InvitedBy { get; set; }                    
        public string Precision { get; set; }
        public HallSectionTypeEnum SectionType { get; set; }
        public DormitoryCategoryEnum DormCategory { get; set; }
        public RegimeEnum TableType { get; set; }
        public SharingGroupCategoryEnum SharingCategory { get; set; }
        public int HallId { get; set; }       
        public int SeatNbr { get; set; }
        public int DormitoryId { get; set; }  
        public int BedNbr { get; set; }
        public int RefectoryId { get; set; }
        public int TableId { get; set; }
        public int TableSeatNbr { get; set; }
        public int SharingGroupNbr { get; set; }
        public int SharingTableNbr { get; set; }

        public override string ToString()
        {
            string ret = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}"
                , Retreats
                , InvitedBy
                , SharingCategory.SharingGroupCategoryToString()
                , AmountPaid
                , Remarks                
                , Precision
                , Id
                , SeatNbr
                , Id
                , BedNbr
                , RefectoryId
                , TableId
                , TableSeatNbr
                , SharingGroupNbr
                , SharingTableNbr
                );

            return ret;
        }

        public string ToString(Dictionary<string, User> AttendeeInfo, Dictionary<int, Hall> Halls, Dictionary<int, Dormitory> Dorms
            , Dictionary<int, Refectory> Refectories, Dictionary<int, Table> Tables)
        {
            string invitedBy = (AttendeeInfo.ContainsKey(this.InvitedBy))
                            ? string.Format("{0} {1}", AttendeeInfo[InvitedBy].FirstName, AttendeeInfo[InvitedBy].LastName)
                            : "";

            string ret = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}"
                    , Retreats
                    , invitedBy
                    , SharingCategory.SharingGroupCategoryToString()
                    , AmountPaid
                    , Remarks                
                    , Precision
                    , Halls[HallId].Name
                    , SeatNbr
                    , Dorms[DormitoryId].Name
                    , BedNbr
                    , Refectories[RefectoryId].Name
                    , Tables[TableId].Name
                    , TableSeatNbr
                    , string.Format("{0} {1} / T{2}", SharingCategory.SharingGroupCategoryToString(), SharingGroupNbr, SharingTableNbr)
                );

            return ret;
        }
        
        public static Dictionary<string, EventAttendee> GetAttendeeList(int eventID)
        {            
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.EventAttendees.Where(x => x.EventId == eventID).ToDictionary(x => x.UserId, x => x); 
        }

        public EventAttendee GetEventAttendeeById(int eventId, string userId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.EventAttendees.Where(x => x.EventId == eventId).FirstOrDefault(e => e.UserId == userId);                        
        }

        public static List<EventAttendee> GetAttendeeListByEvent(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.EventAttendees.Where(x => x.EventId == eventId).ToList();
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
                context.EventAttendees.Add(this);
            }

            context.SaveChanges();

            return this.Id;
        }

        public object GetRecordID()
        {
            return Id;
        }
    }
}

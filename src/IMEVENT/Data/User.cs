using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.SharedEnums;

namespace IMEVENT.Data
{
    public class User : IdentityUser
    {
        public DateTime DateofBirth { get; set; }
        public string Sex { get; set; }
        public int Status { get; set; }
        public string Language { get; set; }        
        public string InvitedBy { get; set; }
        public int GroupId { get; set; }
        public int ZoneId { get; set; }
        public int SousZoneId { get; set; }
        public string Town { get; set; }
        public bool IsGroupResponsible { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string GroupOriginLabel { get; set; }
        public string TownOriginLabel { get; set; }
        public MembershipLevelEnum Level { get; set; }

        public string ToString(Dictionary<int, string> groups, Dictionary<int, string> zone, Dictionary<int, string> sousZone)
        {            
            string ret = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}"
                 , LastName
                 , FirstName
                 , Sex
                 , Town
                 , (groups == null || !groups.ContainsKey(GroupId))? "" : groups[GroupId]
                 , Level.MemberShipLevelToString()
                 , Language
                 , Email
                 , PhoneNumber
                 , IsGroupResponsible ? "Oui" : "Non" 
                 , (zone == null || !zone.ContainsKey(ZoneId)) ? "" : zone[ZoneId]
                 , (sousZone == null || !sousZone.ContainsKey(SousZoneId)) ? "" : sousZone[SousZoneId]
                 , TownOriginLabel
                 , GroupOriginLabel
                 );

            return ret;
        }

        public int OriginZone { get; set; }

        /// <summary>
        /// gets the Id of the user associated with the full name passed as parameter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fullName">Fullname in format LASTNAME-FIRSTMANE of the user</param>
        /// <returns></returns>
        public static String GetUserIdByName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return string.Empty;
            }
            fullName = fullName.Trim();
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            User user = context.Users.FirstOrDefault(d => (d.LastName + d.FirstName).Equals(fullName));        
            if (user == null)
            {
                return string.Empty; 
            }

            return user.Id;
        }

        public static Dictionary<string, User> GetRegisteredUsersPerEventId(int eventId)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            var ret = (from Item1 in context.Users
                       join Item2 in context.EventAttendees
                       on Item1.Id equals Item2.UserId
                       select new { Item2, Item1 })
                       .Where(x => x.Item2.EventId == eventId)
                       .ToDictionary(x => x.Item1.Id, x => x.Item1);
            return ret;            
        }

        public string persist()
        {
            //This Persist method behaves differently from the implementation in different classes
            //it is due to the fact that apparently Users are generated directly with their ID at instantiation and not at
            //the time they are saved in the database. 
            
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            string exists = GetUserIdByName(LastName + FirstName);
            if (string.IsNullOrEmpty(exists))
            {
                context.Users.Add(this);
                context.SaveChanges();
                return this.Id;
            }

            //user already exists - TODO : find a way to update user info in DB            
            this.Id = exists;
            return this.Id;
        }
    }
}

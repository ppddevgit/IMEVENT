using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.SharedEnums;
using System.Threading;
using IMEVENT.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace IMEVENT.Data
{
    public class User : IdentityUser
    {
     
        private readonly IUserManager userManager;
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
        public MembershipLevelEnum Level { get; set; }

        public User(IUserManager manager )
        {
            userManager = manager;
           
        }
        public override string ToString()
        {            
            string ret = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}"
                 , LastName
                 , FirstName
                 , Sex
                 , Town
                 , ""//TODO - read group
                 , IsGroupResponsible ? "Oui" : "Non"
                 , Level.MemberShipLevelToString()                 
                 , Language
                 , Email
                 , PhoneNumber
                 );

            return ret;
        }
        public User()
        {

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
            //var store = new UserStore<User>(context);
            //var manager = new UserManager<User>(store, null, null, null, null, null, null, null, null);
       
            string exists = GetUserIdByName(LastName + FirstName);
            if (string.IsNullOrEmpty(exists))
            {
                context.Users.Add(this);
                context.SaveChanges();
                exists =  this.Id;
            }
            else
            {
                
               this.Id = exists;
                // needs to be updated will do that with raw SQL since I am having issues with UserManager.  
                String sql = "update AspNetUsers " +
                             "Set " +
                             "Sex = '" + Sex + "' , " +
                             "Status = '" + Status + "' , " +
                             "Language = '" + Language + "' , " +
                             "InvitedBy = '" + InvitedBy + "' , " +
                             "GroupId = " + GroupId + " , " +
                             "ZoneId = " + ZoneId + " , " +
                             "SousZoneId = " + SousZoneId + " , " +
                             "Town = '" + Town + "' , " +
                             "IsGroupResponsible = " + (IsGroupResponsible ? 1: 0) + " , " +
                             "FirstName = '" + FirstName + "' , " +
                             "LastName = '" + LastName + "' "+
                             "where id = '" + exists + "' ";
                context.Database.ExecuteSqlCommand(sql);
            }
            
            this.Id = exists;
            return this.Id;
        }
    }
}

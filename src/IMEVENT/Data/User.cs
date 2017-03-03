﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.SharedEnums;

namespace IMEVENT.Data
{
    public class User:IdentityUser
    {
        public DateTime DateofBirth { get; set; }
        public string Sex { get; set; }
        public int Status { get; set; }                
        public string Language { get; set; }
        public SharingGroupCategoryEnum Category { get; set; }
      
        public String InvitedBy { get; set; }
        public int GroupId { get; set; }
        public int ZoneId { get; set; }
        public int SousZoneId { get; set; }
        public string Town { get; set; }
        public bool IsGroupResponsible { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public MembershipLevelEnum Level { get; set; }

        public override string ToString()
        {
            string ret = String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}"
                 , LastName
                 , FirstName
                 , Sex
                 , Town
                 , ""//TODO - read group
                 , IsGroupResponsible ? "Oui" : "Non"
                 , Level.MemberShipLevelToString()
                 , Category.SharingGroupCategoryToString()
                 , Language
                 , Email
                 , PhoneNumber
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
            if (String.IsNullOrEmpty(fullName))
            {
                return String.Empty;
            }
            fullName = fullName.Trim();
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            User user = context.Users.FirstOrDefault(d => (d.LastName + d.FirstName).Equals(fullName));        
            if (user == null)
            {
                return String.Empty; 
            }

            return user.Id;
        }

        public static Dictionary<string, User> GetRegisteredUsers(List<string> keys)
        {
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
            return context.Users.Where(x => keys.Contains(x.Id)).ToDictionary(x => x.Id, x => x);
        }

        public string persist()
        {
            //This persist method behaves differently from the implementation in different classes
            //it is due to the fact that apparently Users are generated directly with their ID at instantiation and not at
            //the time they are saved in the database. 
            
            ApplicationDbContext context = ApplicationDbContext.GetDbContext();
             String exists = GetUserIdByName(LastName + FirstName);
            if (String.IsNullOrEmpty(exists))
            {
                context.Users.Add(this);
                context.SaveChanges();
            }
           
           
            return this.Id;
        }
    }
}

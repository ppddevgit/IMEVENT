using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IMEVENT.Data;
using IMEVENT.SharedEnums;
using System.Text;
using NLog;

namespace IMEVENT.Events
{
    public class DataMatchingGenerator
    {
        #region internal data definition

        private static Logger logger = LogManager.GetCurrentClassLogger();
        //Is the data loaded from the DB
        public bool IsAllDataLoaded
        {
            get
            {
                return this.IsAttendeeInfoLoaded;
            }
        }
               
        public Event CurrentEvent { get; set; }

        //Constructor
        public DataMatchingGenerator(Event ev)
        {
            this.CurrentEvent = ev;
            InvalidateAllData();
        }

        //Test Constructor
        public void LoadDataInMatchingGenerator(Dictionary<string, EventAttendee> participants, Dictionary<string, User> participantsInfo, 
            Dictionary<int, Hall> seats, Dictionary<int, Dormitory> beds, Dictionary<int, Refectory> refs,
            Dictionary<int, Table> tables)
        {
            attendees = participants;
            attendeesInfo = participantsInfo;                   
        }                              

        //Input attendees
        private Dictionary<string, EventAttendee> attendees;
        public Dictionary<string, EventAttendee> Attendees
        {
            get
            {
                EnsureLoaded();
                return attendees;
            }
        }

        //Attendees Info
        private Dictionary<string, User> attendeesInfo;
        public Dictionary<string, User> AttendeesInfo
        {
            get
            {
                EnsureLoaded();
                return attendeesInfo;
            }
        }

        public HallMatching seatsInHall;
        public DormMatching bedsInDorm;
        public RefectoryMatching tablesInRefs;
        public SharingGroupMatching sharingGroups;
        #endregion

        #region Data load

        private void EnsureLoaded()
        {
            if (this.IsAllDataLoaded)
            {                
                return; //data already loaded
            }

            //Get list of attendees
            this.attendees = EventAttendee.GetAttendeeList(this.CurrentEvent.Id);
            if (this.attendees == null)
            {
                throw new System.NullReferenceException(string.Format("No Attendee registered yet for the event at {0}, starting on {1}"
                    , this.CurrentEvent.Place
                    , this.CurrentEvent.StartDate.ToString()));
            }
            
            //Get attendees information
            this.attendeesInfo = User.GetRegisteredUsersPerEventId(this.CurrentEvent.Id);
            if (this.attendeesInfo == null)
            {
                throw new System.NullReferenceException(string.Format("Infos not available for registered users for the event at {0}, starting on {1}"
                    , this.CurrentEvent.Place
                    , this.CurrentEvent.StartDate.ToString()));
            }
            this.IsAttendeeInfoLoaded = true;

            this.seatsInHall = new HallMatching(this.CurrentEvent);
            this.bedsInDorm = new DormMatching(this.CurrentEvent);
            this.tablesInRefs = new RefectoryMatching(this.CurrentEvent);
            this.sharingGroups = new SharingGroupMatching(this.CurrentEvent);                       
        }        
        
        private bool isAttendeeInfoLoaded;
        public bool IsAttendeeInfoLoaded
        {
            get
            {
                return isAttendeeInfoLoaded;
            }

            set
            {
                isAttendeeInfoLoaded = value;
            }
        }
        #endregion

        #region Invalidate data
        private void InvalidateAllData()
        {
            this.IsAttendeeInfoLoaded = false;            
        }        
        #endregion         

        public bool GenerateAllItems()
        {
            if (!this.seatsInHall.GenerateItemsForMatching()
                || !this.bedsInDorm.GenerateItemsForMatching()
                || !this.tablesInRefs.GenerateItemsForMatching()
                || !this.sharingGroups.GenerateGroupsForMatching(this.attendees))
            {
                return false;
            }

            return true;
        }

        #region Badge Generation
        public bool GenerateAllBadges()
        {
            EnsureLoaded();
            if (!GenerateAllItems())
            {
                return false;
            }

            // Check that we have enough places (Seats, beds, tables) available for the matching
            int nbrAttendees = this.attendees.Count;
            int nbrSections = this.seatsInHall.CountAvailableResource();
            int nbrDorms = this.bedsInDorm.CountAvailableResource();
            int nbrTables = this.tablesInRefs.CountAvailableResource();
            if (nbrSections < nbrAttendees
               || nbrDorms < nbrAttendees
               || nbrTables < nbrAttendees)
            {
                logger.Log(LogLevel.Error, String.Format("Not enought resources available for registered attendees! " +
                    "Sections: {0}, Dorms: {1}, Tables: {2}", nbrSections, nbrDorms, nbrTables));                
                return false;
            }

            int nbAssignment = 0;
            foreach (KeyValuePair<SharingGroupCategoryEnum, List<GroupSharingEntry>> cat in this.sharingGroups.ListAvailableSharingGroups)
            {
                foreach (GroupSharingEntry gshare in cat.Value)
                {
                    string attendeeKey = gshare.UserId;
                    EventAttendee attendee = this.attendees[attendeeKey];
                    HallEntry aSeat = this.seatsInHall.GetElement(this.CurrentEvent.MingleAttendees ? HallSectionTypeEnum.NONE : attendee.SectionType);
                    if (aSeat == null)
                    {
                        return false;
                    }

                    DormitoryTypeEnum dType = this.attendeesInfo[attendee.UserId].Sex.Trim().ToLower() == "f" 
                                              ? DormitoryTypeEnum.WOMEN
                                              : DormitoryTypeEnum.MEN;

                    DormEntry aBed = this.bedsInDorm.GetElement(this.CurrentEvent.MingleAttendees ? DormitoryTypeEnum.NONE : dType,
                                                  this.CurrentEvent.MingleAttendees ? DormitoryCategoryEnum.BED : attendee.DormCategory);
                    if (aBed == null)
                    {
                        return false;
                    }

                    TableEntry aTable = this.tablesInRefs.GetElement(this.CurrentEvent.MingleAttendees ? RegimeEnum.NONE : attendee.TableType);
                    if (aTable == null)
                    {
                        return false;
                    }

                    this.attendees[attendeeKey].HallId = aSeat.HallId;
                    this.attendees[attendeeKey].SeatNbr = aSeat.SeatNbr;

                    this.attendees[attendeeKey].DormitoryId = aBed.DormitoryId;
                    this.attendees[attendeeKey].BedNbr = aBed.BedNbr;

                    this.attendees[attendeeKey].RefectoryId = aTable.RefectoryId;
                    this.attendees[attendeeKey].TableId = aTable.TableId;
                    this.attendees[attendeeKey].TableSeatNbr = aTable.SeatNbr;
                    
                    this.attendees[attendeeKey].SharingGroupNbr = gshare.Place + 1; //+1 because groups are numbered from 0 on...
                    this.attendees[attendeeKey].SharingTableNbr = gshare.Table;

                    this.attendees[attendeeKey].Persist();//save data in DB
                    nbAssignment++;
                }                                
            }
            
            //Just a check to make sure all attendees have been assigned
            if(nbAssignment != nbrAttendees)
            {
                logger.Log(LogLevel.Error, String.Format(string.Format("Failure to generate badges for all registered participants of the event at {0}, starting on {1}"
                    , this.CurrentEvent.Place
                    , this.CurrentEvent.StartDate.ToString())));

                throw new System.NullReferenceException(string.Format("Failure to generate badges for all registered participants of the event at {0}, starting on {1}"
                    , this.CurrentEvent.Place
                    , this.CurrentEvent.StartDate.ToString()));
            }

            //Save remaining seats in DB
            seatsInHall.SaveRemainingItemsInDB();
            bedsInDorm.SaveRemainingItemsInDB();
            tablesInRefs.SaveRemainingItemsInDB();
            sharingGroups.SaveRemainingItemsInDB();

            return true;
        }

        #endregion

        #region Print helpers
        public List<string> GetStringListOfAssignedAttendees()
        {
            List<string> ret = new List<string>
            {
                //Add file header
                string.Format("Evenement organisé du {1} {2} au {3} {4} dans la ville de {0}"
                         , this.CurrentEvent.Place
                         , this.CurrentEvent.StartDate.DayOfWeek
                         , this.CurrentEvent.StartDate
                         , this.CurrentEvent.EndDate.DayOfWeek
                         , this.CurrentEvent.EndDate)
            };

            ret.Add(String.Format("Theme: \"{0}\"", this.CurrentEvent.Theme));
            ret.Add(",,,,,,,,,,,,,,,,,,,,,,,");
            ret.Add(String.Format(" Prix: {0} Fcfa", this.CurrentEvent.Fee));
            ret.Add(",,,,,,,,,,,,,,,,,,,,,,,");
            string header = "Nom,Prenom,Sexe,Ville,Groupe,Niveau,Langue,Email,Téléphone,Responsable Groupe,"
                            + "Zone,Sous-Zone,Ville Origine,Groupe Origine,Retraite,Invité Par,Categorie,Frais Payés,Remarques,Précision,Section Hall,Nr Siège,Dortoir,"
                            + "Nr Lit,Réfectoire,Nr Table, Nr Siège, Groupe Partage";

            ret.Add(header);

            Dictionary<int, string> groups = Group.GetList();
            Dictionary<int, string> zones = Zone.GetList();
            Dictionary<int, string> sousZones = SousZone.GetList();
            //Add rows            
            foreach (KeyValuePair<string, EventAttendee> entry in this.Attendees)
            {
                string aMatching = string.Format("{0},{1}"
                    , attendeesInfo[entry.Key].ToString(groups, zones, sousZones)
                    , entry.Value.ToString(this.attendeesInfo, this.seatsInHall.Seats, this.bedsInDorm.Beds, this.tablesInRefs.Refectories, this.tablesInRefs.Tables));

                ret.Add(aMatching);
            }

            return ret;
        }

        public List<string> GetStringListOfEmptySections()
        {
            return this.seatsInHall.GetListOfRemainingItems();
        }

        public List<string> GetStringListOfEmptyBeds()
        {
            return this.bedsInDorm.GetListOfRemainingItems();
        }

        public List<string> GetStringListOfEmptyTables()
        {
            return this.tablesInRefs.GetListOfRemainingItems();
        }

        public List<string> GetStringListOfAvailableSharingGroups()
        {
            return this.sharingGroups.GetListOfRemainingItems();
        }

        public void PrintBadgesToFile(string directoryPath, bool forceRecompute, bool printFreeSpots)
        {                       
            if (this.Attendees == null || !this.Attendees.Any() || forceRecompute)
            {
                GenerateAllBadges();
            }

            //string extFile = "Donnees";//Convertors.EventTypeToString(this.CurrentEvent.Type, true);
            List<string> temp = GetStringListOfAssignedAttendees();
            string outputBadgesFile = string.Format("{0}\\Donnees_Badges.csv", directoryPath);
            File.WriteAllLines(outputBadgesFile, temp.ToArray(), Encoding.Unicode);            

            if (!printFreeSpots)
            {
                return;
            }

            temp = GetStringListOfEmptySections();            
            string freePlacesFile = string.Format("{0}\\Liste_Sieges_Hall_Disponibles.csv", directoryPath);
            File.WriteAllLines(freePlacesFile, temp.ToArray(), Encoding.Unicode);

            temp = GetStringListOfEmptyBeds();
            freePlacesFile = string.Format("{0}\\Liste_Lits_Dortoir_Disponibles.csv", directoryPath);
            File.WriteAllLines(freePlacesFile, temp.ToArray(), Encoding.Unicode);

            temp = GetStringListOfEmptyTables();
            freePlacesFile = string.Format("{0}\\Liste_Tables_Refectoire_Disponibles.csv", directoryPath);
            File.WriteAllLines(freePlacesFile, temp.ToArray(), Encoding.Unicode);

            temp = GetStringListOfAvailableSharingGroups();
            freePlacesFile = string.Format("{0}\\Liste_Groupe_Partage.csv", directoryPath);
            File.WriteAllLines(freePlacesFile, temp.ToArray(), Encoding.Unicode);
        }
        #endregion
    }
}

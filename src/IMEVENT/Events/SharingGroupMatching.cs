using IMEVENT.Data;
using IMEVENT.SharedEnums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public class SharingGroupMatching : BaseEventResourceManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public SharingGroupMatching(Event eventI) : base(eventI)
        {
        }

        int tableIndex;

        private Dictionary<SharingGroupCategoryEnum, int> sharingGroups;
        public Dictionary<SharingGroupCategoryEnum, int> SharingGroups
        {
            get
            {
                EnsureLoaded();
                return sharingGroups;
            }
        }

        public Dictionary<SharingGroupCategoryEnum, List<GroupSharingEntry>> ListAvailableSharingGroups;
        Dictionary<SharingGroupCategoryEnum, GroupSharingEntry> NextFreeSpot;

        public override int CountAvailableResource()
        {
            throw new NotImplementedException();
        }

        //Split input attendee in groups (Round robin Algorithm)
        public List<GroupSharingEntry> ShuffleSharingGroupEntries(List<string> inputList, int capacity, SharingGroupCategoryEnum groupCat)
        {
            inputList.Shuffle();
            List<GroupSharingEntry> ret = new List<GroupSharingEntry>();
            int index = 0;
            int groupIndex = 0;
            GroupSharingEntry grp = null;

            foreach (string userId in inputList)
            {                
                if (index == capacity)
                {
                    groupIndex++;
                    this.tableIndex++;
                    index = 0;
                }

                index++;
                grp = new GroupSharingEntry
                {
                    UserId = userId,
                    Place = groupIndex,
                    Table = this.tableIndex
                };

                ret.Add(grp);                
            }

            grp.Capacity = capacity;
            grp.RemainingSeats = capacity - index;//amount of remaining places in the group
            NextFreeSpot[groupCat] = grp;//save last group per sharing category                 
            this.tableIndex++;//move next group to the next table
            return ret;
        }

        public override bool GenerateItemsForMatching()
        {
            return false;
        }

        public bool GenerateGroupsForMatching(Dictionary<string, EventAttendee> attendeesList)
        {
            List<GroupSharingEntry> tempStack;
            this.ListAvailableSharingGroups = new Dictionary<SharingGroupCategoryEnum, List<GroupSharingEntry>>();
            if (this.Event.MingleAttendees)
            {
                int capacity = this.SharingGroups[SharingGroupCategoryEnum.ADULTE_SINGLE];
                tempStack = ShuffleSharingGroupEntries(attendeesList.Keys.ToList(), capacity, SharingGroupCategoryEnum.ADULTE_SINGLE);
                if (tempStack == null)
                {
                    log.Error("GenerateGroupsForMatching: Error in generating sharing groups per category with participants mingle");
                    return false;
                }

                this.ListAvailableSharingGroups[SharingGroupCategoryEnum.ADULTE_SINGLE] = tempStack;
                return true;
            }

            //Get attendees keys and shuffle
            List<string> attendeeKeys = new List<string>(attendeesList.Keys);
            attendeeKeys.Shuffle();

            Dictionary<SharingGroupCategoryEnum, List<string>> tempDict = new Dictionary<SharingGroupCategoryEnum, List<string>>();
            
            foreach (string attendee in attendeeKeys)
            {
                //ADULTE_S | ADULTE_M | JEUNE_MARIE are grouped together
                SharingGroupCategoryEnum sGroup = ((attendeesList[attendee].SharingCategory == SharingGroupCategoryEnum.JEUNE_MARIE)
                                                    || (attendeesList[attendee].SharingCategory == SharingGroupCategoryEnum.ADULTE_MARIE)
                                                    || ((attendeesList[attendee].SharingCategory == SharingGroupCategoryEnum.ADULTE_SINGLE)))
                                                    ? SharingGroupCategoryEnum.ADULTE_SINGLE : attendeesList[attendee].SharingCategory;

                if (!tempDict.ContainsKey(sGroup))
                {
                    tempDict[sGroup] = new List<string> { attendeesList[attendee].UserId };
                }
                else
                {
                    tempDict[sGroup].Add(attendeesList[attendee].UserId);
                }
            }

            // regroup per category
            this.tableIndex = 1;
            NextFreeSpot = new Dictionary<SharingGroupCategoryEnum, GroupSharingEntry>();

            foreach (KeyValuePair<SharingGroupCategoryEnum, List<string>> cat in tempDict)
            {                                
                tempStack = ShuffleSharingGroupEntries(cat.Value, this.SharingGroups[cat.Key], cat.Key);                
                if (tempStack == null)
                {
                    log.Error("GenerateGroupsForMatching: Error in generating sharing groups per category with participants not mingle");
                    return false;
                }
                this.ListAvailableSharingGroups[cat.Key] = tempStack;
            }

            return true;
        }

        public override List<string> GetListOfRemainingItems()
        {
            List<string> ret = new List<string>();
            List<FreeSharingGroup> items = FreeSharingGroup.GetFreeSharingGroupList(this.Event.Id);

            if (items == null || items.Count == 0)
            {
                ret.Add("Pas de groupe de Partage disponible!");
                return ret;
            }

            ret.Add("Categorie,Groupe Dispo.,Table,Capacite,Places Restantes");
            foreach (FreeSharingGroup elem in items)
            {
                ret.Add(string.Format("{0},{1},T{2},{3},{4}"
                                , Convertors.SharingGroupCategoryToString(elem.Cat)
                                , elem.GroupNbr + 1//since groups are numbered from 0 on...
                                , elem.Table
                                , elem.Capacity
                                , elem.RemainingSeats));
            }

            return ret;
        }

        public override void SaveRemainingItemsInDB()
        {
            if (this.NextFreeSpot.Count() == 0)
            {
                //nothing to save
                return;
            }

            foreach (KeyValuePair<SharingGroupCategoryEnum, GroupSharingEntry> spot in this.NextFreeSpot)
            {
                FreeSharingGroup group = new FreeSharingGroup
                {
                    EventId = this.Event.Id,
                    Cat = spot.Key,
                    GroupNbr = spot.Value.Place,
                    Table = spot.Value.Table,
                    Capacity = spot.Value.Capacity,
                    RemainingSeats = spot.Value.RemainingSeats
                };
                 
                group.Persist();
            }
        }

        protected override void EnsureLoaded()
        {
            if (this.IsDataLoaded)
            {
                //Data already loaded
                return;
            }

            this.sharingGroups = SharingGroup.GetSharingGroups(this.Event.Id);
            if (this.sharingGroups == null)
            {
                log.Error(string.Format("Sharing groups not configured for event {0}"
                    , this.Event.Id));
                throw new System.NullReferenceException(string.Format("Sharing groups not configured for event {0}"
                    , this.Event.Id));
            }
            this.IsDataLoaded = true;
        }
    }
}

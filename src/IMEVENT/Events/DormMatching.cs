using IMEVENT.Data;
using IMEVENT.SharedEnums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public class DormMatching : BaseEventResourceManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Dictionary<int, Dormitory> bedsInDorms;
        public Dictionary<int, Dormitory> Beds
        {
            get
            {
                EnsureLoaded();
                return bedsInDorms;
            }
        }

        Dictionary<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Stack<DormEntry>>> ListAvailableDorms;

        public DormMatching(Event eventI) : base(eventI)
        {
        }

        public override int CountAvailableResource()
        {
            return this.ListAvailableDorms.Count();
        }

        public DormEntry GetElement(DormitoryTypeEnum dormType, DormitoryCategoryEnum dormCat)
        {
            if (this.ListAvailableDorms.IsNullOrEmpty()
                || !this.ListAvailableDorms.ContainsKey(dormType)
                || this.ListAvailableDorms[dormType].IsNullOrEmpty()
                || !this.ListAvailableDorms[dormType].ContainsKey(dormCat)
                || this.ListAvailableDorms[dormType][dormCat].IsNullOrEmpty()
               )
            {
                log.Error("GetDormEntry(): No seat available!");               
                return null;
            }

            return this.ListAvailableDorms[dormType][dormCat].Pop();
        }

        public Stack<DormEntry> ShuffleBedEntries(Dictionary<int, Dormitory> inputBedList)
        {
            Dictionary<int, Section> listofBeds = new Dictionary<int, Section>();
            try
            {
                //build the list of beds Indexes
                int index = 0;
                foreach (KeyValuePair<int, Dormitory> dorm in inputBedList)
                {
                    for (int j = 1; j <= dorm.Value.Capacity; j++)
                    {
                        index++;
                        listofBeds[index] = new Section
                        {
                            Id = dorm.Value.Id,
                            PlaceNbr = j
                        };
                    }
                }

                //Shuffle Indexes
                List<int> bedsIds = new List<int>(listofBeds.Keys);
                bedsIds.Shuffle();

                //Perform the assignement
                Stack<DormEntry> outputBedList = new Stack<DormEntry>();
                foreach (int bedId in bedsIds)
                {
                    outputBedList.Push(new DormEntry
                    {
                        DormitoryId = listofBeds[bedId].Id,
                        BedNbr = listofBeds[bedId].PlaceNbr
                    });
                }

                return outputBedList;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception in generating Beds per category");                
                return null;
            }
        }

        public override bool GenerateItemsForMatching()
        {            
            //if mingle true, no need to break per category            
            Stack<DormEntry> tempStack;
            this.ListAvailableDorms = new Dictionary<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Stack<DormEntry>>>();
            if (this.Event.MingleAttendees)
            {
                tempStack = ShuffleBedEntries(this.Beds);
                if (tempStack == null)
                {
                    log.Error("GenerateItemsForMatching(): Error in generating beds per category with participants mingle");                    
                    return false;
                }

                //when we mingle, we put all attendees on Beds
                this.ListAvailableDorms[DormitoryTypeEnum.NONE][DormitoryCategoryEnum.BED] = tempStack;
                return true;
            }

            //Break the list per category and type if mingle = false
            Dictionary<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Dictionary<int, Dormitory>>> tempDict
                = new Dictionary<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Dictionary<int, Dormitory>>>();

            foreach (KeyValuePair<int, Dormitory> bed in this.Beds)
            {
                if (!tempDict.ContainsKey(bed.Value.DormType))
                {
                    tempDict[bed.Value.DormType] = new Dictionary<DormitoryCategoryEnum, Dictionary<int, Dormitory>>();
                }

                if (!tempDict[bed.Value.DormType].ContainsKey(bed.Value.DormCategory))
                {
                    tempDict[bed.Value.DormType][bed.Value.DormCategory] = new Dictionary<int, Dormitory>
                {
                    { bed.Value.Id, bed.Value }
                };
                    continue;
                }

                tempDict[bed.Value.DormType][bed.Value.DormCategory][bed.Value.Id] = bed.Value;
            }

            // Match per category
            foreach (KeyValuePair<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Dictionary<int, Dormitory>>> type in tempDict)
            {
                this.ListAvailableDorms[type.Key] = new Dictionary<DormitoryCategoryEnum, Stack<DormEntry>>();
                foreach (KeyValuePair<DormitoryCategoryEnum, Dictionary<int, Dormitory>> cat in type.Value)
                {
                    tempStack = ShuffleBedEntries(cat.Value);
                    if (tempStack == null)
                    {
                        log.Error("GenerateItemsForMatching(): Error in generating beds per category with participants not mingle");                        
                        return false;
                    }
                    this.ListAvailableDorms[type.Key][cat.Key] = tempStack;
                }
            }

            return true;
        }

        protected override void EnsureLoaded()
        {
            if (this.IsDataLoaded)
            {
                //Data already loaded
                return;
            }

            //Get list of Dorms/Beds from DB
            this.bedsInDorms = Dormitory.GetDormitoryList(this.Event.Id);
            if (this.bedsInDorms == null)
            {
                log.Error(string.Format("Dorms not configured for event {0}"
                    , this.Event.Id));
                throw new System.NullReferenceException(string.Format("Dorms not configured for event {0}"
                    , this.Event.Id));                
            }

            this.IsDataLoaded = true;
        }

        public override List<string> GetListOfRemainingItems()
        {
            List<string> ret = new List<string>();
            List<FreeDormitory> items = FreeDormitory.GetFreeDormitoryList(this.Event.Id);

            if (items == null || items.Count == 0)
            {
                ret.Add("Plus de lit disponible dans les dortoirs!");
                return ret;
            }

            ret.Add("Type Dortoir,Catégorie,Nom Dortoir,Nr Lit");
            foreach (FreeDormitory elem in items)
            {
                ret.Add(string.Format("{0},{1},{2},{3}"
                                , Convertors.DormitoryTypeToString(elem.Type)
                                , Convertors.DormitoryCategoryToString(elem.CatType)
                                , elem.Name
                                , elem.Place));
            }
            
            return ret;
        }

        public override void SaveRemainingItemsInDB()
        {
            if (this.ListAvailableDorms.Count() == 0)
            {
                //nothing to save
                return;
            }                        

            foreach (KeyValuePair<DormitoryTypeEnum, Dictionary<DormitoryCategoryEnum, Stack<DormEntry>>> elem1 in this.ListAvailableDorms)
            {
                foreach (KeyValuePair<DormitoryCategoryEnum, Stack<DormEntry>> elem2 in elem1.Value)
                {
                    foreach (DormEntry elem3 in elem2.Value)
                    {
                        FreeDormitory dorm = new FreeDormitory(this.Event.Id, elem1.Key, elem2.Key, 
                            this.Beds[elem3.DormitoryId].Name, elem3.BedNbr);

                        dorm.Persist();
                    }
                }
            }
        }
    }
}

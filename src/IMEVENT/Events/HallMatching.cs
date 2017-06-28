using IMEVENT.Data;
using IMEVENT.SharedEnums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public class HallMatching : BaseEventResourceManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        private Dictionary<int, Hall> seats;
        public Dictionary<int, Hall> Seats
        {
            get
            {
                EnsureLoaded();            
                return seats;
            }
        }

        //Get available places in defined sections
        Dictionary<HallSectionTypeEnum, Stack<HallEntry>> ListAvailableSections = null;

        public HallMatching(Event eventI): base(eventI)
        {           
          
        }
        public override int CountAvailableResource()
        {
            return this.ListAvailableSections.Count();
        }


        public HallEntry GetElement(HallSectionTypeEnum sectionType)
        {
            if (this.ListAvailableSections.IsNullOrEmpty()
               || !this.ListAvailableSections.ContainsKey(sectionType)
               || this.ListAvailableSections[sectionType].IsNullOrEmpty())
            {
                log.Error("GetHallEntry(): No seat available!");
                return null;
            }

            return this.ListAvailableSections[sectionType].Pop();
        }        

        public List<Section> BalanceShuffle(Dictionary<int, Stack<Section>> inputList)
        {
            if (inputList == null || !inputList.Any())
            {
                return null;
            }

            List<Section> ret = new List<Section>();
            int index = 0;
            int nbrElem = inputList.Count();//total seats in the list
            int count = 0;
            while (count < nbrElem)
            {
                index = index % inputList.Count;
                if (inputList[index].IsNullOrEmpty())
                {
                    index++;
                    continue;
                }

                Section tmp = inputList[index++].Pop();
                ret.Add(tmp);
                count++;
            }
            return null;
        }

        public Stack<HallEntry> ShuffleSectionEntries(Dictionary<int, Hall> inputSeatList)
        {
            Dictionary<int, Section> listofSeats = new Dictionary<int, Section>();
            try
            {
                int index = 0;
                foreach (KeyValuePair<int, Hall> seat in inputSeatList)
                {
                    for (int j = 1; j <= seat.Value.Capacity; j++)
                    {
                        index++;
                        listofSeats[index] = new Section
                        {
                            Id = seat.Value.Id,
                            PlaceNbr = j
                        };
                    }
                }

                //Shuffle Ids
                List<int> seatsIds = new List<int>(listofSeats.Keys);
                seatsIds.Shuffle();

                //do the assignement
                Stack<HallEntry> outputSeatList = new Stack<HallEntry>();
                foreach (int seatID in seatsIds)
                {
                    HallEntry tmp = new HallEntry
                    {
                        HallId = listofSeats[seatID].Id,
                        SeatNbr = listofSeats[seatID].PlaceNbr
                    };

                    outputSeatList.Push(tmp);
                }

                return outputSeatList;
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception in generating Seats per category");                
                return null;
            }
        }

        protected override void EnsureLoaded()
        {            
            if (this.IsDataLoaded)
            {
                //Data already loaded
                return;
            }

            //Get list of seats from DB
            this.seats = Hall.GetHallSections(this.Event.Id);
            if (this.seats == null)
            {
                log.Error(string.Format("Hall Section not configured for event {0}"
                    , this.Event.Id));
                throw new System.NullReferenceException(string.Format("Hall Section not configured for event {0}"
                    , this.Event.Id));
            }
            this.IsDataLoaded = true;
        }

        public override bool GenerateItemsForMatching()
        {
            //if mingle true, no need to break per category
            Stack<HallEntry> tempStack;
            this.ListAvailableSections = new Dictionary<HallSectionTypeEnum, Stack<HallEntry>>();
            if (this.Event.MingleAttendees)
            {
                tempStack = ShuffleSectionEntries(this.Seats);
                if (tempStack == null)
                {
                    log.Error("GenerateItemsForMatching(): Error in generating hall seats per category with participants mingle");                    
                    return false;
                }

                this.ListAvailableSections[HallSectionTypeEnum.NONE] = tempStack;
                return true;
            }

            //Break the list per category if mingle false
            Dictionary<HallSectionTypeEnum, Dictionary<int, Hall>> tempDict = new Dictionary<HallSectionTypeEnum, Dictionary<int, Hall>>();
            foreach (KeyValuePair<int, Hall> section in this.Seats)
            {
                if (!tempDict.ContainsKey(section.Value.HallType))
                {
                    tempDict[section.Value.HallType] = new Dictionary<int, Hall>
                    {
                        { section.Value.Id, section.Value }
                    };
                }
                else
                {
                    tempDict[section.Value.HallType][section.Value.Id] = section.Value;
                }
            }


            // Match per category
            foreach (KeyValuePair<HallSectionTypeEnum, Dictionary<int, Hall>> cat in tempDict)
            {
                tempStack = ShuffleSectionEntries(cat.Value);
                if (tempStack == null)
                {
                    log.Error("GenerateItemsForMatching(): Error in generating hall seats per category with participants not mingle");                    
                    return false;
                }
                this.ListAvailableSections[cat.Key] = tempStack;
            }

            return true;
        }

        public override List<string> GetListOfRemainingItems()
        {
            List<string> ret = new List<string>();
            List<FreeHallSection> items = FreeHallSection.GetFreeSectionList(this.Event.Id);

            if (items == null || items.Count == 0)
            {
                ret.Add("Plus de place disponible dans le Hall!");
                return ret;
            }

            ret.Add("Type Section,Nom Section,Nr Siège");
            foreach (FreeHallSection elem in items)
            {
                ret.Add(string.Format("{0},{1},{2}"
                            , Convertors.HallSectionTypeToString(elem.Type)
                            , elem.Name
                            , elem.Place));
            }

            return ret;
        }

        public override void SaveRemainingItemsInDB()
        {            
            if (this.ListAvailableSections.Count() == 0)
            {
                //nothing to save
                return;
            }
           
            foreach (KeyValuePair<HallSectionTypeEnum, Stack<HallEntry>> elem in this.ListAvailableSections)
            {
                foreach (HallEntry hall in elem.Value)
                {
                    FreeHallSection sec = new FreeHallSection(this.Event.Id, elem.Key, this.Seats[hall.HallId].Name, hall.SeatNbr);
                    sec.Persist();                    
                }
            }            
        }
    }
}

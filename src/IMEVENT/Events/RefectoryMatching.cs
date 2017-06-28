using IMEVENT.Data;
using IMEVENT.SharedEnums;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public class RefectoryMatching : BaseEventResourceManager
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        public RefectoryMatching(Event eventI) : base(eventI)
        {
        }

        private Dictionary<int, Refectory> refectories;
        public Dictionary<int, Refectory> Refectories
        {
            get
            {
                EnsureLoaded();
                return refectories;
            }
        }

        private Dictionary<int, Table> tables;
        public Dictionary<int, Table> Tables
        {
            get
            {
                EnsureLoaded();
                return tables;
            }
        }

        Dictionary<RegimeEnum, Stack<TableEntry>> ListAvailableTables;

        public override int CountAvailableResource()
        {
            return this.ListAvailableTables.Count();
        }

        protected override void EnsureLoaded()
        {
            if (this.IsDataLoaded)
            {
                //Data already loaded
                return;
            }

            //Get list of Refectories from DB
            this.refectories = Refectory.GetRefectoryList(this.Event.Id);
            if (this.refectories == null)
            {
                log.Error(string.Format("Refectories not configured for event {0}"
                    , this.Event.Id));
                throw new System.NullReferenceException(string.Format("Refectories not configured for event {0}"
                    , this.Event.Id));
            }

            //Get list of tables from DB
            this.tables = Table.GetTableList(this.Event.Id);
            if (this.tables == null)
            {
                log.Error(string.Format("Tables not configured for event {0}"
                    , this.Event.Id));
                throw new System.NullReferenceException(string.Format("Tables not configured for event {0}"
                    , this.Event.Id));
            }
            this.IsDataLoaded = true;
        }

        public TableEntry GetElement(RegimeEnum refType)
        {
            if (this.ListAvailableTables.IsNullOrEmpty()
                || !this.ListAvailableTables.ContainsKey(refType)
                || this.ListAvailableTables[refType].IsNullOrEmpty())
            {
                Console.WriteLine("GetTableEntry(): No table available!");
                return null;
            }

            return this.ListAvailableTables[refType].Pop();
        }

        public Stack<TableEntry> ShuffleTableEntries(Dictionary<int, Table> inputTableList)
        {
            Dictionary<int, Section> listofTableSeats = new Dictionary<int, Section>();
            try
            {
                int index = 0;
                foreach (KeyValuePair<int, Table> table in inputTableList)
                {
                    for (int i = 1; i <= table.Value.Capacity; i++)
                    {
                        listofTableSeats[++index] = new Section
                        {
                            Id = table.Value.Id,
                            IdRef = table.Value.RefectoryId,
                            PlaceNbr = i
                        };
                    }
                }

                //Shuffle Ids
                List<int> tablesIds = new List<int>(listofTableSeats.Keys);
                tablesIds.Shuffle();

                //do the assignement
                Stack<TableEntry> outputTableList = new Stack<TableEntry>();
                foreach (int refID in tablesIds)
                {
                    outputTableList.Push(new TableEntry
                    {
                        RefectoryId = listofTableSeats[refID].IdRef,
                        TableId = listofTableSeats[refID].Id,
                        SeatNbr = listofTableSeats[refID].PlaceNbr
                    });
                }

                return outputTableList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in generating Tables per category" + ex);
                return null;
            }
        }

        public override bool GenerateItemsForMatching()
        {
            //if mingle true, no need to break per category            
            Stack<TableEntry> tempStack;
            this.ListAvailableTables = new Dictionary<RegimeEnum, Stack<TableEntry>>();
            if (this.Event.MingleAttendees)
            {
                tempStack = ShuffleTableEntries(this.Tables);
                if (tempStack == null)
                {
                    log.Error("Error in generating tables per category with participants mingle");
                    Console.WriteLine("Error in generating tables per category with participants mingle");
                    return false;
                }

                this.ListAvailableTables[RegimeEnum.NONE] = tempStack;
                return true;
            }

            //Break the list per category if mingle false
            Dictionary<RegimeEnum, Dictionary<int, Dictionary<int, Table>>> tempDict = new Dictionary<RegimeEnum, Dictionary<int, Dictionary<int, Table>>>();
            foreach (KeyValuePair<int, Table> table in this.Tables)
            {
                if (!tempDict.ContainsKey(table.Value.RegimeType))
                {
                    tempDict[table.Value.RegimeType] = new Dictionary<int, Dictionary<int, Table>>();
                    tempDict[table.Value.RegimeType][table.Value.RefectoryId] = new Dictionary<int, Table>();
                    tempDict[table.Value.RegimeType][table.Value.RefectoryId][table.Value.Id] = table.Value;
                }
                else
                {
                    if (!tempDict[table.Value.RegimeType].ContainsKey(table.Value.RefectoryId))
                    {
                        tempDict[table.Value.RegimeType][table.Value.RefectoryId] = new Dictionary<int, Table>();
                        tempDict[table.Value.RegimeType][table.Value.RefectoryId][table.Value.Id] = table.Value;
                    }
                    else
                    {
                        tempDict[table.Value.RegimeType][table.Value.RefectoryId][table.Value.Id] = table.Value;
                    }
                }
            }

            // Match per category
            foreach (KeyValuePair<RegimeEnum, Dictionary<int, Dictionary<int, Table>>> cat in tempDict)
            {
                foreach (KeyValuePair<int, Dictionary<int, Table>> refect in cat.Value)
                {
                    tempStack = ShuffleTableEntries(refect.Value);
                    if (tempStack == null)
                    {
                        Console.WriteLine("Error in generating tables per category with participants not mingle");
                        return false;
                    }
                    if (!this.ListAvailableTables.ContainsKey(cat.Key))
                    {
                        this.ListAvailableTables[cat.Key] = new Stack<TableEntry>(tempStack);
                        continue;
                    }

                    this.ListAvailableTables[cat.Key] = this.ListAvailableTables[cat.Key].PushStack(tempStack);
                }
            }

            return true;
        }

        public override void SaveRemainingItemsInDB()
        {
            if (this.ListAvailableTables.Count() == 0)
            {
                //nothing to save
                return;
            }

            foreach (KeyValuePair<RegimeEnum, Stack<TableEntry>> elem in this.ListAvailableTables)
            {
                foreach (TableEntry table in elem.Value)
                {
                    FreeRefectory refec = new FreeRefectory(this.Event.Id, this.refectories[table.RefectoryId].Name,
                        elem.Key, this.tables[table.TableId].Name, table.SeatNbr);
                    refec.Persist();
                }
            }
        }

        public override List<string> GetListOfRemainingItems()
        {
            List<string> ret = new List<string>();
            List<FreeRefectory> items = FreeRefectory.GetFreeRefectoryList(this.Event.Id);

            if (items == null || items.Count == 0)
            {
                ret.Add("Plus de place disponible dans les Réfectoires!");
                return ret;
            }

            ret.Add("Type Table,Réfectoire,Table,Nr Siège");
            foreach (FreeRefectory elem in items)
            {                
                ret.Add(string.Format("{0},{1},{2},{3}"
                        , Convertors.RegimeToString(elem.Type)
                        , elem.Name
                        , elem.Table
                        , elem.Place));             
            }

            return ret;
        }
    }
}

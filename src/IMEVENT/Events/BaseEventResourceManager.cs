using IMEVENT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Events
{
    public abstract class BaseEventResourceManager
    {

        public BaseEventResourceManager(Event eventI)
        {            
            this.Event = eventI;
            this.IsDataLoaded = false;
        }

        //Are resource data already loaded?
        protected bool IsDataLoaded { get; set; }

        //Id of the current event
        public Event Event { get; set; }

        //Load data from DB
        protected abstract void EnsureLoaded();

        //generate resource list that will be used for matching
        public abstract bool GenerateItemsForMatching();

        //Invalidate data loaded
        public virtual void InvalidateData()
        {
            this.IsDataLoaded = false;
        }

        public abstract int CountAvailableResource();

        public abstract List<string> GetListOfRemainingItems();

        public abstract void SaveRemainingItemsInDB();
        //Get one element of this resource type
        //public abstract T GetElement<T,U>(U elem);
    }
}

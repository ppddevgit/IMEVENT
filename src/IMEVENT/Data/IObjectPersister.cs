using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMEVENT.Data
{
    interface IObjectPersister
    {
         int Persist();
        /*
         * this method will return the id of a record if it already exists base on unicity criteria defined
         */
         object GetRecordID();
       
    }
}

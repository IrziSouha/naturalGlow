using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naturalGlow.DAO
{
    public static class Factory
    {
        public static IDAO CreateDBInstance(string jsonFilePath, string projectName)
        {
            return new IDAOFirebaseImpl(jsonFilePath, projectName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdlingMiner
{
    class P_Cheker
    {
        public static bool isThereAProccess()
        {
            Process[] listProc = Process.GetProcesses();
            foreach (var p in listProc)
            {
                if (p.ProcessName == PN)
                {
                    return true;
                }
            }
            return false;
        }
        public static string PN = "none";
    }
}

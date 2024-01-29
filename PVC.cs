using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Photometrics.Pvcam;

namespace BPCsharp
{
    public class PVC
    {
        public PVC()
        {
            bool init = PVCAM.pl_pvcam_init();
            if (!init)
            {
                Console.WriteLine("Could not initialise PVCAM, exiting.");
                Console.ReadKey();
                return;
            }
        }

        ~PVC()
        {
            bool cam_close = PVCAM.pl_cam_close(01); //lol gotta put the right cam in here
            if (!cam_close)
            {
                Console.WriteLine("Could not properly close camera.");
            }
        }
    }
}

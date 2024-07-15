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
            ushort pvcam_ver;
            if (!PVCAM.pl_pvcam_get_ver(out pvcam_ver))
            {
                Console.WriteLine("Could not establish PVCAM version, are you sure it is installed?");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Detected PVCAM version: {0}.", pvcam_ver);

            Console.WriteLine("Initialising PVCAM...");
            // bool init;
            if (!PVCAM.pl_pvcam_init())
            {
                Console.WriteLine("Could not initialise PVCAM, exiting.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Successfully initialised PVCAM.");

            short totl_cams;
            if (!PVCAM.pl_cam_get_total(out totl_cams)) // out means ur passing the pointer in here
            {
                Console.WriteLine("Could not get number of cameras, exiting.");
                Console.ReadKey();
                return;
            }



        }

        ~PVC()
        {
            bool cam_close = PVCAM.pl_cam_close(01); // lol gotta put the right cam in here
            if (!cam_close)
            {
                Console.WriteLine("Could not properly close camera.");
            }
        }
    }
}

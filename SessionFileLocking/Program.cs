using System;
using System.IO;
using System.Threading;
using WinSCP;

// If you comment out the next line and just use regular System.IO.File everything works
//using File = Alphaleonis.Win32.Filesystem.File;

namespace SessionFileLocking
{
    // Assuming all of the following:
    // Your environment has a file server available (mine is on a VM).
    // WinSCP.ini is sufficient to log you in to that file server when you run winscp.exe. Either you saved the password,
    // you pointed to the right ssh key, or whatever else works.
    // There is a file on your server available at /opt-11/foo.txt. In my environment this is a big 1 GB garbage file.

    class Program
    {
        static void Main(string[] args)
        {
            var t = new Thread(DoFileIo);

            t.Start();

            new Thread(DoSftp).Start();

            Thread.Sleep(5000);

            t.Join();

            DoFileIo();

            Console.ReadKey();
        }

        private static void DoSftp()
        {
            using (var session = new Session())
            {
                Console.WriteLine("SFTP Session created");

                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = "FileServer",
                    GiveUpSecurityAndAcceptAnySshHostKey = true
                };

                session.DefaultConfiguration = false; // needed for WinSCP.ini to get picked up apparently
                session.IniFilePath = "./WinSCP.ini";

                session.Open(sessionOptions);

                Console.WriteLine("SFTP Session is opened!");

                session.GetFiles("/opt-11/foo.txt", "./FileFromSftp.txt");

                Console.WriteLine("Got files");
            }
        }

        private static void DoFileIo()
        {
            Console.WriteLine("Starting file IO");

            try
            {
                using (Alphaleonis.Win32.Filesystem.File.Open( // Works with regular old File though : \ ...
                    "./FileForFileIo_NotSFTP.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't open the file. Another executable (WinSCP, I'm looking at you) has locked it");
                Console.WriteLine(ex.Message);
            }


            Console.WriteLine("Ending file IO");
        }
    }
}

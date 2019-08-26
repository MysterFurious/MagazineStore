using MagazineStore.Helpers;
using System;

namespace MagazineStore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Beginning challenge...");

            try
            {                
                string results = new ApiHelper().RunChallenge();
                Console.WriteLine("Challenge completed successfully!");
                Console.WriteLine(String.Format("Results: {0}", results));
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpeted error has occurred...");
                Console.WriteLine(GetInnermostException(ex).Message);
            }

            Console.WriteLine("Execution complete");
            Console.WriteLine("Press the <Enter> key to exit");
            Console.Read();
        }

        static Exception GetInnermostException(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex;
        }
    }
}

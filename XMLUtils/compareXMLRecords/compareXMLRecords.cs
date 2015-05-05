using System;
using System.Text;
using System.IO;

namespace Commandline.compareXMLRecords
{

    /// <summary>
    /// This is static class with a static main method to provide a commandline interface for the XML Compare class.
    /// </summary>
    /// <remarks>
    /// <para>This function compares XML files in 2 directories.</para>
    /// <para>It identifies the files to by comparing part of the filename (parameter 1) to the files in each of the directories (parm 2 and 3) and taking the most recent file.</para>
    /// <para></para>
    /// <para>
    /// It evaluates the commandline and translates the parameters to arguments voor the constructor of XML Compare</para>
    /// <para>Parameters must be the following formats:</para>
    /// <para>Provide 5 or more parameters:</para>
    /// <list type="number">
    /// <item>Identifying part of XML File</item>
    /// <item>directory with xml file to compare</item>
    /// <item>the other directory with the XML file to compare</item>
    /// <item>the container tag which identifies the XML subtree to be compared</item>
    /// <item>1 or more identifying tag(s)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <para><strong>Example for a call to the executable:</strong></para>
    /// <code>compareXMLRecords HYPONP_1_7.1_B_20121231_M P:\ILH\DI\OUT\MONTH C:\TEMP\SVN\BASELINE\MONTH hyp:Onderpand hyp:OnderpandId</code>
    /// </example>
    class compareXMLRecords
    {
		/// <summary>
		/// Returns a helptext that explains the purpose and parameters of this executable
		/// </summary>
		/// <returns></returns>
		private static string getHelpText() {
            StringBuilder msg = new StringBuilder("This function compares XML files in 2 directories.\r\n");
            msg.Append("It identifies the files to by comparing part of the filename (parameter 1) to the files in each of the directories (parm 2 and 3)\r\n");
            msg.Append("and taking the most recent file.\r\n");
            msg.Append("Provide 5 or more parameters:\r\n");
			msg.Append("1. Identifying part of XML File\r\n");
            msg.Append("2. directory with xml file to compare\r\n");
			msg.Append("3. the other directory with the XML file to compare");
            msg.Append("4. the container tag which identifies the XML subtree to be compared");
            msg.Append("5. 1 or more identifying tag\r\n\r\n");
			return msg.ToString();
		}
        /// <include file='..\XMLRecords\generic_docs.xml' path='//XMLRecords/Main/summary'/>
        /// <param name="args"> Commandline parameters. If a parameter contains spaces, enclose them in "</param>
	   public static void Main(string[] args)
	   {
		   // The Length property is used to obtain the length of the array. 
		   // Notice that Length is a read-only property:
		   // Console.WriteLine("Number of command line parameters = {0}",
			  // args.Length);
			if(args.Length <  5)
			{	
				throw new System.ArgumentException(getHelpText());
			}
			
			string containerTag = "";
			string[] identifyingTags;
			string filePrefix = "";
			string[] file = new string[2];
			bool[] file_exists = new bool[2];
			bool[] dir_exists = new bool[2];
			StringBuilder filepattern = new StringBuilder("");

			
			filepattern.Clear();
			filepattern.AppendFormat("{0}*.xml", args[0]);

			for(int i=0;i<2;i++){
				dir_exists[i] = Directory.Exists(args[i+1]);
				if(!dir_exists[i]) {
					StringBuilder msg = new StringBuilder("");
					msg.AppendFormat("Directory {0} does not exist!\r\n{1}", args[i+1], getHelpText());
					Console.Write(msg.ToString());
					Environment.Exit(1);
				}
					
				string[] files = Directory.GetFiles(args[i+1], filepattern.ToString());
				if(files.Length <= 0) {
					StringBuilder msg = new StringBuilder("");
					msg.AppendFormat("No file for pattern {0} found in directory {1} !\r\n{2}", filepattern.ToString(), args[i+1], getHelpText());
					Console.Write(msg.ToString());
					Environment.Exit(1);
						
				}
					
				if(files.Length > 1) {
					string recentFile = files[0];
					//pak het meest recente bestand
					for(int t=0;t<files.Length;t++) {
						FileInfo f = new FileInfo(recentFile);
						FileInfo f1 = new FileInfo(files[t]);
						if (f.Exists && f1.Exists) if (DateTime.Compare(f.LastWriteTime, f1.LastWriteTime)<0) recentFile = files[t];
					}
					file[i] = recentFile;
				}

				if(files.Length == 1) file[i] = files[0];
			}
			filePrefix=args[0];
			containerTag = args[3];
            
            int idtagsLength = args.Length - 4;
            identifyingTags = new string[idtagsLength];
            for (int i = 0; i < idtagsLength; i++) 
			    identifyingTags[i] = args[i+4];

            int rc = 0;
            try
            {
                Core.XMLCompare.XMLCompare myCompare = new Core.XMLCompare.XMLCompare(file[0], file[1], containerTag, identifyingTags);
                string compfile = myCompare.compare(filePrefix);

                Console.WriteLine("Comparison results are in the file '{0}'", compfile);
                rc = 0; //Success
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                rc =1; //Failure
            }
            finally
            {
                // Keep the console window open in debug mode.
                Console.WriteLine("");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(rc);
            }


        }
    }
}
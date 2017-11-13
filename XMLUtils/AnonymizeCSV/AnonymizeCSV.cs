using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Anonymization;
using System.IO;
using System.Diagnostics;

namespace Commandline.AnonymizeCSV
{
  /// <summary>
  /// This class anonymizes a csv according to the rules in AnonymizeCSV.txt and writes the result
  /// to a file with the orginal filename and "_Anonymous" appended to it
  /// This is static class with a static main method to provide a commandline interface for 
  /// the <see cref="Core.Anonymization.AnonymizerCSV"/> class.
  /// <para>
  /// <strong>Executable:  </strong><see cref="Core.Anonymization.AnonymizerCSV"/>.exe
  /// </para>
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class processes a csv, anonymizes according to the rules in anonymizeCSV.txt and writes the result
  /// to a file with the orginal filename and "_Anonymous" appended to it
  /// This is static class with a static main method to provide a commandline interface for 
  /// the <see cref="Core.Anonymization.AnonymizerCSV"/> class.
  /// </para>
  /// <para></para>
  /// <para>
  /// It evaluates the commandline and translates the parameters to arguments voor the constructor 
  /// of <see cref="Core.Anonymization.AnonymizerCSV"/></para>
  /// <para>Parameters are:</para>
  /// <list type="number">
  /// <item><term>form:&lt;any name&gt;.csv</term><description>Name of the CSV file</description></item>
  /// <item><term>form:separator="&lt;any single character&gt;"</term>
  /// <description>Indicates the character that separates the values</description></item>
  /// <item><term>form:textindicator=&lt;any single character&gt;</term>
  /// <description>Indicates the start and end of a text string</description></item>
  /// <item><term>form:headerrow=Y or N</term><description>Indicates if there's a headerrow</description></item>
  /// </list>
  /// </remarks>
  /// <example>
  /// <para><strong>Example for a call to the executable:</strong></para>
  /// <code>AnonymizeCSV.exe test.csv separator=";" textIndicator=" headerrow=N</code>
  /// </example>
  class AnonymizeCSV
  {
    /// <summary>
    /// Contains the constant voor the file with the anonymization rules, currently AnonymizeCSV.txt
    /// </summary>
    private const string anonymizeFile  = "AnonymizeCSV.txt";
    /// <summary>
    /// Postfix to add to the original filename to produce the filename of the file with the anonymized data, currently _Anonymous
    /// </summary>
    private const string anonymizePostfix = ".Anonymous.csv";
    /// <summary>
    /// Constant that is the name of the parameter that contains the separator
    /// </summary>
    private const string parmSeperator = "seperator";
    /// <summary>
    /// Constant that is the name of the parameter that contains the separator
    /// </summary>
    private const string parmTextIndicator = "textindicator";
    /// <summary>
    /// Constant that is the name of the parameter that contains the separator
    /// </summary>
    private const string parmheaderRowIndicator = "headerrow";

    /// <summary>
    /// Returns a helpmessage
    /// </summary>
    /// <returns></returns>
    private static string getHelpMessage()
    {
      StringBuilder sb = new StringBuilder("");
      sb.Append("This utility anonymizes a csv according to the rules in\r\n");
      sb.AppendFormat("{0} and writes the result to a file named the orginal filename\r\n", anonymizeFile);
      sb.AppendFormat("with \"{0}\" appended to it\r\n", anonymizePostfix);
      sb.Append("\r\n");
      sb.Append("Parameters:\r\n");
      sb.Append("-----------\r\n");
      sb.Append("1. filename of the CSV file to be anonymized\r\n");
      sb.Append("2. format:seperator=<any char>\tIndicates the character that separates the values\r\n");
      sb.Append("3. format:textindicator=<any char>.\tIndicates the start and end of a text string\r\n");
      sb.Append("4. format:headerrow=Y or N\tIndicates if there's a headerrow\r\n");
      sb.Append("\r\n");
      sb.Append("Example:\r\n");
      sb.Append("AnonymizeCSV.exe test.csv seperator=\";\" textIndicator=\" headerrow=N\r\n");

      return sb.ToString();
    }

    /// <summary>
    /// Function to simplify throwing exceptions
    /// </summary>
    /// <param name="msg"></param>
    private static void throwException(string msg) 
    {
      StringBuilder sb = new StringBuilder("\r\n");
      sb.Append(msg);
      sb.Append("\r\n");
      sb.Append(getHelpMessage());
      sb.Append("System error:");
      throw new System.ArgumentException(sb.ToString());
    }

    /// <summary>
    /// Main function that parses the arguments and creates an instance of <see cref="Core.Anonymization.AnonymizerCSV"/>
    /// Then it processes the file to be anonymzed.
    /// </summary>
    /// <param name="args">command line arguments</param>
    static void Main(string[] args)
    {
      int rc = 0;
      try
      {
        //check for arguments
        if (args.Length == 0)
          throwException("No parameters.");

        //check if the anonymizer file exists
        if (!File.Exists(anonymizeFile))
          throwException(String.Concat("File ", anonymizeFile, " with anonymize rules wasn't found!"));

        //check if the input file exists
        string inputfilename = args[0];
        if (inputfilename == null)
          throwException("File to be anonymized must be passed as a parameter");

        if (!File.Exists(inputfilename))
          throwException(String.Concat("File ", inputfilename, " to be anonymized wasn't found!"));

        //Process the remaining arguments
        //there is a complication with " as textindicator
        //The argument parser will think when it encounters a " that a literal string is passed  and will look for 
        //the next " or the end of the argument line so one argument can contain a value like this:
        //textIndicator= headerrow=N
        //while the commandline was:
        //AnonymizeCSV  textIndicator=" headerrow=N
        //The way to handle this is as follows:

        //first we concatenate all the args from the second argument back to one string
        StringBuilder sbarg = new StringBuilder();
        for (int i = 1; i < args.Length; i++)
        {
          string value = args[i].Trim().Replace("  ", " ");
          sbarg.Append(value);
          sbarg.Append(" ");
        }
        //then we split the arguments based on a spaces a separator
        string[] arguments = sbarg.ToString().Trim().Split(' ');

        //then we process the arguments
        string headerrow = "";
        string seperator = "";
        string textQualifier  = "";
        
        for (int i = 0; i < arguments.Length; i++) 
        {
          string [] argvalue = arguments[i].Split('=');
          string arg = argvalue[0].Trim().ToLower();
          string value = "";
          if (argvalue.Length > 0)
            value = argvalue[1].Trim().ToLower();

          if (arg.Equals(parmheaderRowIndicator))
            headerrow = value;
          if (headerrow.Length == 0)
            headerrow = "N";

          if (arg.Equals(parmSeperator))
            seperator = value;
          if (seperator.Length == 0)
            seperator = ",";

          if (arg.Equals(parmTextIndicator))
            textQualifier = value;
          if (textQualifier.Length == 0)
            textQualifier = "\"";
        }

        //Initialize the Anonymizer object
        //Pass the inputfilename to the contructor so the anonymizer can determine
        //which anonymize rules are applicable to the inputfile
        bool header = (headerrow.ToUpper()=="Y");
        AnonymizerCSV myAnonymizer = new AnonymizerCSV(inputfilename, anonymizeFile,seperator,textQualifier);

        // Read the input file and anonymize it
        List<String> headers = null;
        int countLines = 0;
        string msg = "";
        try
        {
          using (StreamReader sr = new StreamReader(inputfilename))
          using (StreamWriter writer = File.CreateText(String.Concat(myAnonymizer.Filename, anonymizePostfix)))
          {
            bool skip_first_row = header;
            while (sr.Peek() >= 0)
            {
              //Read the line with the values to be anonymized
              string line = sr.ReadLine();
              //Sla de eerste rij over als er een header is
              if (skip_first_row)
              {
                //process the header row
                headers = myAnonymizer.split(line);
                for(int i = 0;i < headers.Count;i++)
                headers[i] = headers[i].Replace(textQualifier,"");
              }

              countLines ++;
              //Split the line in its seperate values (no accounting for " or ')
              List<String> fields = myAnonymizer.split(line);
              if (!skip_first_row)
              {
                foreach (AnonymizeRule rule in myAnonymizer.AnonymizerRules)
                {
                int result = 0;
                int pos = 0; // mag niet nul zijn na onderstaand if then statement
                msg = "Rule to be processed:\t" + rule.ToString();
                if (headers != null) { 
                  msg += "\r\nHeaders:\r\n";
                  for (int i = 0; i < headers.Count; i++)
                    msg += headers[i] + seperator;
                }
                msg += "\r\nLine:\r\n" + line;

                if (Int32.TryParse(rule.Location, out result))
                {
                  pos = Int32.Parse(rule.Location);
                }
                else
                {
                  //Bevat location een header en een index ?
                  if (rule.Location.IndexOf(':') > 0)
                  {
                  string[] headervalues = rule.Location.Split(':');
                  if (headervalues.Length > 2)
                  {
                    StringBuilder mysb = new StringBuilder("");
                    mysb.AppendFormat("Location {0} in the anonymization rule is invalid. The character : may only occur once or not at all.", rule.Location);
                    throw new Exception(mysb.ToString());
                  }
                  if (headervalues != null)
                    pos = Int32.Parse(headervalues[0]);
                  }
                  else
                  {
                  //Het is geen numerieke waarde in rule.Location en het bevat geen :
                  //dus moet het een header zijn

                  //Bepaal of het een geldige header is en zo ja, zet pos op de index + 1
                  for (int i = 0; i < headers.Count; i++)
                  {
                    if (rule.Location.Replace(textQualifier, "").Equals(headers[i], StringComparison.OrdinalIgnoreCase))
                    {
                    pos = i + 1;
                    break;
                    }
                  }
                  }
                }
                //Als pos nu nog kleiner of gelijk aan 0 is dan is het fout
                if (pos <= 0)
                  rule.Allowed = false;

                 
                //pos bevat een geldige waarde
                if (pos <= fields.Count && rule.Allowed)
                {
                  string anonymizedValue = myAnonymizer.Anonymize(fields[pos - 1], rule.Method);
                  fields[pos - 1] = anonymizedValue;
                }
                else
                {
                  if (rule.Allowed)
                  Console.WriteLine("{0} has an issue:\r\nPosition {1} is not a valid column index!", rule.ToString(), pos);
                  rule.Allowed = false;
                }
                }
              }
              //merge fields
              StringBuilder sb = new StringBuilder("");
              for (int i = 0; i < fields.Count; i++)
              {
                sb.Append(fields[i]);
                if (i < (fields.Count - 1))
                  sb.Append(seperator);
              }

              //writeline to file
              writer.WriteLine(sb.ToString());
              if (skip_first_row)
                skip_first_row = false;
            }
          }
          Console.WriteLine("{0} lines in file {1} anonymized with {2} rules."
            , countLines, inputfilename, myAnonymizer.AnonymizerRules.Count);
        }
        catch (Exception e)
        {
          msg = String.Concat("The file could not be read:\r\n", e.Message,"\r\n", msg);
          Console.WriteLine(msg);
          Debug.Print(msg);
          }

        rc = 0; //Success
        StringBuilder rulesb = new StringBuilder();
        foreach (AnonymizeRule rule in myAnonymizer.AnonymizerRules)
          if (!rule.Allowed)
          rulesb.AppendFormat("{0}\r\n", rule.ToString());

        if (rulesb.Length > 0)
        {
          rulesb.Insert(0, "The following rules weren't used (allowed) in this session:\r\n");
          Console.WriteLine(rulesb.ToString());
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        rc = 1; //Error
      }
      finally
      {
        // Keep the console window open in debug mode.
        //Console.WriteLine("");
        //Console.WriteLine("Press any key to exit.");
        //Console.ReadKey();
        Environment.Exit(rc);
      }
    }
  }
}

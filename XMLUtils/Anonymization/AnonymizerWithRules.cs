using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Core.Anonymization
{
  /// <summary>
  /// Extension of the Anonymizer class to handle files that contain Anonymizer rules
  /// </summary>
  /// <remarks>
  /// <para>
  /// The anonymizer class is used to replace meaningful values with meaningless values.
  /// Anonymizing is done to cloak personal information like names, account number and anything that can be used
  /// to identify a person and use that information in a way that was not intended by the person, identified by the information.
  /// The <see cref="AnonymizerWithRules"/> class extends the Anonymizer class by providing functions to import a file with Anonymization rules.
  /// These rules provide a filemask and the data elements for instances of the <see cref="AnonymizeRule"/> class.
  /// </para>
  /// <para>
  /// A rule file consists of the following elements, seperated by a ; :
  /// </para>
  /// <list type="number">
  /// <listheader>
  /// <term>Element</term>
  /// <description>Description</description>
  /// </listheader>
  /// <item>
  /// <term>Filemask</term>
  /// <description>
  /// Part of the name of the file to be anonymized. This way one rules file can apply to multiple files to be anonymized.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Location</term>
  /// <description>
  /// Indicates the element to be anonymized. For a CSV file this is a column name or index. For an XML file, this is an xpath expression
  /// </description>
  /// </item>
  /// <item>
  /// <term>FilterLocation</term>
  /// <description>
  /// <para> First part of a filter.Indicates the element whose value is to be filtered on. For CSV it can be a header name or index. 
  /// For XML, it's an xpath expression that identifies 
  /// 1 XML element</para>
  /// <para>Optional when none of the other parts of the filer are empty</para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>FilterOperator</term>
  /// <description>
  /// <para>Second part of the filter. Comparison operator for the filter</para>
  /// <para>Optional when none of the other parts of the filer are empty</para>
  /// </description>
  /// </item>
  /// <item>
  /// <term>FilterValue</term>
  /// <description>
  /// <para>Third part of a filter. Contains the value to which the filter compares the data element, specified in <see cref="AnonymizeRule.FilterLocation"/></para>
  /// <para>Optional when none of the other parts of the filer are empty</para>
  /// </description>
  /// </item>
  /// </list>
  /// </remarks>
  /// <example>
  /// <para><h2>Examples of anonymization rules</h2></para>
  /// <para><h3>Example for CSV file (no header)</h3></para>
  /// <list type="bullet">
  /// <item>House;1;Houseaidvnr</item>
  /// <item>test;1;Text</item>
  /// </list>
  /// <para><h3>Example for an XML file</h3></para>
  /// <list type="bullet">
  /// <item>House;//hs:Rel_Match/hs:AchterNaam;Text</item>
  /// <item>House;//hs:Rel_Match/hs:GeboorteDt;Date</item>
  /// </list>
  /// </example>
  public class AnonymizerWithRules : Anonymizer
  {
      //Variables below needed for support anonymizeRules file bestand
      /// <summary>
      /// Constant: Value <![CDATA[<filename>]]>
      /// </summary>
      private const string fnIndicator = "<filename>";

      /// <summary>
      /// List of anonymization Rules. see <see cref="AnonymizeRule"/>. Only accessible to descendants
      /// </summary>
      protected List<AnonymizeRule> _AnonymizerRules;
        /// <summary>
      /// List of anonymization Rules. see <see cref="AnonymizeRule"/>
      /// </summary>
      public List<AnonymizeRule> AnonymizerRules
      {
          get { return _AnonymizerRules; }
          set { _AnonymizerRules = value; }
      }

      /// <summary>
      /// This list stores the (parts of) the filenames that are stored in the Anonymization rules
      /// </summary>
      protected List<String> _anonymizerRulesFilename;
      /// <summary>
      /// Filename for the files that contains the rules
      /// </summary>
      public List<String> AnonymizerRulesFilename
      {
          get { return _anonymizerRulesFilename; }
      }
      /// <summary>
      /// Contains the file to be anonymized
      /// </summary>
      private string _filename = "";

      /// <summary>
      /// read only property for private member _filename
      /// </summary>
      public string Filename
      {
        get { return _filename; }
      }
      /// <summary>
      /// Contains the directory of the file to be anonymized
      /// </summary>
      private string _filepath = "";
        
      /// <summary>
      /// <para>
      /// This property manages two private members: _filename and _filepath.
      /// </para><para>
      /// When it's set, it's value is split in its directory and filename. If no directory is in the filename, _filepath remains empty
      /// </para>
      /// </summary>
      protected string File_to_be_anonymized
      {
        get { return String.Concat(_filepath,_filename); }
        set {
          if (value.IndexOf("\\") < 0)
            _filename = value;
          else
          {
            int pos = value.LastIndexOf("\\");
            _filepath = value.Substring(0, pos + 1);
            _filename = value.Substring(pos + 1);
          }
        }
      }
      /// <summary>
      /// Contains the file to be anonymized
      /// </summary>
      protected string _anonymizerFile = "Anonymize.txt";
      //protected virtual string[] dateformats = { "yyyymmdd", "dd/mm/yy", "dd-mm-yy", "dd/mm/yyyy", "dd-mm-yyyy" };


      /// <summary>
      /// Constructor. Initializes the AnonymizerWithRules instance with the file to be anonymized and the name of the the file with the rules
      /// </summary>
      /// <param name="file_to_be_anonymized">name of the file to be anonymized</param>
      /// <param name="file_with_anonymizeRules">name of file with anonymization rules</param>
      public AnonymizerWithRules(string file_to_be_anonymized, string file_with_anonymizeRules)
      {
          _anonymizerFile = file_with_anonymizeRules;
          common_constructor(file_to_be_anonymized);
          if (this.GetType().Name == "AnonymizerWithRules")
              ProcessAnonymizerFile();
      }

        /// <summary>
      /// Constructor. Initializes the AnonymizerWithRules instance with the file to be anonymized and the name of the the file with the rules
      /// </summary>
      /// <param name="file_to_be_anonymized">name of the file to be anonymized</param>
      public AnonymizerWithRules(string file_to_be_anonymized)
      {
          common_constructor(file_to_be_anonymized);
          if (this.GetType().Name == "AnonymizerWithRules")
              ProcessAnonymizerFile();
      }

      /// <summary>
      /// Constructor. Initializes with default values except for the memeber for file to be anonymized. It will be empty.
      /// </summary>
      public AnonymizerWithRules()
      {
          base.common_constructor();
      }

      /// <summary>
      /// This is a method that's called by all constructors to initialize members common to all constructors
      /// </summary>
      /// <param name="file_to_be_anonymized"> file whose data and maybe name will be anonymized</param>
      protected virtual void common_constructor(string file_to_be_anonymized)
      {

          _AnonymizerRules = new List<AnonymizeRule>();
          _anonymizerRulesFilename = new List<String>();
          common_constructor();
          File_to_be_anonymized = file_to_be_anonymized;

      }

      /// <summary>
      ///This method reads the rules file and determines which rules are applicable to the file to be anonymized. See remarks in <see cref="AnonymizerWithRules"/> for further details
      /// </summary>
      protected void ProcessAnonymizerFile()
      {
          if (_anonymizerFile == null) return;

          try
          {
              using (StreamReader sr = new StreamReader(_anonymizerFile))
              {
                  while (sr.Peek() >= 0)
                  {
                      string line = sr.ReadLine();
                      string[] fields = line.Split(';');
                      if (_filename.ToLower().IndexOf(fields[0].ToLower()) >= 0)
                      {
                          ProcessRule(fields);
                      }
                  }
              }
          }
          catch (Exception e)
          {
              string msg = String.Concat("The file could not be read:\r\n", e.Message);
              Console.WriteLine(msg);
              Debug.Print(msg);                
          }

      }

      /// <summary>
      /// This method processes a single rule in the anonymization riles file, then adds it to the _AnonymizerRules list.
      /// This method is a hook for derived classes to implement their own method and extend the basic handling of this class. see class 
      /// Core.XMLRecords.Anonymization.AnonymizerXML  for an example
      /// </summary>
      /// <param name="fields">The fields that have been extracted from the line to be processed in the rules file</param>
      protected virtual void ProcessRule(string[] fields)
      {
          if (fields[2].ToLower().IndexOf(fnIndicator.ToLower()) >= 0)
          {
              _anonymizerRulesFilename.Add(fields[1]);
          }
          else
          {
              AnonymizeRule rule = new AnonymizeRule();
              rule.Location = fields[1];
              rule.Method = AnonymizeRule.getMethod(fields[2]);
              //Add filter elements if they are present
              if (fields.Length > 3)
                  rule.FilterLocation = fields[3];
              if (fields.Length > 4)
                  rule.FilterOperator = fields[4];
              if (fields.Length > 5)
                  rule.FilterValue = fields[5];
              _AnonymizerRules.Add(rule);
          }

      }

      //Public functions
      /// <summary>
      /// Sets the name of the file to be anonymized
      /// </summary>
      /// <param name="filename">file to be anonymized</param>
      public void setAnonymizerFile(string filename)
      {
          _anonymizerFile = filename;
      }

      /// <summary>
      /// This function processes special anonymization rules that require filters
      /// </summary>
      /// <param name="rule">Anonymization rule to be processed</param>
      /// <param name="value">The value to be anonymized</param>
      /// <param name="filterVariableValue">The variabele value that has to satisfy the filter condition</param>
      /// <returns>anonymized value</returns>
      /// <remarks>
      /// This functions checks if a value or its context is filtered. If it is, the value is anonymized
      /// </remarks>
      public string ProcessFilter(AnonymizeRule rule, string value, string filterVariableValue)
      {
        string result = "";

        if (rule.IsFiltered && filterVariableValue != null)
        {
            //example rule: file;header;Empty;header3;<;20
            var compareresult = filterVariableValue.CompareTo(rule.FilterValue);
            if (rule.FilterOperator == "<" && rule.Method == AnonymizeRule.MethodType.Empty)
            {
                result = value;
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Filter rule {0} was correct (filterVariableValue={1}). Value {2} can be anonymized"
                , rule.Filter, filterVariableValue, value);
                Console.WriteLine(sb.ToString());
                if (compareresult < 0)
                {
                    Console.WriteLine("and was emptied");
                    result = "";
                }
                else
                    Console.WriteLine("but wasn't because it didn't satisfy the filter condition!");
            }
        }
        else
        {
            Console.WriteLine("Filter rule {0} was incorrect (filterVariableValue={1}). Value {2} was not anonymized!"
                , rule.Filter, filterVariableValue, value);
        }
        return result;
      }
      public bool IsFiltered(AnonymizeRule rule, string value)
      {
        var compareresult = value.CompareTo(rule.FilterValue);

        if (!rule.IsFiltered)
          return false;

        if (rule.FilterOperator.Equals("!="))
        {
          return (compareresult != 0);
        }
        if (rule.FilterOperator.Equals("="))
        {
          return (compareresult == 0);
        }
        if (rule.FilterOperator.Equals("<"))
        {
          return (compareresult < 0);
        }
        if (rule.FilterOperator.Equals(">"))
        {
          return (compareresult > 0);
        }

        return false;
      }
  }


}

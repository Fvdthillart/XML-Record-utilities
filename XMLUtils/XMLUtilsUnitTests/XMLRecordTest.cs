using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Core.XMLRecords;
using System.Text.RegularExpressions;

namespace XMLUtilsUnitTests
{
  /// <summary>
  /// Unit test for class XMLRecord
  /// </summary>
  [TestClass]
  public class XMLRecordTest
  {
    #region members
    string testID = "1,2,3";
    string[] testIDs = { "1", "2", "3" };
    string[] errMsgs = { "Parameter IDs is empty"
                        ,"Parameter XMLRecord is null or empty"
                        ,"Parameter XMLDecl is null or empty"
                        ,"Parameter nsTag is null or empty"
                        ,"Parameter nsEndTag is null or empty"
                        };
    List<string> testIDList = new List<string>();

    string XMLDecl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
    string XMLDeclTestPattern = "^(?i)\\<\\?xml\\s+version\\s*=\\s*\\\"";

    string nsTagTestPattern = "^(?i)\\<.+(xmlns:.+=\"[\\w:\\/\\s]+\"\\s*>\\s*)+";
    string nsEndTagTestPattern = "(?i)\\</[\\w=\"\\s]+\\s*>";

    private string filename = "HYPSCH_1_7.1_B_20121231_M_20131119095127.xml";

    RegexOptions RegexOpts = RegexOptions.IgnoreCase
                            | RegexOptions.IgnorePatternWhitespace
                            | RegexOptions.Compiled;

    #endregion

    #region Private functions

    private void TestRegEx(bool mustMatch, string toBeTested, string regex)
    {
      Regex pattern = new Regex(regex, RegexOpts);
      if (mustMatch)
        StringAssert.Matches(toBeTested, pattern);
      else
        StringAssert.DoesNotMatch(toBeTested, pattern);
    }

    private void  fillTestIDList() {
      for (int c = 0; c < testIDs.Length; c++)
        testIDList.Add(testIDs[c]);
    }

    #endregion

    #region Test methods
    /// <summary>
    /// Test if a simplified constructor for an indexOf implementation works correctly
    /// </summary>
    [TestMethod]
    public void XMLRecordSearchConstructor()
    {
      XMLRecord testRec = new XMLRecord(testID);
      int i = 0;
      foreach (string ID in testRec.getIDList())
      {
        i++;
        Assert.AreEqual(ID, i.ToString());
      }
      Assert.IsTrue(testRec.SearchOnly);
      Assert.IsTrue(testRec.getXMLRecord().Length == 0);
      Assert.IsTrue(testRec.getID() == testID);
    }

    /// <summary>
    /// Test to see if all the list functions like IndexOf and Sort function as expected
    /// </summary>
    [TestMethod]
    public void XMLRecordBehaviorInList()
    {
      List<XMLRecord> testList = new List<XMLRecord>();
      string[] testArray = new string[5];
      int c = 0;
      testArray[c] = testID; c++;
      testArray[c] = "4,5,6,7"; c++;
      testArray[c] = "2,3,4"; c++;
      testArray[c] = "3,4,5,6,7,8"; c++;
      testArray[c] = "3,,,,7,8"; c++;

      for (int i = 0; i < testArray.Length; i++)
      {
        testList.Add(new XMLRecord(testArray[i]));
      }


      // test Sort
      testList.Sort();
      c = 0;
      int[] sortorder = {0,2,4,3,1};
      foreach (XMLRecord testRec in testList)
      {
        Console.WriteLine(testRec.getID());
        Assert.AreEqual(testArray[sortorder[c]], testRec.getID());
        c++;
      }

      //test indexOf, contains, BinarySearch
      XMLRecord searchRec = new XMLRecord(testArray[2]);
      Assert.IsTrue(testList.IndexOf(searchRec) == 1);
      Assert.IsTrue(testList.Contains(searchRec)); 
      Assert.IsTrue(testList.BinarySearch(searchRec) == 1);
    }
    
    /// <summary>
    /// Test for factory method <see cref="XMLRecord.XMLRecordFactory(List&lt;string&gt;, string, string, string, string)"/> 
    /// to see if null parameters return a null reference and throw the correct exceptions
    /// </summary>
    [TestMethod]
    public void XMLRecordfactoryMethodAllArgsAreNull()
    {
      fillTestIDList();      
      int i = 0;
      for (int c = 0; c < testIDs.Length; c++) {
        try
        {
          XMLRecord testRec = null;
          i++;
          switch (i+1)
          {
            case 1:
              testRec = XMLRecord.XMLRecordFactory(null, null, null, null, null);
              break;
            case 2:
              testRec = XMLRecord.XMLRecordFactory(testIDList, null, null, null, null);
              break;
            case 3:
              testRec = XMLRecord.XMLRecordFactory(testIDList, "<XML>Not really XML</XML>", null, null, null);
              break;
            case 4:
              testRec = XMLRecord.XMLRecordFactory(testIDList, "<XML>Not really XML</XML>", XMLDecl, null, null);
              break;
            case 5:
              testRec = XMLRecord.XMLRecordFactory(testIDList, "<XML>Not really XML</XML>", XMLDecl, "nsTag", null);
              break;
          }
        }
        catch(Exception e)
        {
          Assert.IsInstanceOfType(e, typeof(ArgumentException));
          Assert.AreEqual(errMsgs[i], e.Message);
        }
      }
    }


    /// <summary>
    /// Test for factory method <see cref="XMLRecord.XMLRecordFactory(List&lt;string&gt;, string, string, string, string)"/> 
    /// to see if all parameters are correctly handled
    /// </summary>
    [TestMethod]
    public void XMLRecordfactoryMethodSuccesfulInstantiation()
    {
      fillTestIDList();
      XMLRecord testRec = XMLRecord.XMLRecordFactory(testIDList, "<XML>Not really XML</XML>", XMLDecl, "<nsTag xmlns:hpg=\"some namespace\">", "</endNSTag>");
      // Test IDs
      int i = 0;
      foreach (string ID in testRec.getIDList())
      {
        i++;
        Assert.AreEqual(ID, i.ToString());
      }
      //TestXMLRecord??
      //test XMLDecl
      testRec.MustBeValidatable = true;
      TestRegEx(true, testRec.getXMLRecord(), XMLDeclTestPattern);
      
      //namespaceTag
      TestRegEx(true, testRec.NamespaceTag, nsTagTestPattern);

      //namespace end tag
      TestRegEx(true, testRec.EndNamespaceTag, nsEndTagTestPattern);

      Assert.IsTrue(!testRec.SearchOnly);


    }

    /// <summary>
    /// Test a new feature in the XMLRecord class: property <see cref="XMLRecord.MustBeValidatable"/>
    /// If the property is set to true, an xml declaration and a root element are added if they are stored in the XMLRecord. 
    /// </summary>
    [TestMethod]
    public void XMLrecordMustBevalidatable()
    {
      try
      {
        List<String> Containertags = new List<String>();
        Containertags.Add("hyp:Schuldenaar");
        List<String> IDtags = new List<String>();
        IDtags.Add("hyp:PartijId");
        XMLRecordFileIterator myXMLFile = XMLRecordFileIterator.GetXMLRecordFileIterator(filename, Containertags, IDtags);

        int i = 0;
        XMLRecord testRec = null;
        foreach (XMLRecord myRec in myXMLFile)
        {
          testRec = myRec;
          i++;
          if (i == 12) break;
        }

        TestRegEx(false, testRec.getXMLRecord(), XMLDeclTestPattern);
        Console.WriteLine(testRec.getXMLRecord());
        Console.WriteLine("--------------------------------------------------------");
        testRec.MustBeValidatable = true;
        TestRegEx(true, testRec.getXMLRecord(), XMLDeclTestPattern);
        Console.WriteLine(testRec.getXMLRecord());
        Console.WriteLine("--------------------------------------------------------");
      }
      catch (Exception e)
      {
        Console.WriteLine("--------------------------------------------------------");
        Console.WriteLine(e.Message);
        //System.Diagnostics.Debug.Print(e.Message);
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
      }
    }

    #endregion
  }
}

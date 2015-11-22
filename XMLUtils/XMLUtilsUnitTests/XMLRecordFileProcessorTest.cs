using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.XMLRecords;
using System.Collections.Generic;
using System.IO;

namespace XMLUtilsUnitTests
{
  /// <summary>
  /// Unit tests for the XMLRecordFileProcessor
  /// </summary>
  [TestClass]
  public class XMLRecordFileProcessorTest
  {
    private string filename = "HYPSCH_1_7.1_B_20121231_M_20131119095127.xml";
    private string testIDList = "1,101,103,105,107,109,11,111,113,115,117,119,121,123,125,127,129,13,131,133,135,137,139,141,143,145,147,149,15,151,153,155,157,159,161,163,165,167,169,17,171,173,175,177,179,181,183,185,187,189,19,191,193,195,198,200,202,204,206,209,21,211,213,215,217,219,221,223,225,227,229,23,231,233,235,237,240,242,244,246,248,25,250,252,254,256,258,260,262,264,266,268,27,270,272,274,276,279,281,283,285,287,289,29,291,293,295,297,299,3,301,303,305,307,309,31,311,313,315,317,319,321,323,325,327,329,33,331,333,335,337,339,341,343,345,347,349,35,351,353,355,357,359,361,363,365,367,369,37,371,373,375,377,379,381,383,385,387,389,39,391,393,395,397,399,401,403,405,407,409,41,411,413,415,417,419,421,423,425,427,429,43,431,433,435,437,439,441,443,445,447,449,45,451,453,455,457,459,461,463,465,467,469,47,471,473,475,477,479,481,483,486,488,49,490,492,494,496,498,5,500,502,505,507,509,51,511,513,515,517,519,521,523,525,527,529,53,531,533,535,537,539,541,543,545,547,549,55,551,553,555,557,559,561,563,565,568,57,570,572,574,576,578,580,582,584,586,588,59,590,592,594,596,598,600,602,604,606,608,61,610,612,614,616,618,620,622,624,626,628,63,630,632,634,636,638,640,642,644,646,648,65,650,652,654,656,658,660,662,664,666,668,67,670,672,674,676,678,680,682,685,69,7,71,73,75,77,79,81,83,85,87,89,9,91,93,95,97,99";

    private string testContainerTag = "hyp:Schuldenaar";
    private string testIDTag = "hyp:PartijId";

    private XMLRecordFileProcessor getProcessor()
    {
      List<String> Containertags = new List<String>();
      Containertags.Add(testContainerTag);
      List<String> IDtags = new List<String>();
      IDtags.Add(testIDTag);

      XMLRecordFileProcessor testProcessor = new XMLRecordFileProcessor(filename, Containertags.ToArray()[0], IDtags.ToArray());
      return testProcessor;
    }

    /// <summary>
    /// Test ToMemory processing
    /// ToMemory stores the XML Records of a file in a list of type List<![CDATA[<XMLRecord>]]>
    /// The <see cref="XMLRecordFileProcessor"/> class exposes this list by means of:
    /// <list type="bullet">
    /// <item>the public property <see cref="XMLRecordFileProcessor.XMLRecordList"/></item>
    /// <item>the method <see cref="XMLRecordFileProcessor.IndexOf(string)"/></item>
    /// <item>the method <see cref="XMLRecordFileProcessor.getXMLRecord(string)"/></item>
    /// <item>the method <see cref="XMLRecordFileProcessor.getIDList"/></item>
    /// </list>
    /// </summary>
    [TestMethod]
    public void XMLRecordFileProcessorToMemory()
    {
      XMLRecordFileProcessor testProcessor = getProcessor();
      Assert.IsNull(testProcessor.XMLRecordList);
      Assert.IsNull(testProcessor.getIDList());
      Assert.IsNull(testProcessor.getXMLRecord("1"));
      Assert.AreEqual(-1, testProcessor.IndexOf("1"));
      testProcessor.Process(XMLRecordFileProcessor.ProcessType.ToMemory);
      Assert.IsNotNull(testProcessor.XMLRecordList);
      Assert.IsNotNull(testProcessor.getIDList());
      Assert.IsNotNull(testProcessor.getXMLRecord("1"));
      Assert.AreEqual(109, testProcessor.IndexOf("3"));

      List<XMLRecord> testList = testProcessor.XMLRecordList;
      Console.WriteLine("{0} XML records in list!", testList.Count);

      //check the sorting of the XMLRecordList
      //build a CSV string of the list
      string test = TestUtils.List2CSV(testProcessor.getIDList());
      Assert.AreEqual(testIDList, test);

    }

    /// <summary>
    /// <strong>Test processing type toFile</strong><br />
    /// The original file in filename will be processed and every XMLRecord is written to a file
    /// in a compressed directory that has the same name as the file except for ".xml"
    /// </summary>
    [TestMethod]
    public void XMLRecordFileProcessorToFile()
    {
      XMLRecordFileProcessor testProcessor = getProcessor();
      testProcessor.Process(XMLRecordFileProcessor.ProcessType.ToFile);
      //check if the corresponding directory exists
      string dirName = filename.Replace(".xml", "");
      Assert.IsTrue(Directory.Exists(dirName));
      string[] testFiles = Directory.GetFiles(dirName);
      Assert.AreEqual(339, testFiles.Length);

      //Verify if the names contain the containerTag, IDtag and ID values of the XML record
      string[] Ids = testIDList.Split(',');
      for (int i = 0; i < testFiles.Length; i++)
      {
        Assert.IsTrue(testFiles[i].IndexOf(testContainerTag.Replace(":", "_")) >= 0);
        Assert.IsTrue(testFiles[i].IndexOf(testIDTag.Replace(":", "_")) >= 0);
        Assert.IsTrue(testFiles[i].IndexOf(Ids[i]) >= 0);
      }
    }

    
    /// <summary>
    /// <strong>Test processing type AnonymizetoFile</strong><br />
    /// The original file in filename will be processed and every XMLRecord is written to a file
    /// in a compressed directory that has the same name as the file except for ".xml"
    /// The contents of the files will be anonymized according to the anonymization rules in anonymize.text
    /// </summary>
    [TestMethod]
    public void XMLRecordFileProcessorAnonymizeToFile()
    {
      XMLRecordFileProcessor testProcessor = getProcessor();
      testProcessor.Process(XMLRecordFileProcessor.ProcessType.AnonymizeToFiles);
      //check if the corresponding directory exists
      string dirName = filename.Replace(".xml", "");
      Assert.IsTrue(Directory.Exists(dirName));
      string[] testFiles = Directory.GetFiles(dirName);
      Assert.AreEqual(339, testFiles.Length);

      //Verify if the names contain the containerTag, IDtag and ID values of the XML record
      string[] Ids = testIDList.Split(',');
      for (int i = 0; i < testFiles.Length; i++)
      {
        Assert.IsTrue(testFiles[i].IndexOf(testContainerTag.Replace(":", "_")) >= 0);
        Assert.IsTrue(testFiles[i].IndexOf(testIDTag.Replace(":", "_")) >= 0);
        Assert.IsTrue(testFiles[i].IndexOf(Ids[i]) >= 0);
      }
    }
  }
}

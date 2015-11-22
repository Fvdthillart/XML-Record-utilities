using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.XMLRecords;
using System.Text.RegularExpressions;

namespace XMLUtilsUnitTests
{
  /// <summary>
  /// Unit tests for <see cref="XMLRecordFileIterator"/>
  /// </summary>
  [TestClass]
  public class XMLRecordFileIteratorTest
  {
    private string filename = "HYPSCH_1_7.1_B_20121231_M_20131119095127.xml";

    /// <summary>
    /// Test if the correct exception is thrown when all arguments are null
    /// </summary>
    [TestMethod]
    public void XMLRecordFileIteratorFactoryMethodAllNullArgs()
    {
      try
      {
        XMLRecordFileIterator myXMLFile = XMLRecordFileIterator.GetXMLRecordFileIterator(null, null, null);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
        Assert.AreEqual("Invalid argument: File  doesn't exist!", e.Message.ToString());
      }
    }

    /// <summary>
    /// Test if the correct exception is thrown when either the Containertags list or the IDtags list is null
    /// </summary>
    [TestMethod]
    public void XMLRecordFileIteratorFactoryMethodListsAreNull()
    {
      try
      {
        XMLRecordFileIterator myXMLFile = XMLRecordFileIterator.GetXMLRecordFileIterator(filename, null, null);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
        Assert.AreEqual("Invalid argument: Either the list Containertags or IDtags is null ", e.Message.ToString());
      }
    }

    /// <summary>
    /// Test if the correct exception is thrown when the lists are not null but contain no elements
    /// </summary>
    [TestMethod]
    public void XMLRecordFileIteratorFactoryMethodNoItemsInList()
    {
      try
      {
        List<string> list1 = new List<string>();
        List<string> list2 = new List<string>();
        XMLRecordFileIterator myXMLFile = XMLRecordFileIterator.GetXMLRecordFileIterator(filename, list1, list2);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
        Assert.AreEqual( "Invalid argument: Either the list Containertags or IDtags has no elements ", e.Message.ToString());
      }
    }
    /// <summary>
    /// Test if iterator works correctly. The XMLRecords are outputted
    /// </summary>
    [TestMethod]
    public void XMLRecordFileIteratorSuccesfulInstantiationAndIteration()
    {
      try
      {
        List<String> Containertags = new List<String>();
        Containertags.Add("hyp:Schuldenaar");
        List<String> IDtags = new List<String>();
        IDtags.Add("hyp:PartijId");
        XMLRecordFileIterator myXMLFile = XMLRecordFileIterator.GetXMLRecordFileIterator(filename, Containertags, IDtags);
        Assert.IsInstanceOfType(myXMLFile, typeof(XMLRecordFileIterator));
        Assert.IsNotNull(myXMLFile);

        int i = 0;
        Console.WriteLine("--------------------------------------------------------");
        foreach (XMLRecord myRec in myXMLFile)
        {
          Assert.IsNotNull(myRec);
          i++;
          Console.WriteLine("{0}:\r\n{1}", i, myRec.getXMLRecord());
          Console.WriteLine("--------------------------------------------------------");
        }
        Assert.IsTrue(i > 0);
      }
      catch (Exception e)
      {
        Assert.IsInstanceOfType(e, typeof(ArgumentException));
      }

    }

  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLUtilsUnitTests
{
  class TestUtils
  {
    public static string List2CSV(List<string> inputList)
    {
      string result = "";
      foreach (string item in inputList)
        result += String.Concat(item, ",");
      result = result.Remove(result.Length - 1);
      return result;
    }
  }
}

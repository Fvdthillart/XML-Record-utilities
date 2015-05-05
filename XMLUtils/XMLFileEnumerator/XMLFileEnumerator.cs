using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dts.Runtime;


namespace Core.XMLRecords.XMLFileEnumerator
{
    [DtsForEachEnumerator( DisplayName = "XMLRecordEnumerator", Description="A sample custom enumerator", UITypeName="FullyQualifiedTypeName,AssemblyName,Version=1.00.000.00,Culture=Neutral,PublicKeyToken=<publickeytoken>")]
    public class XMLFileEnumerator
    {
      #region members
      private string variableNameValue;

      public string VariableName
      {
        get { return this.variableNameValue; }
        set { this.variableNameValue = value; }
      }
      #endregion

      public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser, IDTSInfoEvents infoEvents, IDTSLogging log)
      {
        if (!variableDispenser.Contains(this.variableNameValue))
        {
          infoEvents.FireError(0, this.GetType().ToString(), "The Variable " + this.variableNameValue + " does not exist in the collection.", "", 0);
          return DTSExecResult.Failure;
        }
        return DTSExecResult.Success;
      }

      public override object GetEnumerator()
      {
        ArrayList numbers = new ArrayList();

        Random randomNumber = new Random(DateTime.Now.Ticks);

        for (int x = 0; x < 100; x++)
          numbers.Add(randomNumber.Next());

        return numbers;
      }
    }
}

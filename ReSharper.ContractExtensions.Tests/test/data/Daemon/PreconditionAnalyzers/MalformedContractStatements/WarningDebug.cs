using System.Linq;
using System.Diagnostics.Contracts;

class A
{
  public void InsideSwitch(string s)
  {
    switch(s)
    {
      case "foo":
        Contract.Requires(s != null);
        break;
    }
  }
}
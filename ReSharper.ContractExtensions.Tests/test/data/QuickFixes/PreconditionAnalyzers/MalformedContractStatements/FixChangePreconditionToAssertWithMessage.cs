using System.Linq;
using System.Diagnostics.Contracts;

class A
{
  public void InsideIf(string s)
  {
    if (s != null)
      {caret}Contract.Requires(s.Length == 42, "Message");
  }
}
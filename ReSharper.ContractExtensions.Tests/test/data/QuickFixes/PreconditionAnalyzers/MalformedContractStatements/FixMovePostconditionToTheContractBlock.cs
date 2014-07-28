using System.Linq;
using System.Diagnostics.Contracts;

class A
{
  public void InsideIf(string s)
  {
    Contract.Requires(s != null);

    if (s != null)
      {caret}Contract.Ensures(s.Length == 42);
  }
}
using System.Linq;
using System.Diagnostics.Contracts;

class A
{
  public void InsideIf(string s)
  {
    int n = 42;
    while (n > 0)
    {
      n--;
      {caret}Contract.EndContractBlock();
    }
  }
}
#define CONTRACTS_FULL
using System.Diagnostics.Contracts;

class A
{
  public void WarningInTryBlock(string s)
  {
    try
    {
      // This tool will show another warning for this case!
      Contract.Requires(s != null);
    }
    finally {}
  }

}
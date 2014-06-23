using System.Diagnostics.Contracts;
using System;

abstract class A
{
  void EnableForContractRequiresWithMessage(string s)
  {
    Con{on}tract.Requires(s != null, "s should not be null");
  }

  void EnableForMethodCallAndAdditionalMessage(string s)
  {
    {on}if (string.IsNullOrEmpty(s)) throw new ArgumentNullException("s", "s should not be null");
  }

  public void EnableOnRangeCheck(int n)
  {
    {on}if (n <= 0 && n >= 42) throw new System.ArgumentOutOfRangeException("n", "n should be from 1 to 41!");
  }


}
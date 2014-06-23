using System.Diagnostics.Contracts;
using System;

abstract class A
{
  void EnableForContractRequires(string s)
  {
    Con{on}tract.Requires(s != null);
  }

  void EnableForGenericContractRequires(string s)
  {
    Cont{on}ract.Requires<ArgumentNullException>(s != null);
  }

  void EnableForIfCheck(string s)
  {
    i{on}f (s == null) throw new ArgumentNullException("s");
  }

  void EnableForComplexIfCheck(string s)
  {
    {on}if (s == null || s.Length == 0) throw new ArgumentNullException("s");
  }

  void EnableForMethodCall(string s)
  {
    {on}if (string.IsNullOrEmpty(s)) throw new ArgumentNullException("s");
  }

  void EnableForMethodCallAndAdditionalMessage(string s)
  {
    {on}if (string.IsNullOrEmpty(s)) throw new ArgumentNullException("s", "s should not be null");
  }


}
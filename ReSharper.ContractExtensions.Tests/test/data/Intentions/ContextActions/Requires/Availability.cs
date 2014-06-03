using System.Diagnostics.Contracts2;
using System;

abstract class A
{
  void EnabledIfContractRequiresExistsButNotChecksNull(string s{on})
  {
    Contract.Requires(s != "");
  }
}
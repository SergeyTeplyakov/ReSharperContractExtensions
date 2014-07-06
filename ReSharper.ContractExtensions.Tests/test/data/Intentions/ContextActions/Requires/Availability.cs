using System.Diagnostics.Contracts;
using System;

abstract class A
{
  void EnabledIfContractRequiresExistsButNotChecksNull(string s{on})
  {
    Contract.Requires(s != "");
  }
}
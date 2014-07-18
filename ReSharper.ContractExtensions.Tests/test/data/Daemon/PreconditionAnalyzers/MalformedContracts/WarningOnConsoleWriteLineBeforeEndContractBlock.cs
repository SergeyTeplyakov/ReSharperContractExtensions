using System.Diagnostics.Contracts;
using System;

class A
{
  public void Foo(string s)
  {
    Console.WriteLine("Hello");
    Contract.EndContractBlock();
  }
}
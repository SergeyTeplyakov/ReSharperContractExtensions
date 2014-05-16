using System.Diagnostics.Contracts;

class A
{
  void Foo2(string s0, string s1, string s2{caret})
  {
    Contract.Requires(s1 != null);
    Contract.Requires(s0 != null);
  }
}
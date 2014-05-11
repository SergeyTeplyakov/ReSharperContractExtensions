using System.Diagnostics.Contracts;

class A
{
  void Foo(string s1, string s2{caret})
  {
    Contract.Requires(s1 != null);
  }
}
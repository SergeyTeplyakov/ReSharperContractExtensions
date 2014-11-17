using System.Collections.Generic;

class A
{
  public IEnumerable<int> Foo(string s)
  {
    {caret}if (s == null) throw new System.ArgumentNullException("s");

    yield return 42;
  }
}
using System.Threading.Tasks;
using System.Collections.Generic;

class A
{
  public async Task Foo(string s)
  {
    {caret}if (s == null) throw new System.ArgumentNullException("s");

    await Task.Delay(42);
  }
}
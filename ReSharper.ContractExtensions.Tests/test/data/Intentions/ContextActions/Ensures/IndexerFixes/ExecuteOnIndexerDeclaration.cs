using System.Diagnostics.Contracts;

class A
{
  public ob{caret}ject this[string index]
  {
    get
    {
      Contract.Requires(index != null);
      return new object();
    }
  }
}
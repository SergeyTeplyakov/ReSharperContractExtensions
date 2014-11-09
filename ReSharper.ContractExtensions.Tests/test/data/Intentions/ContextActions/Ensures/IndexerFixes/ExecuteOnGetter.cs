using System.Diagnostics.Contracts;

class A
{
  public object this[string index]
  {
    g{caret}et
    {
      Contract.Requires(index != null);
      return new object();
    }
  }
}
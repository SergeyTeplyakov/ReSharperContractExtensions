using System.Diagnostics.Contracts;
using System;

abstract class A
{
  public ob{on}ject this[string index]
  {
    get
    {
      Contract.Requires(index != null);
      return new object();
    }
  }
}

abstract class A2
{
  public object this[string index]
  {
    ge{on}t
    {
      Contract.Requires(index != null);
      return new object();
    }
  }
}

abstract class A3
{
  public object this[string index]
  {
    get
    {
      Contract.Requires(index != null);
      ret{on}urn new object();
    }
  }
}

abstract class A4
{
  public i{on}nt? this[string index]
  {
    get
    {
      Contract.Requires(index != null);
      return new object();
    }
  }
}

abstract class B
{
  public ob{off}ject this[string index]
  {
    get
    {
      Contract.Ensures(Contract.Result<object>() != null);
      return new object();
    }
  }
}

abstract class B
{
  public in{off}t this[string index]
  {
    get
    {
      return 42;
    }
  }
}
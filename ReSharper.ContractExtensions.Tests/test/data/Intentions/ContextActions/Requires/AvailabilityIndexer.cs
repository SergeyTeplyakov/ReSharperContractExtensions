using System;
using System.Diagnostics.Contracts;

abstract class A
{  
  public object this[string inde{on}x]
  {
    get
    {
        return new object();
    }
  }
}

abstract class B
{  
  public object this[string inde{on}x]
  {
    set
    {
        return new object();
    }
  }
}

abstract class B2
{  
  public object this[string inde{on}x]
  {
    get
    {
        return new object();
    }
    set
    {}
  }
}

abstract class B3
{  
  public object this[string inde{on}x]
  {
    get
    {
        Contract.Requires(index != null);
        return new object();
    }
    set
    {}
  }
}

abstract class B3
{  
  public object this[string inde{on}x]
  {
    get
    {
        return new object();
    }
    set
    {
        Contract.Requires(index != null);
    }
  }
}

abstract class B4
{  
  public object this[string inde{off}x]
  {
    get
    {
        Contract.Requires(index != null);
        return new object();
    }
    set
    {
        Contract.Requires(index != null);
    }
  }
}

abstract class C
{  
  public object this[int inde{off}x]
  {
    get
    {
        return new object();
    }
    set
    {}
  }
}

abstract class C2
{  
  public object this[int inde{off}x]
  {
    get
    {
        return new object();
    }
  }
}

abstract class C3
{  
  public object this[int inde{off}x]
  {
    set
    {
        return new object();
    }
  }
}

abstract class D1
{  
  public int this[int index]
  {
    s{off}et
    {
        return 42;
    }
  }
}

abstract class D2
{  
  public string this[int index]
  {
    s{off}et
    {
        Contract.Requires(value != null);
        return "42";
    }
  }
}

abstract class D3
{  
  public string this[int index]
  {
    s{on}et
    {
        return "42";
    }
  }
}

abstract class D4
{  
  public string this[string index]
  {
    s{on}et
    {
        Contract.Requires(index != null);
        return "42";
    }
  }
}


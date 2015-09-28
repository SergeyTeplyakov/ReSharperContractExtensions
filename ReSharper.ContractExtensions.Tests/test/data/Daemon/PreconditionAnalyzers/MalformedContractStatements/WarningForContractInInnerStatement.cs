#define CONTRACTS_FULL

using System.Diagnostics.Contracts;

class A
{
  public void InsideIf(string s)
  {
    if (s != null)
      Contract.Requires(s != null);
  }

  public void EndContractBlockInsideIf(string s)
  {
    if (s != null)
      Contract.EndContractBlock();
  }

  public void InsideElse(string s)
  {
    if (s != null)
      Console.WriteLine();
    else
      Contract.Requires(s != null);
  }
  
  public void InsideLoops()
  {
    for(int n = 0; n < 10; n++)
      Contract.Requires(false);

    foreach(var n in Enumerable.Range(1, 10))
      Contract.Ensures(false);

    int n = 42;
    while(n > 0)
    {
      Contract.EnsuresOnThrow<System.Exception>(false);
      n--;
    }
  }

  public void InsideSwitch(string s)
  {
    switch(s)
    {
      case "foo":
        Contract.Requires(s != null);
        break;
    }
  }

  public void InTry(string s)
  {
    try
    {
    }
    catch(System.Exception e)
    {
      Contract.EndContractBlock();
    }
    finally 
    {
      Contract.Requires(false);
    }
  }

  public void InUsing(string s)
  {
    using (new System.IO.MemoryStream())
    {
      Contract.Requires(s != null);
    }
  }

  public void InChecked(string s)
  {
    checked
    {
        // Fine!
        Contract.Requires(s != null);
    }
  }

  public void WarningInTryBlock(string s)
  {
    try
    {
      // This tool will show another warning for this case!
      Contract.Requires(s != null);
    }
    finally {}
  }
}
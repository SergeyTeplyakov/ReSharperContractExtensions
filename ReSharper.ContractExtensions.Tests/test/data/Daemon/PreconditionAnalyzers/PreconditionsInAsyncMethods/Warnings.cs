using System;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

class A
{
  public async Task FooIfThrow(string s)
  {
    if (s == null) throw new ArgumentNullException("s");
  }

  public async Task FooWithLegacyCheck(string s)
  {
    if (s == null) throw new ArgumentNullException("s");
    Contract.EndContractBlock();
  }

  public async Task FooWithRequires(string s)
  {
    Contract.Requires(s != null);
  }

  protected async Task<int> FooIntWithRequires(string s)
  {
    Contract.Requires(s != null);
    return 42;
  }
}
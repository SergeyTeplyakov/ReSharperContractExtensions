using System.Threading.Tasks;
using System.Diagnostics.Contracts;

class A
{
  private async Task Foo(string s)
  {
    Contract.Requires(s != null);
  }

  public Task FooWithoutAsyncKeyword(string s)
  {
    Contract.Requires(s != null);
    throw new System.NotImplementedException();
  }
}
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

class A
{
  public Task Foo(string s)
  {
    Contract.Requires(s != null);
    return DoFoo(s);
  }

  private async Task DoFoo(string s)
  {
    await Task.Delay(42);
  }
}
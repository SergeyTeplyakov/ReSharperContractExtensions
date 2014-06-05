using System.Diagnostics.Contracts;

class A
{
  public void EnableOnSimpleMethod{on}(string s, string s2) {}
  public void EnableOnSimpleMethodWithDefaults{on}(string s, string s2, int n, int? nn, string s3 = null) {}
  
  public void DisabledOnArgument(string st{off}r) {}

  public void DisabledBecauseNoneArgsAvailable{off}(int n, string s = null) {}
  public void DisabledBecauseNoArgument{off}() {}
  public void DisabledBecauseAllArgsChecked{off}(string s, int? n)
  {
    Contract.Requires(s != null);
    Contract.Requires(n != null);
  }

  public string DisabledOnProperty{off} {set {}}
}
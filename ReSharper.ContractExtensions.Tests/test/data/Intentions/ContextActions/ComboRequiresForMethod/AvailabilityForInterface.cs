using System.Diagnostics.Contracts;

[ContractClass (typeof(IAContract))]
interface IA
{
  void EnabledOnAbstractMethod{on}(string s, int? n);

  void DisabledBecauseAllChecked{off}(string s, int? n);
  void DisabledBecauseNoArgument{off}();
  string DisabledOnProperty{off} {set;}
}

[ContractClassFor (typeof(IA))]
abstract class IAContract : IA
{
  void IA.EnabledOnAbstractMethod(string s, int? n) {}

  void IA.DisabledBecauseAllChecked(string s, int? n)
  {
    Contract.Requires(s != null);
    Contract.Requires(n != null);

    throw new System.NotImplementedException();
  }

  void IA.DisabledBecauseNoArgument() {}
  string IA.DisabledOnProperty {set {}}
}
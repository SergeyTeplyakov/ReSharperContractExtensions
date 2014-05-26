using System.Diagnostics.Contracts;

[ContractClass(typeof(IAContract))]
interface IA
{
  string EnabledOnAbstractMethodWithoutContract{on}(string s);

  string DisabledOnAbstractMethod{off}(string s);
}

[ContractClassFor(typeof(IA))]
abstract class IAContract : IA
{
  public string DisabledOnAbstractMethod(string s)
  {
    throw new System.NotImplementedException();
  }
}
using System.Diagnostics.Contracts;

[ContractClass(typeof(IAContract))]
interface IA
{
  public abstract void EnabledOnAbstractMethodWithoutContract(string s{on});

  public abstract void DisabledOnAbstractMethod(string s{off});
}

[ContractClassFor(typeof(IA))]
abstract class IAContract : IA
{
  public void DisabledOnAbstractMethod(string s)
  {
  }
}
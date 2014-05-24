using System.Diagnostics.Contracts;

[ContractClass(typeof (ISomeInterfaceContract))]
internal interface ISomeInterface{caret}
{
  void MethodWithPrecondition(string s);
}

[ContractClassFor(typeof (ISomeInterface))]
abstract class ISomeInterfaceContract : ISomeInterface
{
}
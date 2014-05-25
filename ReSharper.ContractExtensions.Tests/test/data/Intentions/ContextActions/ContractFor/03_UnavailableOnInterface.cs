using System.Diagnostics.Contracts;

[ContractClass(typeof(ISomeInterfaceClassContract))]
internal interface ISomeInterface{off}
{
  void MethodWithPrecondition(string s);
  string MethodWithPostcondition();
  string PropertyWithPostcondition { get; }
}

[ContractClassFor(typeof(ISomeInterface))]
internal class ISomeInterfaceClassContract : ISomeInterface
{
    void ISomeInterface.MethodWithPrecondition(string s)
    {
        throw new System.NotImplementedException();
    }

    string ISomeInterface.MethodWithPostcondition()
    {
        throw new System.NotImplementedException();
    }

    string ISomeInterface.PropertyWithPostcondition { get; private set; }
}
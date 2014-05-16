using System.Diagnostics.Contracts;

[ContractClass(typeof(SampleAbstractClassContract))]
internal abstract class SampleAbstractClass{off}
{
    public abstract void MethodWithPrecondition(string s);
    public abstract string MethodWithPostcondition();
    public abstract string PropertyWithPostcondition { get; }
}

[ContractClassFor(typeof(SampleAbstractClass))]
internal class SampleAbstractClassContract{off} : SampleAbstractClass
{
    public override void MethodWithPrecondition(string s)
    {
        throw new System.NotImplementedException();
    }

    public override string MethodWithPostcondition()
    {
        throw new System.NotImplementedException();
    }

    public override string PropertyWithPostcondition
    {
        get { throw new System.NotImplementedException(); }
    }
}
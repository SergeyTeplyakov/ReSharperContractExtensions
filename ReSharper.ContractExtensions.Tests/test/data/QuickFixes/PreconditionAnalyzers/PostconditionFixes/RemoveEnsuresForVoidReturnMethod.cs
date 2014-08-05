using System.Diagnostics.Contracts;

class A
{
  public void PostconditionInVoidMethod()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionInVoidMethod: Detected a call to Result with 'System.String', should be 'System.Void'.
     {caret}Contract.Ensures(Contract.Result<string>() != null);
  }
}
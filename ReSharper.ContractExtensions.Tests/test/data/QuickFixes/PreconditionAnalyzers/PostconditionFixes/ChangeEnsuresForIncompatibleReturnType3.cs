using System.Diagnostics.Contracts;

class A
{
  public object PostconditionWithTwoDerivedTypes()
  {
     // I know that this fix will break compilation, but anyway it should be useful!
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     {caret}Contract.Ensures(Contract.Result<string>() != null && Contract.Result<string>().Length != 0);
     throw new NotImplementedException();
  }
}
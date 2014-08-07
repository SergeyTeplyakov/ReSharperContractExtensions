using System.Diagnostics.Contracts;
using System.Threading.Tasks;

class A
{
  public void PostconditionInVoidMethod()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionInVoidMethod: Detected a call to Result with 'System.String', should be 'System.Void'.
     Contract.Ensures(Contract.Result<string>() != null);
  }

  public object PostconditionWithDerivedType()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<string>() != null);
     throw new NotImplementedException();
  }

  public object PostconditionWithTwoDerivedTypes()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<string>() != null && Contract.Result<string>().Length != 0);
     throw new NotImplementedException();
  }

  public object PostconditionWithOneWrongType()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<object>() != null && Contract.Result<string>().Length != 0);
     throw new NotImplementedException();
  }

  public object PostconditionWithTwoDifferentTypes()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<string>() != null && Contract.Result<System.DateTime>() != null);
     throw new NotImplementedException();
  }


  public int PostconditionWithUnrelatedType1()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<string>() != null);
     throw new NotImplementedException();
  }

  public System.StringBuilder PostconditionWithUnrelatedType2()
  {
     // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
     Contract.Ensures(Contract.Result<string>() != null);
     throw new NotImplementedException();
  }

  public object PostconditionWithProperty
  {
     get
     {
       // error CC1002: In method CodeContractInvestigations.Postconditions.PostconditionWithDerivedType: Detected a call to Result with 'System.String', should be 'System.Object'.
       Contract.Ensures(Contract.Result<string>() != null);
       throw new NotImplementedException();
     }
  }

  public string PostconditionWithBaseType()
  {
    // OK
    Contract.Ensures(Contract.Result<object>() != null);
    throw new NotImplementedException();
  }

  public Task<string> EnsuresOnTask()
  {
    // OK
    Contract.Ensures(Contract.Result<string>() != null);
    throw new NotImplementedException();
  }

  public async Task<string> EnsuresOnAsyncTask()
  {
    // OK
    Contract.Ensures(Contract.Result<string>() != null);
    throw new NotImplementedException();
  }
}
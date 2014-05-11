
abstract class A
{
  public System.String EnabledReferenceTypeResult()
  {
    // Contract.Result contains type name without namespace! So this code will not compile!
    Contract.Ensures(Contract.Result<String>() != null);
    return "";
  }

}
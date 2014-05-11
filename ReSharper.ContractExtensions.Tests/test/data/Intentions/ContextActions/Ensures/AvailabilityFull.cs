using System;
using System.Diagnostics.Contracts;

abstract class A
{
  public string{on} EnabledOnReferenceTypeResult() { return ""; }
  public str{on}ing EnabledWhenCaretIsDirectlyOnTheReturnType() { return ""; }
  public string EnabledW{on}henCaretIsOnMethodType() { return ""; }

  public int?{on} EnabledOnNullableValueTypeResult() { return 42; }
  
  public string DisabledWhenCarretNotOnResult({on}) {return "";}

  public int{off} DisabledOnValueTypeResult() { return 42; }
  public void{off} DisabledOnVoidResult() {}

  public abstract string{off} DisabledOnAbstractMethod();
  
  public string EnabledIfEnsuresIsInvalid()
  {
    Contract.Ensures(Contract.Result<object>() != null);
  }
  
  public string{off} DisabledIfEnsuresAlreadyExists()
  {
    Contract.Ensures(Contract.Result<string>() != null);
    return "";
  }

  public string EnabledOnPropertyGett{on}er
  {
        get {return "";}
  }
    
  public string EnabledReturnStatement()
  {
        retu{on}rn "";
  }

  public string DisabledOnAutoPropert{off}y {get; set;}

  public string DisabledOnNonReadableProper{off}ty {set {}}

  public string DisabledInsideMethod()
  {
    {off}var f = "";
    return "foo";
  }

}
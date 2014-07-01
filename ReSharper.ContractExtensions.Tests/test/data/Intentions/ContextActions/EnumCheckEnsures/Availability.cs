using System;
using System.Diagnostics.Contracts;

enum Foo
{
  Value1,
}

abstract class A
{
  Foo{on} EnableOnCustomEnum()
  {
    throw new System.NotImplementedException();
  }

  Foo?{on} EnableOnCustomNullableEnum()
  {
    throw new System.NotImplementedException();
  }

  System.Reflection.BindingFlags{on} EnableOnDotNetEnum()
  {
    throw new System.NotImplementedException();
  }

  System.Reflection.BindingFlags{off} DisabledBecauseAlreadyChecked()
  {
    Contract.Ensures(Enum.IsDefined(typeof(System.Reflection.BindingFlags), Contract.Result<System.Reflection.BindingFlags>()));
    throw new System.NotImplementedException();
  }

  System.Reflection.BindingFlags?{off} DisabledBecauseAlreadyCheckedForNullable()
  {
    Contract.Ensures(Enum.IsDefined(typeof(System.Reflection.BindingFlags), Contract.Result<System.Reflection.BindingFlags>()));
    throw new System.NotImplementedException();
  }

}
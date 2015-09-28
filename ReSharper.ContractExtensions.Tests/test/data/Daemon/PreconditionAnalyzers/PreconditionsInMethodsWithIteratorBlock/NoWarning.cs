#define CONTRACTS_FULL
using System.Collections.Generic;
using System.Diagnostics.Contracts;

class A
{
    private IEnumerable<object> Foo(string s)
    {
        Contract.Requires(s != null);
        return null;
    }
}
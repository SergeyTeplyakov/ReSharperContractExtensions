namespace ReSharper.ContractExtensions.ContractsEx
{
    public enum ContractExpressionType
    {
        ContractRequires,
        ContractEnsures,
        ContractInvariant,
        ContractAssert,
        ContractAssume,
        IfThrowPrecondition,
        GuardBasedPrecondition,
    }
}
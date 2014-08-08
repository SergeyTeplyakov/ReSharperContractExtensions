R# support for Code Contracts 
----------------------------------

Are you using Code Contracts library in your project? Do you want to simplify some basic steps, like adding preconditions, postconditions or object invariant? Maybe you hate current Code Contract approach for adding assertions to the abstract classes and interfaces. If you fond of Design by Contract but hate typing, this R# plug-in is what you need.

### Download

Currently supported ReSharper versions are 8.0-8.2

This plugin is available for download in [ReSharper extensions gallery](https://resharper-plugins.jetbrains.com/packages/ReSharper.ContractExtensions/0.8.0);

### Support for Code Contract Compiler warnings and errors inside the editor

1. Inconsistent preconditions visibility
 - Warn on Contract.Requires&lt;CustomException&gt;() when CustomException does not have ctor(string) or ctor(string, string) (#4).
 - Warn on Contfact.Requires&lt;CustomException&gt;() when CustomException is less visible then the enclosing method (#3).
 - Error when Contract.Requires less visible members in the predicate than the enclosing method.

2. Errors for malformed method contracts
 - Error for Requires/Ensures after EndContractBlock (#21)
 - Error for Requires/Ensures in the middle of the method (#26, #22)
 - Error when Ensures placed before Requires (#29, #9)
 - Error for calling void-return method in the contract block (#5)
 - Warning for calling non-void return method in the contract block (#25)
 - Error on method calls as a source of contract error message (Contract.Requires(false, GetMessage())
 - Error on non-static internal strings used as a source of contract error message (#2)
 - Error on assignments in the contract block (#9)
 - Error on Assert/Assume calls in the contract block (#6)
 - Error on using Requires/Ensures in the try block (#10)
 - Warning on redundant EndContractBlock (for instance used after Contract.Requires/Ensures).

3. Postcondition checks
 - Error for inconsistent method return type with Contract.Result&lt;T&gt; (#34)

4. Other features
 - Generated contract classes would be marked with ExlucdeFromCodeCoverageAttribute if this option is enabled in the plug-in options page. (#31)
 - Warn on redundant Contract.Requires on nullable arguments

### Basic use cases

#### Add preconditions

![](https://raw.githubusercontent.com/SergeyTeplyakov/ReSharperContractExtensions/master/Content/Requires_avi.gif)

#### Add postconditions

![](https://raw.githubusercontent.com/SergeyTeplyakov/ReSharperContractExtensions/master/Content/Postcondition_avi.gif)

#### Add object invariants for fields or properties

![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/Invariant_avi.gif)

#### Add contract class for interface (the same feature exists for abstract classes)

![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/Interface_avi.gif)

#### “Combo” actions for abstract class (the same feature exists for interfaces)

![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/AbstractClassCombo_avi.gif)

#### Convert null-check preconditions from if-throw to Contract.Requires (new in v.0.7.5)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/075%20-%20ConvertPreconditionANE.gif)

#### Convert range check from if-throw to Contract.Requires (new in v.0.7.5)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/075%20-%20ConvertPreconditionWithAOR.gif)

#### Add precondition check for enum argument (new in v.0.7.5)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/075%20-%20EnumRequires.gif)

#### Add postcondition check for enum argument (new in v.0.7.5)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/075%20-%20EnsureEnums.gif)

#### Add preconditions for all arguments (new in v.0.7)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/07_MethodCombo.gif)

#### Check string arguments with string.IsNullOrEmpty (new in v.0.7)
(Note, this feature should be turned on at plug-in settings page, available at R# Options)
![](https://github.com/SergeyTeplyakov/ReSharperContractExtensions/raw/master/Content/08_StringCheck.gif)



### What next for you?
Download sources and play with them or
Download this plug-in via R# Gallery - https://resharper-plugins.jetbrains.com/packages/ReSharper.ContractExtensions/0.7.51

### What next for me?
0. Add support for R# 8.0 and 8.1 (Done)

1. Add quick fixes and code analysis rules that will show some common errors in the IDE directly, instead of waiting for compilation time.

2. Add support for Intelli Sense and show contract in the tool tips (I know about “Code Contracts Editor Extension” but it never worked for me).
P.S. This tool was written with the previous version of this extension. So, if you want to look at the sample code with a lot of contracts, this source of the project could be useful as well.

P.S. This tool was written with the previous version of this extension. So, if you want to look at the sample code with a lot of contracts, this source of the project could be useful as well.

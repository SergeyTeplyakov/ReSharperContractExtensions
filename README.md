R# support for Code Contracts 
----------------------------------

Are you using Code Contracts library in your project? Do you want to simplify some basic steps, like adding preconditions, postconditions or object invariant? Maybe you hate current Code Contract approach for adding assertions to the abstract classes and interfaces. If you fond of Design by Contract but hate typing, this R# plug-in is what you need.

### Download

Currently supported ReSharper versions are 8.2

This plugin is available for download in [ReSharper extensions gallery](https://resharper-plugins.jetbrains.com/packages/ReSharper.ContractExtensions/0.5.0);

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


### What next for you?
Download sources and play with them or
Download this plug-in via R# Gallery - https://resharper-plugins.jetbrains.com/packages/ReSharper.ContractExtensions/0.5.0

### What next for me?
0. Add support for R# 8.0 and 8.1

1. Add quick fixes and code analysis rules that will show some common errors in the IDE directly, instead of waiting for compilation time.

2. Add support for Intelli Sense and show contract in the tool tips (I know about “Code Contracts Editor Extension” but it never worked for me).
P.S. This tool was written with the previous version of this extension. So, if you want to look at the sample code with a lot of contracts, this source of the project could be useful as well.

P.S. This tool was written with the previous version of this extension. So, if you want to look at the sample code with a lot of contracts, this source of the project could be useful as well.

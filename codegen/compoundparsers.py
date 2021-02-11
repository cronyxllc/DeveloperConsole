import argparse
from codegen import *

# --- CONFIGURATION ---
kNamespaceName = "Cronyx.Console.Parsing"
kImports = [
    "System",
    "System.Collections.Generic"
]
kResultType = "TResult"
kResultMethod = "GetResult"
kGroupingCharsMethod = "GroupingChars"
kSeperatorMethod = "Seperator"
kClassInheritName = "CompoundParser"
kClassName = "CompoundParser"

kOverrideResultMethod = "GetResult"
kOverrideTypesMethod = "GetTypes"

bTokenResult = f"<typeparamref name=\"{kResultType}\"/>"
bTokenParameterParser = f"<see cref=\"ParameterParser{{T}}\"/>"

# --- DOCUMENTATION TEXT ---

kTokenNumber = "%NUMBER%"               # Replaces with 'one', 'two', 'three', etc. depending on the number
kTokenNumberAdj = "%NUMBERADJ%"         # Replaces with 'first', 'second', 'third,' etc. depending on the number
kTokenAddPlural = "%PLURAL%"            # Replaces with 's' depending on whether or not the number is plural
kTokenExampleFormat = "%EXAMPLE%"       # Replaces with '[T0],' '[T0 T1],' '[T0 T1 T2],' etc. depending on the number
kTypeTokenList = "%TYPETOKENLIST%"      # Replaces with '<typeparam name="T0"/>,' '<typeparam name="T0"/>, <typeparam name="T1"/>,' etc depending on the number
kTypeToken = "%TYPETOKEN%"              # Replaces with '<typeparam name="T0"/>', '<typeparam name="T1"/>,' etc. depending on the number

kClassSummary = f'''<summary>
A {bTokenParameterParser} that produces a {bTokenResult} object from %NUMBER% parameter%PLURAL%.
</summary>
'''

kClassRemarks = f'''<remarks>
<para>By default, this parser will parse an object with the following format:</para>
<code>
%EXAMPLE%
</code>
<para>and will return a {bTokenResult} object by passing the constituent parameters (%TYPETOKENLIST%) to <c>{kResultMethod}</c>.</para>
<para>By default, the grouping symbols used are parenthesis '()' and brackets '[],' but these can be changed by overriding <c>{kGroupingCharsMethod}</c> in a derived class.
Elements can be optionally seperated from one another using a comma ','. This seperator character can be changed by overriding <c>{kSeperatorMethod}</c> in a derived class.</para>
</remarks>
'''

kClassTypeParamRef = "The %NUMBERADJ% parameter type in this compound type."
kClassTypeResultRef = f"The type that this {bTokenParameterParser} produces."

kMethodResultSummary = '''<summary>
Constructs a <typeparamref name="TResult"/> object from its constituent types.
</summary>
'''

kMethodParamRef = "The %NUMBERADJ% parameter, whose type is %TYPETOKEN%"
kMethodReturnRef = f"A {bTokenResult} object."

# --- OUTPUT FILE ---

output = "../console/Packages/com.cronyx.console/Runtime/Scripts/Parsing/CompoundParserAutoGen.cs"

parser = argparse.ArgumentParser(description='Generates generic derived classes for CompoundParser<T>')
parser.add_argument('-o', '--output', type=argparse.FileType('w'), default=output,
                    help='Where to store the generated output')
parser.add_argument('-n', '--number', type=int, default = 20,
                    help='Number of generic classes to generate')
args = parser.parse_args()

def replaceStringGeneric (string: str, nargs: int):
    string = string.replace(kTokenNumber, numbers[nargs])
    string = string.replace(kTokenNumberAdj, numbersadj[nargs])
    string = string.replace(kTokenAddPlural, "s" if nargs > 1 else "")
    string = string.replace(kTypeToken, f"<typeparamref name=\"T{nargs-1}\"/>")

    # Create example string
    eg = "["
    for i in range(0, nargs):
        eg += f"T{i}"
        if i < nargs - 1: eg += " "
    eg += "]"
    string = string.replace (kTokenExampleFormat, eg)

    # Create type token list
    typetokenlist = ""
    maxTokens = min (3, nargs)
    for i in range(0, maxTokens):
        typetokenlist += f"<typeparamref name=\"T{i}\"/>"
        if i < maxTokens - 1: typetokenlist += ", "
    if nargs > maxTokens: typetokenlist += ", etc."
    string = string.replace(kTypeTokenList, typetokenlist)
    return string


def generateClass (indent: int, nargs: int):
    # Write summary:
    for line in kClassSummary.splitlines(): docline(replaceStringGeneric(line, nargs), indent)

    # Write remarks:
    for line in kClassRemarks.splitlines(): docline(replaceStringGeneric(line, nargs), indent)

    # Write type param doc references
    for i in range (nargs):
        docline(f"<typeparam name=\"T{i}\">{replaceStringGeneric(kClassTypeParamRef, i+1)}</typeparam>", indent)

    # Write result type param
    docline(f"<typeparam name=\"{kResultType}\">{replaceStringGeneric(kClassTypeResultRef, nargs)}</typeparam>", indent)

    # Generate class signature
    # First get list of generic type arguments
    typeargs = ""
    for i in range(nargs):
        typeargs += f"T{i}, "
    typeargs += kResultType # Add result type to end of typeargs
    writeline (f"public abstract class {kClassName}<{typeargs}> : {kClassInheritName}<{kResultType}>", indent)

    writeline ("{", indent) # Open class brace

    # Generate overriden get result method
    # First generate method body
    overrideResultBody = ""
    overrideResultBody += f"{kResultMethod}("
    for i in range (nargs):
        overrideResultBody += f"(T{i}) elements[{i}]"
        if i != nargs - 1: overrideResultBody += ", "
    overrideResultBody += ");"

    writeline(f"protected sealed override {kResultType} {kOverrideResultMethod}(object[] elements)", indent + 1)
    writeline(f"=> {overrideResultBody}", indent + 2)
    writeline()

    # Generate overriden get types method
    # First generate method body
    overrideTypesBody = "new[] { " 
    for i in range (nargs):
        overrideTypesBody += f"typeof(T{i})"
        if i != nargs - 1: overrideTypesBody += ","
        overrideTypesBody += " "
    overrideTypesBody += "};"

    writeline(f"protected sealed override IEnumerable<Type> {kOverrideTypesMethod}()", indent + 1)
    writeline(f"=> {overrideTypesBody}", indent + 2)
    writeline()

    # Generate abstract get result method
    # Start with documentation
    for line in kMethodResultSummary.splitlines(): docline(replaceStringGeneric(line, nargs), indent+1)
    for i in range (nargs): docline(f"<param name=\"t{i}\">{replaceStringGeneric(kMethodParamRef, i+1)}</param>", indent+1)
    docline(f"<returns>{kMethodReturnRef}</returns>", indent+1)

    # Now create method signature
    params = ""
    for i in range (nargs):
        params += f"T{i} t{i}"
        if i != nargs - 1: params += ", "
    writeline(f"protected abstract {kResultType} {kResultMethod}({params});", indent+1)

    writeline ("}", indent) # Close class brace

    writeline() # Space between classes

# Write header for this autogen
writeheader(args.output.name, f"Contains generic implementations of {kClassInheritName} for an arbitrary number of type parameters.")

# Write imports
for namespace in kImports:
    writeline(f"using {namespace};")
writeline()

# Namespace name
writeline(f"namespace {kNamespaceName}")
writeline("{") # Open namespace

# Generate generic classes
for n in range(1, args.number + 1):
    generateClass(1, n)

writeline("}") # Close namespace

# Log to output file
writeToOutput(args.output)


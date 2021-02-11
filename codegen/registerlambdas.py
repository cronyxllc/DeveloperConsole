import argparse
from codegen import *

# --- DEFAULT OUTPUT ---
output = "../console/Packages/com.cronyx.console/Runtime/Scripts/DeveloperConsole_GenericRegistration.cs"

# --- CONFIGURATION ---
kPartialClassName = "DeveloperConsole"
kNamespace = "Cronyx.Console"
kImports = [
    "System"
]

kMethodName = "RegisterCommand"
kMethodNameToInvoke = "RegisterCommand"
kParameterName = "name"
kParameterCommand = "command"
kParameterDescription = "description"

# --- DOCUMENTATION ---
kDocMethodSummary = '''<summary>
Registers a console command that will parse %NUMBER% argument%PLURAL% and invoke a delegate.
</summary>'''
kDocTypeParam = f"The type of the %NUMBERADJ% parameter passed to <paramref name=\"{kParameterCommand}\"/>"
kDocParameterName = "A unique name for this command. Cannot be null or whitespace." 
kDocParameterCommand = "A delegate that is invoked when the command is called from the console."
kDocParameterDescription = "A short, optional description of the command that appears in a list of all commands."
kDocExceptions = {
    "ArgumentException": f"Thrown if <paramref name=\"{kParameterName}\"/> is null or empty, or if <paramref name=\"{kParameterCommand}\"/> is null.",
    "InvalidOperationException": f"Thrown if <paramref name=\"{kParameterName}\"/> is taken by another command."
}

# --- SPECIAL VALUES ---
kTokenNumber = "%NUMBER%"                   # Replaces with 'one,' 'two,' 'three,' etc. depending on the number
kTokenNumberAdj = "%NUMBERADJ%"             # Replaces with 'first,' 'second, 'third,' etc. depending on the number
kTokenAddPlural = "%PLURAL%"                # Replaces with 's' if the number is greater than 1 or equal to zero

def replaceSpecialValues (raw: str, nargs: int):
    raw = raw.replace(kTokenNumber, numbers[nargs])
    raw = raw.replace(kTokenNumberAdj, numbersadj[nargs])
    raw = raw.replace(kTokenAddPlural, "" if nargs == 1 else "s")
    return raw

def generateMethod (nargs: int, indent=1):
    # Write summary documentation
    for line in replaceSpecialValues(kDocMethodSummary, nargs).splitlines(): docline(line, indent)

    # Write all type params documentation
    for i in range(nargs):
        docline(f"<typeparam name=\"T{i}\">{replaceSpecialValues(kDocTypeParam, i+1)}</typeparam>", indent)

    # Write parameter documentations
    docline(f"<param name=\"{kParameterName}\">{kDocParameterName}</param>", indent)
    docline(f"<param name=\"{kParameterCommand}\">{kDocParameterCommand}</param>", indent)
    docline(f"<param name=\"{kParameterDescription}\">{kDocParameterDescription}</param>", indent)

    # Write exception documentations
    for exception in kDocExceptions:
        docline(f"<exception cref=\"{exception}\">{kDocExceptions[exception]}</exception>", indent)

    # Generate generic type parameter list
    paramList = ""
    if (nargs > 0):
        paramList = "<"
        for i in range(nargs):
            paramList += f"T{i}"
            if i != nargs - 1: paramList += ", "
        paramList += ">"
    
    # Write method signature
    writeline(f"public static void {kMethodName}{paramList}(string {kParameterName}, Action{paramList} {kParameterCommand}, string {kParameterDescription}=null)", indent)
    
    # Write method body
    writeline(f"=> {kMethodNameToInvoke}({kParameterName}, (Delegate){kParameterCommand}, {kParameterDescription});", indent+1)

    # Create space after method
    writeline()

parser = argparse.ArgumentParser(description='Generates generic methods for RegisterCommand')
parser.add_argument('-o', '--output', type=argparse.FileType('w'), default=output,
                    help='Where to store the generated output')
parser.add_argument('-n', '--number', type=int, default = 16,
                    help='Number of generic methods to generate')
args = parser.parse_args()

# Write header for this autogen
writeheader(args.output.name, f"Contains generic implementations of {kMethodName} for an arbitrary number of generic type parameters.")

# Write imports
for namespace in kImports:
    writeline(f"using {namespace};")
writeline()

# Namespace name
writeline(f"namespace {kNamespace}")
writeline("{") # Open namespace

# Class name
writeline(f"public partial class {kPartialClassName}", 1)
writeline("{", 1) # Open class

# Generate methods
for n in range(0, args.number + 1):
    generateMethod(n, 2)

writeline("}", 1) # Close class

writeline("}") # Close namespace

# Log to output file
writeToOutput(args.output)



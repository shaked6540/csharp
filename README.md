# csharp
Run c# code from the command line, like python or javascript. 

![Gif](https://i.imgur.com/khxe5cD.gif "Gif")


# Features
- Run c# code form the command line
- No semicolons needed
- Load external assemblies
- Save scripts
- Single executeable, ready to run out of the box
- Supports Windows, Linux, macOS, and Android (Instructions to install on android will be at the bottom)
## Built-in functions
### help
`help` is a function that takes any object or a [`Type`](https://docs.microsoft.com/en-us/dotnet/api/system.type?view=netcore-3.1) and prints all its methods, properties, fields and events. It's useful because there is no auto completion in the command line, and sometimes its faster to just type `help(typeof(int))` than to open the documentaion.
![img](https://i.imgur.com/x4QSEXh.png)
### variables
`variables` is a function that prints all the variables that exist in the current script session and their values.
![img](https://i.imgur.com/B9XqmBW.png)


# Installing
You don't need to do anything special to install `csharp`, just grab the latest version from the [releases](https://github.com/shaked6540/csharp/releases) page or build it by yourself and drop it in a folder. I recommend adding the executable location to `PATH`.

[Here is how to add a value to `PATH` on Linux](https://unix.stackexchange.com/a/26059)

[Here is how to add a value to `PATH` on Windows](https://docs.telerik.com/teststudio/features/test-runners/add-path-environment-variables)

# Configuring

`csharp` can be configured using a config file, simply place a 'Config.json' file in the same folder as the exectueable and add the following properties
Note: you don't need to add all of them, they are all optional 

- Assemblies: An array of strings containing file names of assemblies that will be loaded when `csharp` runs. Usually, there will be a bunch of dll files here. The assemblies must be in the same folder as the executeable
- Namespaces: An array of strings containing names of namespaces that will be included when running `csharp`. This is equivalent to "using x;"
- BeforeScriptFile: A string containing a path to file that will be loaded before user input. This is useful if you want to exectue something every time you start `csharp`, for example, creating a variable that holds the names of all the files in the current directory
- SaveScripts: A boolean to indicate whether the running code will be saved to a .txt file or not.
- SavedScriptsLocation: A string containing a path to a folder where the scripts will be saved. If this property is missing but `SaveScripts` is set to true, it defaults to a folder called `Scripts` which will be created where the exectueable resides
- NewlinePrefix: A string indicating what prefix will be displayed when `csharp` prompts a user input. The default value is `"> "`
- MultilineCodeKeyword: A string indicating what value, when entered, will wait until the `EndKeyword` is inserted before exectuing the code. This means you will be able to insert as many new lines as you want without executing the code until `EndKeyword` is entered. The default value is `lines` 
- EndKeyword: A string indicating what value, when entered, will execute the code that was writtin since `MultilineCodeKeyword` was entered. If `MultilineCodeKeyword` was not entered prior to using this keyword, the program will exit. The default value is `end`



An example of a full `Config.json` file:
```json
{
    "Assemblies": ["MoreLinq.dll"],
    "Namespaces": ["MoreLinq","System.Text.Json"],
    "BeforeScriptFile": "D:\\csharp\\BeforeScript.cs",
    "SaveScripts": true,
    "SavedScriptsLocation": "D:\\csharp\\Scripts",
    "NewlinePrefix": "> ",
    "MultilineCodeKeyword": "lines",
    "EndKeyword": "end"
}
```

An example of a `BeforeScript.cs` (Note: semicolons are optional here too!) : 
```cs
public static void cw(object o) => Console.WriteLine(o);
var files = Directory.GetFiles(Directory.GetCurrentDirectory());
class MyClass
{
    public int myInt
    public string myString
    
    public MyClass(int myInt, string myString) 
    {
        this.myInt = myInt
        this.myString = myString
    }
    
    public void Print() 
    {
        cw($"{myInt} - {myString}")
    }
    
}

var myClass = new MyClass(5, "Hello")
myClass.Print()
```

# Building
1. Clone the project using git
2. Open a commandline and run `dotnet publish -c Release --self-contained false -p:PublishSingleFile=true -r win-x64`.
Replace `--self-contained false` to `--self-contained true` if you wish to build a self contained version of the program.
Replace `win-x64` with your platform if you build for an OS other than windows.

# Installing on Android
1. Install [termux](https://termux.com/). 
2. Install ubunutu using [this guide](https://github.com/MFDGaming/ubuntu-in-termux)
3. (Optional) If you want to use the framework dependent build of the program, install .NET Core 3.1 from [here (32bit)](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.302-linux-arm32-binaries) or [here (64bit)](https://dotnet.microsoft.com/download/dotnet-core/thank-you/sdk-3.1.302-linux-arm64-binaries) and follow the instructions provided
4. Thats it! run the program. I recommend adding the executeable location to `PATH` so you don't need to navigate to it every time. I also recommend adding the `startubuntu.sh` script to `PATH` or create an alias for it in `.bashrc`

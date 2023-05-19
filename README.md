# Build/Run Instructions
1. Download and install **Visual Studio** (in the current example the **Visual Studio 2022** version is the one used):
   + https://visualstudio.microsoft.com/downloads/

2. Open a terminal window in the current root folder (`...\c-sharp-sample`):
    <ol type="a">
    <li>Use the <img src="https://i.postimg.cc/sxyF8VSD/windows-key.png"/> + <code>R</code> shortcut</li>
    <li>Type <code>cmd</code></li>
    <li>Write <code>cd <strong>FOLDER_PATH</strong></code>, being <code><strong>FOLDER_PATH</strong></code> the absolute path to the <code>c-sharp-sample</code> root folder</li>
    </ol>
3. Execute the <code><strong>"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\Roslyn\\csc.exe"</strong> -reference:<strong>XX-bit</strong>/plux.dll <strong>PROGRAM.cs</strong> -out:"<strong>XX-bit</strong>/Program.exe"</code> command.

> **NOTE**
> + The **"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"** path to the **csc.exe** executable may vary in accordance to the **Visual Studio** version being used;
> + The **XX-bit** folder can be **32-bit** or **64-bit** in accordance with the operating system architecture;
> + **PROGRAM.cs** should be replaced by the name of the program to be built. Taking into consideration the content of the root folder the **Program.cs** | **Program_biosignalsPluxPro.cs** | **Program_fnirsExplorer.cs** are three valid values.
4. In the same terminal window, run the **Program.exe** file generated during the build by providing your **Device MAC-Address** as an input parameter and defining the log file: <code>"<strong>XX-bit</strong>/Program.exe" BTH58:8E:81:A2:41:A6 > "log.txt"</code>
5. When the output is intended to be directly printed into the terminal, the <code>> "log.txt"</code> instructions can be removed from the command aforementioned in **Step 4**.

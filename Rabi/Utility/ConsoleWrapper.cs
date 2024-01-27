using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ConsoleWrapper : IConsole
{
    public string ReadLine() => Console.ReadLine();

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }
}
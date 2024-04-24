using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UcciLox.Exceptions;
using UcciLox.Statements;
using UcciLox.Tokens;

namespace UcciLox;

internal class Options
{
    [Option('s', "script", HelpText = "Lox script to run.")]
    public string? Script { get; set; }

    [Option("gen-ast", HelpText = "Generate AST.")]
    public bool? GenerateAst { get; set; }

    [Option("run-ast", HelpText = "Run AST.")]
    public string? RunAst { get; set; }

    public bool HasOptions() => Script != null || GenerateAst != null || RunAst != null;
}

internal class Program
{
    private static readonly bool _hasError = false;
    private static bool _hadRuntimeError = false;
    private static readonly Interpreter _interpreter = new();

    private static void Main() => RunFile("./LoxScripts/loadlib.lox");

    private static void RunFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path));
        }

        var sourceText = File.ReadAllText(path);
        if (sourceText == null || sourceText.Length == 0)
        {
            Error(0, $"Error reading file {path}");
            return;
        }

        var statements = ParseSource(sourceText);

        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        _interpreter.Interpret(statements);

        stopwatch.Stop();
        Console.WriteLine("\r\n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Ran script in {stopwatch.Elapsed.TotalSeconds} seconds.");
        Console.ResetColor();

        if (_hasError || _hadRuntimeError)
            Environment.Exit(-1);
    }

    private static List<Statement> ParseSource(string source)
    {
        Scanner scanner = new(source);

        List<Token> tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        var statements = parser.Parse();

        return statements;
    }

    internal static void Error(int line, string message) => Report(line, string.Empty, message);

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[Line {line}] Error {where}: {message}"); ;
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine("[line " + error.token?.Line + "] " + error.Message);
        _hadRuntimeError = true;
    }
}
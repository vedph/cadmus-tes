using Cadmus.Core.Storage;
using Cadmus.Tes.Services;
using Spectre.Console;
using System;

namespace Tes.Cli.Services;

internal static class CommandHelper
{
    public static ICadmusRepository GetCadmusRepository(string connStr)
    {
        TesRepositoryProvider provider = new()
        {
            ConnectionString = connStr
        };
        return provider.CreateRepository();
    }

    public static void DisplayException(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
        Exception? inner = ex.InnerException;
        while (inner != null)
        {
            AnsiConsole.MarkupLineInterpolated($"- [red]{inner.Message}[/]");
            inner = inner.InnerException;
        }
        AnsiConsole.MarkupLineInterpolated($"[yellow]{ex.StackTrace}[/]");
    }
}

using System;
using System.Threading;

public static class ConsoleHelper
{
    public static CancellationTokenSource BindToCancelKey()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.Out.WriteLine("Stopping");
            cancellationTokenSource.Cancel(false);
        };
        return cancellationTokenSource;
    }
}
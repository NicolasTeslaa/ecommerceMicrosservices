using System.Diagnostics;

var stripeExecutable = Environment.GetEnvironmentVariable("STRIPE_CLI_PATH");
if (string.IsNullOrWhiteSpace(stripeExecutable))
{
    stripeExecutable = "stripe";
}

var forwardTo = Environment.GetEnvironmentVariable("STRIPE_FORWARD_TO");
if (string.IsNullOrWhiteSpace(forwardTo))
{
    forwardTo = "http://localhost:5120/api/payments/webhooks/stripe";
}

var events = Environment.GetEnvironmentVariable("STRIPE_EVENTS");
if (string.IsNullOrWhiteSpace(events))
{
    events = "payment_intent.succeeded,payment_intent.payment_failed";
}

Console.WriteLine($"[{Timestamp()}] Stripe webhook listener starting...");
Console.WriteLine($"[{Timestamp()}] Forwarding events to {forwardTo}");

var startInfo = new ProcessStartInfo
{
    FileName = stripeExecutable,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false
};

startInfo.ArgumentList.Add("listen");
startInfo.ArgumentList.Add("--events");
startInfo.ArgumentList.Add(events);
startInfo.ArgumentList.Add("--forward-to");
startInfo.ArgumentList.Add(forwardTo);

using var stripeProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

stripeProcess.OutputDataReceived += (_, args) =>
{
    if (!string.IsNullOrWhiteSpace(args.Data))
    {
        Console.WriteLine(args.Data);
    }
};

stripeProcess.ErrorDataReceived += (_, args) =>
{
    if (!string.IsNullOrWhiteSpace(args.Data))
    {
        Console.Error.WriteLine(args.Data);
    }
};

try
{
    if (!stripeProcess.Start())
    {
        Console.Error.WriteLine($"[{Timestamp()}] The Stripe CLI process could not be started.");
        Environment.ExitCode = 1;
        return;
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[{Timestamp()}] Failed to start Stripe CLI.");
    Console.Error.WriteLine($"[{Timestamp()}] Make sure the Stripe CLI is installed and available in PATH, or set STRIPE_CLI_PATH.");
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
    return;
}

stripeProcess.BeginOutputReadLine();
stripeProcess.BeginErrorReadLine();

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

try
{
    await stripeProcess.WaitForExitAsync(cancellation.Token);
    Environment.ExitCode = stripeProcess.ExitCode;
}
catch (OperationCanceledException)
{
    if (!stripeProcess.HasExited)
    {
        stripeProcess.Kill(entireProcessTree: true);
        await stripeProcess.WaitForExitAsync();
    }
}

static string Timestamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

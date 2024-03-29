﻿using System.Data.Common;
using System.Diagnostics;
using Microsoft.Extensions.DiagnosticAdapter;
using Prometheus;

namespace TownSuite.Web.SSV3Adapter.Prometheus;

// https://bartwullems.blogspot.com/2021/01/using-systemdiagnosticdiagnosticsource_6.html
// https://bartwullems.blogspot.com/2021/01/using-systemdiagnosticdiagnosticsource.html
// https://sudonull.com/post/3671-Using-the-DiagnosticSource-in-NET-Core-Theory
// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.diagnosticlistener?view=net-6.0

public sealed class SqlClientObserver : IObserver<DiagnosticListener>
{
    private static Gauge _sqlTotal;
    private static Histogram _sqlDuration;
    private readonly List<IDisposable> _listeners = new();


    private readonly AsyncLocal<Stopwatch> _stopwatch = new();

    public SqlClientObserver(string prefix = "")
    {
        _sqlTotal = Metrics
            .CreateGauge($"{prefix}sql_commands_executed_total",
                "Provides the count of sql commands sent to a database.",
                new GaugeConfiguration { LabelNames = new[] { "commandtype", "commandtext", "server", "database" } });

        _sqlDuration = Metrics
            .CreateHistogram($"{prefix}sql_commands_duration_seconds",
                "The duration of individual sql commands sent to a database.",
                new HistogramConfiguration
                    { LabelNames = new[] { "commandtype", "commandtext", "server", "database" } });
    }

    void IObserver<DiagnosticListener>.OnNext(DiagnosticListener diagnosticListener)
    {
        lock (_listeners)
        {
            if (diagnosticListener.Name == "SqlClientDiagnosticListener")
            {
                var subscription = diagnosticListener.SubscribeWithAdapter(this);
                _listeners.Add(subscription);
            }
        }
    }


    void IObserver<DiagnosticListener>.OnError(Exception error)
    {
    }

    void IObserver<DiagnosticListener>.OnCompleted()
    {
        lock (_listeners)
        {
            _listeners.ForEach(x => x.Dispose());
            _listeners.Clear();
        }
    }

    [DiagnosticName("System.Data.SqlClient.WriteCommandBefore")]
    public void OnCommandBefore(DbCommand command)
    {
        _stopwatch.Value = Stopwatch.StartNew();
    }

    [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
    public void OnCommandAfter(DbCommand command)
    {
        _stopwatch.Value?.Stop();
        UpdateMetrics(command);
    }

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandBefore")]
    public void MicrosoftOnCommandBefore(DbCommand command)
    {
        _stopwatch.Value = Stopwatch.StartNew();
    }

    [DiagnosticName("Microsoft.Data.SqlClient.WriteCommandAfter")]
    public void MicrosoftOnCommandAfter(DbCommand command)
    {
        _stopwatch.Value?.Stop();
        UpdateMetrics(command);
    }

    private void UpdateMetrics(DbCommand command)
    {
        var timeTakenSecs = _stopwatch.Value.ElapsedMilliseconds / 1000d;

        _sqlDuration.WithLabels(command.CommandType.ToString(),
            command.CommandText,
            command.Connection?.DataSource ?? "",
            command.Connection?.Database ?? "").Observe(timeTakenSecs);
        _sqlTotal.WithLabels(command.CommandType.ToString(),
            command.CommandText,
            command.Connection?.DataSource ?? "",
            command.Connection?.Database ?? "").Inc();
    }

    //[DiagnosticName("System.Data.SqlClient.WriteConnectionOpenBefore")]
    //public void OnOpenConnectionBefore(DbConnection con)
    //{
    //    Console.WriteLine($"Before Connection: {con?.Database}");
    //}

    //[DiagnosticName("System.Data.SqlClient.WriteConnectionOpenAfter")]
    //public void OnOpenConnectionAfter(DbConnection con)
    //{
    //    Console.WriteLine($"After Connection: {con?.Database}");
    //}

    //[DiagnosticName("System.Data.SqlClient.WriteConnectionCloseBefore")]
    //public void OnCloseConnectionBefore(DbConnection con)
    //{
    //    Console.WriteLine($"Before Close Connection: {con.Database}");
    //}

    //[DiagnosticName("System.Data.SqlClient.WriteConnectionCloseAfter")]
    //public void OnCloseConnectionAfter(DbConnection con)
    //{
    //    Console.WriteLine($"After Close Connection: {con.Database}");
    //}
}
using System.Diagnostics;
using Prometheus;
using TownSuite.Web.SSV3Adapter.Interfaces;

namespace TownSuite.Web.SSV3Adapter.Prometheus;

// See license.txt
// modified https://github.com/rocklan/prometheus-net.AspNet/blob/master/src/Prometheus.AspNet/Classes/PrometheusHttpRequestModule.cs
// for service stack Adapter
public class SsPrometheus : IDisposable, ISSV3Prometheus
{
    private static Counter _globalExceptions;

    private static Gauge _httpRequestsInProgress;
    private static Gauge _httpRequestsTotal;
    private static Histogram _httpRequestsDuration;
    private static string _timerKey;

    private readonly Stopwatch timer;

    // Record the time of the begin request event.
    public SsPrometheus()
    {
        _httpRequestsInProgress.Inc();
        timer = new Stopwatch();
        timer.Start();
    }

    public void Dispose()
    {
        _httpRequestsInProgress.Dec();
        timer?.Stop();
    }


    public void ExceptionTriggered()
    {
        _globalExceptions.Inc();
    }

    public void EndRequest(string code, string method, string controller, string action)
    {
        if (timer != null)
        {
            timer.Stop();

            var timeTakenSecs = timer.ElapsedMilliseconds / 1000d;

            _httpRequestsDuration.WithLabels(code, method, controller, action).Observe(timeTakenSecs);
            _httpRequestsTotal.WithLabels(code, method, controller, action).Inc();
        }
    }

    public static void Initialize(string prefix = "")
    {
        _globalExceptions = Metrics
            .CreateCounter($"{prefix}global_exceptions", "Number of global exceptions.");

        _httpRequestsInProgress = Metrics
            .CreateGauge($"{prefix}http_requests_in_progress", "The number of HTTP requests currently in progress");

        _httpRequestsTotal = Metrics
            .CreateGauge($"{prefix}http_requests_received_total",
                "Provides the count of HTTP requests that have been processed by this app",
                new GaugeConfiguration { LabelNames = new[] { "code", "method", "controller", "action" } });

        _httpRequestsDuration = Metrics
            .CreateHistogram($"{prefix}http_request_duration_seconds",
                "The duration of HTTP requests processed by this app.",
                new HistogramConfiguration { LabelNames = new[] { "code", "method", "controller", "action" } });

        _timerKey = $"{prefix}PrometheusHttpRequestModule.Timer";
    }
}
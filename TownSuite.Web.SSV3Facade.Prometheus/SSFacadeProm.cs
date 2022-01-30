using Prometheus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using TownSuite.Web.SSV3Facade.Interfaces;

namespace TownSuite.Web.SSV3Facade.Prometheus
{
    // See license.txt
    // modified https://github.com/rocklan/prometheus-net.AspNet/blob/master/src/Prometheus.AspNet/Classes/PrometheusHttpRequestModule.cs
    // for service stack facade
    public class SsPromethues : IDisposable, ISSV3Promethues
    {
        private static Counter _globalExceptions;

        private static Gauge _httpRequestsInProgress;
        private static Gauge _httpRequestsTotal;
        private static Histogram _httpRequestsDuration;
        private static string _timerKey;


        public void ExceptionTriggered()
        {
            _globalExceptions.Inc();
        }

        Stopwatch timer = null;

        // Record the time of the begin request event.
        public SsPromethues()
        {
            _httpRequestsInProgress.Inc();
            timer = new Stopwatch();
            timer.Start();
        }

        public static void Initialize(string prefix="")
        {

            _globalExceptions = Metrics
                .CreateCounter($"{prefix}global_exceptions", "Number of global exceptions.");

            _httpRequestsInProgress = Metrics
                .CreateGauge($"{prefix}http_requests_in_progress", "The number of HTTP requests currently in progress");

            _httpRequestsTotal = Metrics
                .CreateGauge($"{prefix}http_requests_received_total", "Provides the count of HTTP requests that have been processed by this app",
                    new GaugeConfiguration { LabelNames = new[] { "code", "method", "controller", "action" } });

            _httpRequestsDuration = Metrics
                 .CreateHistogram($"{prefix}http_request_duration_seconds", "The duration of HTTP requests processed by this app.",
                     new HistogramConfiguration { LabelNames = new[] { "code", "method", "controller", "action" } });

            _timerKey = $"{prefix}PrometheusHttpRequestModule.Timer";


        }

        public void EndRequest(string code, string method, string controller, string action)
        {
            if (timer != null)
            {
                timer.Stop();

                double timeTakenSecs = timer.ElapsedMilliseconds / 1000d;

                _httpRequestsDuration.WithLabels(code, method, controller, action).Observe(timeTakenSecs);
                _httpRequestsTotal.WithLabels(code, method, controller, action).Inc();
            }

        }

        public void Dispose()
        {
            _httpRequestsInProgress.Dec();
            timer?.Stop();
        }
    }
}
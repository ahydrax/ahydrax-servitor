using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ahydrax.Servitor.Extensions;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class HealthActor : ReceiveActor
    {
        private readonly Process _process;
        private readonly int _cpus;

        public HealthActor()
        {
            _process = Process.GetCurrentProcess();
            _cpus = Environment.ProcessorCount;

            ReceiveAsync<MessageArgs>(ReportStatus);
        }

        private async Task ReportStatus(MessageArgs arg)
        {
            _process.Refresh();
            var consumedCpuTime = _process.TotalProcessorTime;
            await Task.Delay(TimeSpan.FromSeconds(5));
            _process.Refresh();
            consumedCpuTime = _process.TotalProcessorTime - consumedCpuTime;

            var cpuPercentage = (decimal)consumedCpuTime.TotalMilliseconds * 1.0M / (5000M * _cpus);
            var bytesConsumed = _process.WorkingSet64;

            Context.System.SelectActor<TelegramMessageChannel>()
                .Tell(new MessageArgs<string>(arg.ChatId,
                    $"[HEALTH_ACTOR] In last 5 seconds: CPU {cpuPercentage * 100:F1}% Mem: {ByteSize(bytesConsumed)}"));
        }

        static string[] sizeSuffixes =
        {
            "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"
        };

        private static string ByteSize(long size)
        {
            const string formatTemplate = "{0}{1:0.#} {2}";

            if (size == 0)
            {
                return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
            }

            var absSize = Math.Abs((double)size);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= sizeSuffixes.Length
                ? sizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);

            return string.Format(
                formatTemplate,
                size < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
        }
    }
}
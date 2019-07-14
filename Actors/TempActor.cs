using System.Diagnostics;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors.Utility;
using ahydrax.Servitor.Extensions;
using ahydrax.Servitor.Utils;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class TempActor : ReceiveActor
    {
        public TempActor()
        {
            ReceiveAsync<MessageArgs>(ReportTemp);
        }

        private async Task ReportTemp(MessageArgs arg)
        {
            try
            {
                const string cmd = @"/opt/vc/bin/vcgencmd measure_temp";
                var escapedArgs = cmd.Replace("\"", "\\\"");

                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                await process.WaitForExitAsync();
                var output = process.StandardOutput.ReadToEnd();

                Context.System.Actor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[TEMP_ACTOR] " + output));
            }
            catch
            {
                Context.System.Actor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[TEMP_ACTOR] Not running on raspberry pi"));
            }
        }
    }
}

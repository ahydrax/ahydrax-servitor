using System.Diagnostics;
using ahydrax.Servitor.Extensions;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class TempActor : ReceiveActor
    {
        public TempActor()
        {
            Receive<MessageArgs>(ReportTemp);
        }

        private void ReportTemp(MessageArgs arg)
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
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                Context.System.SelectActor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[TEMP_ACTOR] " + output));
            }
            catch
            {

                Context.System.SelectActor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[TEMP_ACTOR] Not running on raspberry pi"));
            }
        }
    }
}

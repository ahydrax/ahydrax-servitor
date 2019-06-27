using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ahydrax.Servitor.Extensions;
using ahydrax.Servitor.Utils;
using Akka.Actor;
using File = System.IO.File;

namespace ahydrax.Servitor.Actors
{
    public class SelfieActor : ReceiveActor
    {
        public SelfieActor()
        {
            ReceiveAsync<MessageArgs>(MakeSelfie);
        }

        private async Task MakeSelfie(MessageArgs arg)
        {
            try
            {
                var cmd = @"fswebcam -r 1280x720 /tmp/selfie.jpg --title " + $"\"{DateTime.Now:R}\"";
                var escapedArgs = cmd.Replace("\"", "\\\"");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                await process.WaitForExitAsync();

                Context.System.SelectActor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[SELFIE_ACTOR] Done"));

                var content = await File.ReadAllBytesAsync("/tmp/selfie.jpg");

                Context.System.SelectActor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<byte[]>(arg.ChatId, content));
            }
            catch
            {
                Context.System.SelectActor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[SELFIE_ACTOR] Not running on raspberry pi"));
            }
        }
    }
}

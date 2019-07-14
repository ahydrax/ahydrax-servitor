using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors.Utility;
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
                var cmd = $@"fswebcam -r 1280x720 /tmp/selfie.png -q --info ""{DateTime.Now:R}"" --png -1 --title ""ahydrax-servitor""";
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
                Context.System.Actor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, "[SELFIE_ACTOR] Working..."));
                await process.WaitForExitAsync();
                var content = await File.ReadAllBytesAsync("/tmp/selfie.png");

                Context.System.Actor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<byte[]>(arg.ChatId, content));
            }
            catch (Exception e)
            {
                Context.System.Actor<TelegramMessageChannel>()
                    .Tell(new MessageArgs<string>(arg.ChatId, $"[SELFIE_ACTOR] Error occured\n{e.Message}\n{e.StackTrace}"));
            }
        }
    }
}

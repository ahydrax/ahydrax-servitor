﻿using System;
using Akka.Actor;

namespace ahydrax_servitor
{
    public class CatStatusResponder : ReceiveActor
    {
        private static readonly Random Random = new Random();
        private static readonly string[] Replies = {
            "Ларис, я занят",
            "У меня митинг",
            "Кот спит",
            "занет пока",
            "Ларис, чо отвлекаешь",
            "Мущина, вы не видите, у нас обед",
            "Кот отдыхает",
            "Кот ест",
            "Кот мышь поймал",
            "Кот раздербанил крысу",
            "Кот лежит",
            "Кот играется",
            "Кот в беседке",
            "Котик бегает",
            "Кот хищник"
        };

        public CatStatusResponder()
        {
            Receive<TelegramMessage<string>>(Respond);
        }

        private bool Respond(TelegramMessage<string> obj)
        {
            var randomIndex = Random.Next(0, Replies.Length);
            var reply = Replies[randomIndex];
            Context.System.ActorSelection("user/" + nameof(TelegramMessageChannel)).Tell(new TelegramMessage<string>(obj.ChatId, reply));
            return true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.DisplayDialogue
{
    public class ConversationQueue
    {
        private Queue<Conversation> _queue = new();

        public Conversation Top => _queue.Peek();
        public void Enqueue(Conversation c) => _queue.Enqueue(c);
        public void InterruptEnqueue(Conversation c) => _queue = new Queue<Conversation>(new[] { c }.Concat(_queue));
        public Conversation Dequeue() => _queue.Dequeue();

        public bool IsEmpty() => _queue.Count == 0;
    }
}
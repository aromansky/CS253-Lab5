using System;

namespace CS253_Lab5
{
    internal static class Program
    {
        static void Main()
        {
            KnowledgeBase kb = new KnowledgeBase();
            kb.LoadKnowledgeBase();

            Console.WriteLine(string.Join('\n', InferenceEngine.BackwardChaining(kb)));
        }
    }
}
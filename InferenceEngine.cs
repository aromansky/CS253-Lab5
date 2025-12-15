using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS253_Lab5
{
    public static class StringExtensions
    {
        public static string Repeat(this string value, int count) => string.Concat(Enumerable.Repeat(value, count));
    }

    internal class InferenceEngine
    {
        public static List<string> ForwardChaining(KnowledgeBase kb)
        {
            HashSet<int> knownFacts = new HashSet<int>(kb.AxiomIds);
            HashSet<int> applicatedRules = new HashSet<int>();
            List<string> explanationLog = new List<string>();

            bool somethingChanged = true;
            bool succes = false;
            while (somethingChanged)
            {
                if (succes) break;

                somethingChanged = false;
                foreach(Rule rule in kb.Rules)
                {
                    if (applicatedRules.Contains(rule.ID))
                        continue;

                    if (rule.PremiseFactsIds.All(x => knownFacts.Contains(x)) && !knownFacts.Contains(rule.ConclusionFactId))
                    {
                        knownFacts.Add(rule.ConclusionFactId);
                        somethingChanged = true;
                        applicatedRules.Add(rule.ID);

                        explanationLog.Add($"Сработало правило {rule.ID}: Так как известны {string.Join(", ", rule.PremiseFactsIds)}, получен факт {rule.ConclusionFactId}");

                        if (rule.ConclusionFactId == kb.TargetFactId)
                        {
                            succes = true;
                            break;
                        }
                    }
                }
            }

            explanationLog.Add("Вывод завершен. Итоговые факты: " + string.Join(", ", knownFacts));
            explanationLog.Add(knownFacts.Contains(kb.TargetFactId) ? $"Факт f{kb.TargetFactId} доказан." : $"Факт f{kb.TargetFactId} невыводим.");
            return explanationLog;
        }

        class GoalNode
        {
            /// <summary>
            /// ID доказываемого факта
            /// </summary>
            public int GoalId { get; set; }

            /// <summary>
            /// Правила, которые могут вывести этот факт
            /// </summary>
            public List<Rule> InferenceRules { get; set; } = new List<Rule>();

            public GoalNode(int goalId)
            {
                GoalId = goalId;
            }
        }

        static public List<String> BackwardChaining(KnowledgeBase kb)
        {
            Stack<GoalNode> goalStack = new Stack<GoalNode>();
            goalStack.Push(new GoalNode(kb.TargetFactId));

            HashSet<int> knownFacts = new HashSet<int>(kb.AxiomIds);
            HashSet<int> failedFacts = new HashSet<int>();

            Dictionary<int, Rule> provenance = new Dictionary<int, Rule>();

            while (goalStack.Count != 0)
            {
                GoalNode goal = goalStack.Peek();
                int goalId = goal.GoalId;

                // вдруг факт доказан или недоказуем
                if (knownFacts.Contains(goalId) || failedFacts.Contains(goalId))
                {
                    goalStack.Pop();
                    continue;
                }

                // Если нода встречена впервые, то докидываем туда правила которые могут её вывести
                if (goal.InferenceRules.Count == 0)
                {
                    // ищем правила которые выводят этот факт
                    goal.InferenceRules = kb.Rules.Where(rule => rule.ConclusionFactId == goalId).ToList();

                    // если таких правил нет, то факт невыводим
                    if (goal.InferenceRules.Count == 0)
                    {
                        failedFacts.Add(goalId);
                        goalStack.Pop();
                        continue;
                    }
                }
                

                bool addedNewPremise = false;
                bool factIsProven = false;
                // если есть, то обходимся по ним
                for (int i = goal.InferenceRules.Count - 1; i >= 0; i--)
                {
                    Rule rule = goal.InferenceRules[i];
                    // если есть всё для срабатывания правила
                    if (rule.PremiseFactsIds.All(factId => knownFacts.Contains(factId)))
                    {
                        knownFacts.Add(goalId);
                        goalStack.Pop();
                        provenance[goalId] = rule;
                        factIsProven = true;
                        break;
                    }

                    // если какого-то факта недостаёт
                    foreach (int premiseFactId in rule.PremiseFactsIds)
                    {
                        // если факт недоказуем, то правило непременимо
                        if (failedFacts.Contains(premiseFactId))
                        {
                            goal.InferenceRules.Remove(rule);
                            break;
                        }

                        // если факт не доказан и не приводит к циклу
                        if (!knownFacts.Contains(premiseFactId) && !goalStack.Any(x => x.GoalId == premiseFactId))
                        {
                            GoalNode node = new GoalNode(premiseFactId);
                            goalStack.Push(node);
                            addedNewPremise = true;
                            break;
                        }
                    }

                    if (addedNewPremise)
                        break;
                }

                if (addedNewPremise)
                    continue;

                // если обошлись по всем правилам и везде циклимся
                if (!factIsProven)
                {
                    goalStack.Pop();
                    failedFacts.Add(goalId);
                }
            }

            List<string> explanationLog = new List<string>();
            Queue<string> resQueue = new Queue<string>();

            if (knownFacts.Contains(kb.TargetFactId))
            {
                resQueue.Enqueue($"Факт f{kb.TargetFactId} доказан.");

                HashSet<int> visitedFacts = new HashSet<int>();
                // id факта и глубина
                Stack<(int, int)> factStack = new Stack<(int, int)>();
                factStack.Push((kb.TargetFactId, 1));

                while (factStack.Count != 0)
                {
                    var (factId, depth) = factStack.Pop();

                    if (visitedFacts.Contains(factId) || kb.AxiomIds.Contains(factId)) continue;

                    visitedFacts.Add(factId);

                    foreach (int id in provenance[factId].PremiseFactsIds)
                        factStack.Push((id, depth + 1));

                    Rule rule = provenance[factId];
                    resQueue.Enqueue("  ".Repeat(depth) + $"Сработало правило {rule.ID}: Так как известны {string.Join(", ", rule.PremiseFactsIds)}, получен факт {rule.ConclusionFactId}");
                }

                while (resQueue.Count != 0) explanationLog.Add(resQueue.Dequeue());
            }
            else
            {
                explanationLog.Add($"Факт f{kb.TargetFactId} невыводим.");
            }

            return explanationLog;
        }
    }
}

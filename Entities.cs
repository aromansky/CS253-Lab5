using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS253_Lab5
{
    internal class Fact
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public double Confidence { get; set; }

        public Fact(int id, string description, double confidence)
        {
            ID = id;
            Description = description;
            Confidence = confidence;
        }

        public override string ToString()
        {
            return $"f{ID}";
        }
    }

    internal class Rule
    {
        public int ID { get; set; }
        public List<int> PremiseFactsIds { get; set; }
        public int ConclusionFactId { get; set; }
        public string Description { get; set; }
        public double Confidence { get; set; }
        public Rule(int id, List<int> premiseFactsIds, int conclusionFactId, string description, double confidence)
        {
            ID = id;
            PremiseFactsIds = new List<int>(premiseFactsIds);
            ConclusionFactId = conclusionFactId;
            Description = description;
            Confidence = confidence;
        }

        public override string ToString()
        {
            return $"r{ID}";
        }
    }

    internal class KnowledgeBase
    {
        public List<Fact> Facts { get; set; }
        public List<Rule> Rules { get; set; }
        public HashSet<int> AxiomIds { get; set; }
        public int TargetFactId { get; set; }

        public KnowledgeBase()
        {
            Facts = new List<Fact>();
            Rules = new List<Rule>();
            AxiomIds = new HashSet<int>();
            TargetFactId = -1;
        }

        public void ParseFacts(string path)
        {
            using(StreamReader reader = new StreamReader(path))
            {
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');
                    if (string.IsNullOrEmpty(line) || parts[0].StartsWith('#'))
                        continue;

                    int id = int.Parse(parts[0][1..]);
                    string description = parts[1];
                    double confidence = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    Facts.Add(new Fact(id, description, confidence));
                }
            }
        }

        public void ParseRules(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(';');
                    if (string.IsNullOrEmpty(line) || parts[0].StartsWith('#'))
                        continue;
                    int id = int.Parse(parts[0][1..]);
                    List<int> premiseFactsIds = parts[1].Split(',').Select(x => int.Parse(x[1..])).ToList();
                    int conclusionFactId = int.Parse(parts[2][1..]);
                    string description = parts[3];
                    double confidence = double.Parse(parts[4], CultureInfo.InvariantCulture);
                    Rules.Add(new Rule(id, premiseFactsIds, conclusionFactId, description, confidence));
                }
            }
        }

        public void ParseAxioms(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
                        continue;
                    AxiomIds.Add(int.Parse(line[1..]));
                }
            }
        }

        public void ParseTargetFact(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                {
                    TargetFactId = int.Parse(line[1..]);
                }
            }
        }

        public void LoadKnowledgeBase(string factsPath = "../../../data/facts.txt", string rulesPath = "../../../data/rules.txt",
            string axiomsPath = "../../../data/axioms.txt", string targetPath = "../../../data/target.txt")
        {
            ParseFacts(factsPath);
            ParseRules(rulesPath);
            ParseAxioms(axiomsPath);
            ParseTargetFact(targetPath);
        }
    }
}

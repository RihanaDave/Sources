using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl
{
    [DataContract]
    public static class Classification
    {
        [DataMember]
        public static LinkedList<ClassificationEntry> EntriesTree { private set; get; }

        static Classification()
        {
            SetStaticClassificationTree();
        }
        private static void SetStaticClassificationTree()
        {
            EntriesTree = new LinkedList<ClassificationEntry>();
            EntriesTree.AddFirst(new ClassificationEntry()
            {
                Title = "بدون طبقه‏بندی",
                IdentifierString = "N"
            });

            EntriesTree.AddLast(new ClassificationEntry()
            {
                Title = "محرمانه",
                IdentifierString = "R"
            });

            EntriesTree.AddLast(new ClassificationEntry()
            {
                Title = "خیلی محرمانه",
                IdentifierString = "C"
            });

            EntriesTree.AddLast(new ClassificationEntry()
            {
                Title = "سری",
                IdentifierString = "S"
            });

            EntriesTree.AddLast(new ClassificationEntry()
            {
                Title = "خیلی سری",
                IdentifierString = "TS"
            });
        }

        public static ClassificationEntry GetFirstClassificationEntryMatchesTitle(string title)
        {
            foreach (var currentEntry in EntriesTree)
            {
                if (currentEntry.Title.Equals(title))
                {
                    return currentEntry;
                }
            }
            throw new InvalidOperationException("No classification matches the title");
        }
        public static ClassificationEntry GetClassificationEntryByIdentifier(string classificationIdentfier)
        {
            foreach (var currentEntry in EntriesTree)
            {
                if (currentEntry.IdentifierString.Equals(classificationIdentfier))
                {
                    return currentEntry;
                }
            }
            throw new InvalidOperationException("No classification matches the identifier");
        }

        public static List<ClassificationEntry> GetClassificationsWithLowerPriorityThan(ClassificationEntry baseClassification)
        {
            List<ClassificationEntry> result = new List<ClassificationEntry>();
            LinkedListNode<ClassificationEntry> baseNode = EntriesTree.Find(GetClassificationEntryByIdentifier(baseClassification.IdentifierString));

            foreach (var item in baseNode.List)
            {
                if (item.IdentifierString.Equals(baseClassification.IdentifierString))
                {
                    result.Add(item);
                    break;
                }
                else
                {
                    result.Add(item);
                }
            }
            //if (!baseNode.Equals(EntriesTree.First))
            //{
            //    LinkedListNode<ClassificationEntry> currentNode = baseNode;
            //    do
            //    {
            //        currentNode = currentNode.Previous;
            //        result.Add(currentNode.Value);
            //    } while (!currentNode.Equals(EntriesTree.First));
            //}
            return result;
        }

        public static int GetClassificationIndex(string classificationIdentfier)
        {
            int result = -1;
            LinkedListNode<ClassificationEntry> baseNode = EntriesTree.Find(GetClassificationEntryByIdentifier(classificationIdentfier));

            for (int i = 0; i < baseNode.List.Count; i++)
            {
                if (baseNode.List.ElementAt(i).IdentifierString.Equals(classificationIdentfier))
                {
                    result = i;
                    break;
                }
            }
            return result;
        }
    }
}

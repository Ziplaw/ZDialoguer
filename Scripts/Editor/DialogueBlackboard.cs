using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZDialoguer
{
    public class DialogueBlackboard : Blackboard
    {
        private new ZDialoguerGraphView graphView;

        public DialogueBlackboard(ZDialoguerGraphView graphView) : base(graphView)
        {
            this.graphView = graphView;
            var dialogueTextSection = new BlackboardSection { title = "Dialogue Text" };
            graphView.InitDialogueTextBlackboard(dialogueTextSection);
            Add(dialogueTextSection);
            Add(new BlackboardSection { title = "Facts" });
            addItemRequested += AddFactToBlackBoard;
            editTextRequested += EditFactText;
            PopulateBlackboardWithFacts();
            /*graphView._editorWindow.rootVisualElement.schedule.Execute(() =>
            {
                SetPosition(new Rect(
                    new Vector2(
                        graphView.resolvedStyle.width - 230, 0), //
                    new Vector2(230, 345)));
            }).StartingIn(300)*//*.Until(() =>
                !float.IsNaN(graphView.worldBound.x) &&
                !float.IsNaN(graphView.worldBound.y) &&
                !float.IsNaN(graphView.worldBound.width) &&
                !float.IsNaN(graphView.worldBound.height) &&
                !float.IsNaN(GetPosition().x) &&
                !float.IsNaN(GetPosition().y) &&
                !float.IsNaN(GetPosition().width) &&
                !float.IsNaN(GetPosition().height))*/;
        }

        string FixNewFactName(string newName)
        {
            int appender = 1;
            if (graphView.graph.facts.Count != 0)
            {
                while (graphView.graph.facts.Any(f => f.nameID == newName))
                {
                    if (newName.Contains($"({appender - 1})"))
                    {
                        newName = newName.Replace($"({appender - 1})", $"({appender})");
                    }
                    else
                    {
                        newName += $" ({appender})";
                    }

                    appender++;
                }
            }

            return newName;
        }

        void PopulateBlackboardWithFacts()
        {
            graphView.graph.facts.ForEach(f =>
            {
                this.Query<BlackboardSection>().ToList().First(s => s.title == "Facts")
                    .Add(GenerateFactContainer(f));
            });
        }

        VisualElement GenerateFactContainer(Fact fact)
        {
            var bbField = new FactBlackboardField(fact)
            {
                text = fact.nameID, typeText = "Fact", OnBlackboardFactSelected = graphView.OnBlackboardFactSelected
            };
            return bbField;
        }

        private void AddFactToBlackBoard(Blackboard blackboard)
        {
            var newFact = graphView.graph.CreateFact(FixNewFactName("New Fact"));
            blackboard.Query<BlackboardSection>().ToList().First(s => s.title == "Facts")
                .Add(GenerateFactContainer(newFact));

            ZDialoguerGraphView.SaveChangesToGraph(graphView.graph);
        }

        private void EditFactText(Blackboard bb, VisualElement field, string value)
        {
            string newName = FixNewFactName(value);
            var _field = (FactBlackboardField)field;
            _field.fact.nameID = newName;
            _field.fact.name = newName;
            _field.text = newName;
            _field.name = newName;
            //Implement this into the Fact Field itself

            EditorUtility.SetDirty(_field.fact);
            ZDialoguerGraphView.SaveChangesToGraph(graphView.graph);

            var view = bb.graphView as ZDialoguerGraphView;
            view.PopulateView(view.graph);
        }
    }
}
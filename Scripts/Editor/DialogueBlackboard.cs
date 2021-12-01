using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
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
            Add(new BlackboardSection { title = "Characters" });
            addItemRequested += OnAddField;
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

        string FixNewFactName(string newName, IEnumerable<string> names)
        {
            int appender = 1;
            if (names.Any())
            {
                while (names.Any(s => s == newName))
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
            // for (var i = 0; i < graphView.graph.facts.Count; i++)
            // {
            //     graphView.graph.facts[i] = GlobalData.Instance.facts.First(f => f.nameID == graphView.graph.facts[i].nameID);//
            // }

            graphView.graph.facts.ForEach(f =>
            {
                
                this.Query<BlackboardSection>().ToList().First(s => s.title == "Facts")
                    .Add(GenerateFactContainer(f));
            });
        }

        VisualElement GenerateFactContainer(int factIndex)
        {
            var bbField = new FactBlackboardField(factIndex)
            {
                text = GlobalData.Instance.facts[factIndex].nameID, typeText = "Fact", OnBlackboardFactSelected = graphView.OnBlackboardFactSelected
            };
            return bbField;
        }

        private void OnAddField(Blackboard blackboard)
        {
            var menuPosition = new Vector2(layout.xMax, 0);
            menuPosition = this.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Fact"),false, data => GlobalDataSearchWindow.Open(graphView.graph, GlobalData.Instance.facts,
                fact => AddFactToBlackBoard(blackboard, fact), 
                list => GlobalData.Instance.Create<Fact>(FixNewFactName("New Fact", GlobalData.Instance.facts.Select(f => f.nameID))),
                fact => GlobalData.Instance.Delete(fact), 
                GUIUtility.GUIToScreenRect(menuRect).position), null);
            menu.AddItem(new GUIContent("Character"),false, data => AddCharacterToBlackboard(blackboard), null);
            
            menu.DropDown(menuRect);
        }

        private void AddFactToBlackBoard(Blackboard blackboard, int factIndex)
        {
            blackboard.Query<BlackboardSection>().ToList().First(s => s.title == "Facts").Add(GenerateFactContainer(factIndex));
            graphView.graph.facts.Add(factIndex);
            ZDialoguerGraphView.SaveChangesToObject(graphView.graph);
        }
        
        private void AddCharacterToBlackboard(Blackboard blackboard, Character character = null)
        {
            Debug.Log(character);

            // var newFact = graphView.graph.CreateFact(FixNewFactName("New Fact"));
            // blackboard.Query<BlackboardSection>().ToList().First(s => s.title == "Facts")
            //     .Add(GenerateFactContainer(newFact));
            //
            // ZDialoguerGraphView.SaveChangesToObject(graphView.graph);
        }

        private void EditFactText(Blackboard bb, VisualElement field, string value)
        {
            string newName = FixNewFactName(value, GlobalData.Instance.facts.Select(f => f.nameID));
            var _field = (FactBlackboardField)field;
            GlobalData.Instance.facts[_field.factIndex].nameID = newName;
            _field.text = newName;
            _field.name = newName;
            //Implement this into the Fact Field itself

            ZDialoguerGraphView.SaveChangesToObject(graphView.graph);
            ZDialoguerGraphView.SaveChangesToObject(GlobalData.Instance);//

            var view = bb.graphView as ZDialoguerGraphView;
            view.PopulateView(view.graph);
        }
    }
}
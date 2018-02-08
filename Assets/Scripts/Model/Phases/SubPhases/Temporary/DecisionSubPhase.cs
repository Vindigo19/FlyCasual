﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

namespace SubPhases
{
    public class Decision
    {
        public string Name { get; private set; }
        public EventHandler Effect { get; private set; }
        public string Tooltip { get; private set; }
        public int Count { get; private set; }

        public bool HasTooltip
        {
            get { return Tooltip != null; }
        }

        public Decision(string name, EventHandler effect, string tooltip = null, int count = -1)
        {
            Name = name;
            Effect = effect;
            Tooltip = tooltip;
            Count = count;
        }

        public void AddTooltip(string tooltip)
        {
            Tooltip = tooltip;
        }

        public void ExecuteDecision(object sender = null, EventArgs e = null)
        {
            Effect.Invoke(sender, e);
        }
    }

    public class DecisionSubPhase : GenericSubPhase
    {
        private GameObject decisionPanel;
        public string InfoText;
        private List<Decision> decisions = new List<Decision>();
        public string DefaultDecisionName;
        protected Players.GenericPlayer DecisionOwner;
        public bool ShowSkipButton;

        private const float defaultWindowHeight = 75;
        private const float buttonHeight = 45;

        public override void Start()
        {
            IsTemporary = true;

            decisionPanel = GameObject.Find("UI").transform.Find("DecisionsPanel").gameObject;

            PrepareDecision(StartIsFinished);
        }

        public virtual void PrepareDecision(Action callBack)
        {
            callBack();
        }

        private void StartIsFinished()
        {
            Initialize();

            UpdateHelpInfo();
        }

        public string AddDecision(string name, EventHandler call, string tooltip = null, int count = -1)
        {
            int counter = 2;
            string newName = name;
            while (decisions.Exists(n => n.Name == newName))
            {
                newName = name + " #" + counter++;
            }
            decisions.Add(new Decision(newName, call, tooltip, count));

            return newName;
        }

        public string AddTooltip(string name, string imageUrl)
        {
            int counter = 2;
            string newName = name;
            while (decisions.Find(n => n.HasTooltip) !=null)
            {
                newName = name + " #" + counter++;
            }
            decisions.Find(n => n.Name == newName).AddTooltip(imageUrl);

            return newName;
        }

        public List<Decision> GetDecisions()
        {
            return decisions;
        }

        public override void Initialize()
        {
            if (decisions.Count != 0)
            {
                decisionPanel.transform.Find("InformationPanel").GetComponentInChildren<Text>().text = InfoText;

                int i = 0;
                foreach (var decision in decisions)
                {
                    GameObject prefab = (GameObject)Resources.Load("Prefabs/DecisionButton", typeof(GameObject));
                    GameObject buttonsHolder = decisionPanel.transform.Find("DecisionsPanel").gameObject;
                    GameObject button = MonoBehaviour.Instantiate(prefab, buttonsHolder.transform);
                    button.transform.localPosition = new Vector3((i % 2 == 0) ? 5 : 200, -buttonHeight * (i / 2), 0);
                    button.name = "Button" + i;

                    button.GetComponentInChildren<Text>().text = decision.Name;

                    if (decision.HasTooltip)
                    {
                        Tooltips.AddTooltip(button, decision.Tooltip);
                    }

                    EventTrigger trigger = button.AddComponent<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerClick;
                    entry.callback.AddListener(
                        (data) => { GameModes.GameMode.CurrentGameMode.TakeDecision(decision, button); }
                    );
                    trigger.triggers.Add(entry);

                    i++;
                }
                decisionPanel.GetComponent<RectTransform>().sizeDelta = new Vector3(decisionPanel.GetComponent<RectTransform>().sizeDelta.x, defaultWindowHeight + ((i + 1) / 2) * buttonHeight);

                if (DecisionOwner == null) DecisionOwner = Roster.GetPlayer(Phases.CurrentPhasePlayer);

                if (ShowSkipButton) UI.ShowSkipButton();

                DecisionOwner.TakeDecision();
            }
        }

        public override void Pause()
        {
            HidePanel();
        }

        public override void Resume()
        {
            Phases.CurrentSubPhase = this;
            UpdateHelpInfo();
            Initialize();
        }

        public override void Next()
        {
            HidePanel();
            Phases.CurrentSubPhase = PreviousSubPhase;
            UpdateHelpInfo();
        }

        private void HidePanel()
        {
            decisionPanel.gameObject.SetActive(false);
            foreach (Transform button in decisionPanel.transform.Find("DecisionsPanel"))
            {
                MonoBehaviour.Destroy(button.gameObject);
            }
        }

        public override bool ThisShipCanBeSelected(Ship.GenericShip ship, int mouseKeyIsPressed)
        {
            bool result = false;
            return result;
        }

        public override bool AnotherShipCanBeSelected(Ship.GenericShip anotherShip, int mouseKeyIsPressed)
        {
            bool result = false;
            return result;
        }

        public void ExecuteDecision(string decisionName)
        {
            decisions.Find(n => n.Name == decisionName).ExecuteDecision();
        }

        public override void DoDefault()
        {
            ExecuteDecision(DefaultDecisionName);
        }

        public static void ConfirmDecision()
        {
            Tooltips.EndTooltip();
            UI.HideSkipButton();

            Action callBack = Phases.CurrentSubPhase.CallBack;
            Phases.FinishSubPhase(Phases.CurrentSubPhase.GetType());
            Phases.CurrentSubPhase.Resume();
            callBack();
        }

        public static void ConfirmDecisionNoCallback()
        {
            Tooltips.EndTooltip();
            UI.HideSkipButton();

            Phases.FinishSubPhase(Phases.CurrentSubPhase.GetType());
            Phases.CurrentSubPhase.Resume();
        }

        public override void SkipButton()
        {
            ConfirmDecision();
        }

    }

}

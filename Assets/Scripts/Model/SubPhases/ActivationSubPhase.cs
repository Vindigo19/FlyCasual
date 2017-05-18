﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SubPhases
{

    public class ActivationSubPhase : GenericSubPhase
    {

        public override void StartSubPhase()
        {
            Game = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
            Name = "Activation SubPhase";

            Game.UI.AddTestLogEntry(Name);

            Dictionary<int, Players.PlayerNo> pilots = Game.Roster.NextPilotSkillAndPlayerAfter(RequiredPilotSkill, RequiredPlayer, Sorting.Asc);
            foreach (var pilot in pilots)
            {
                RequiredPilotSkill = pilot.Key;
                RequiredPlayer = pilot.Value;
                UpdateHelpInfo();
            }

            Game.Roster.GetPlayer(RequiredPlayer).PerformManeuver();
        }

        public override void NextSubPhase()
        {
            Game.Phases.CurrentSubPhase = new ActionSubPhase();
            Game.Phases.CurrentSubPhase.PreviousSubPhase = this;
            Game.Phases.CurrentSubPhase.RequiredPilotSkill = RequiredPilotSkill;
            Game.Phases.CurrentSubPhase.RequiredPlayer = RequiredPlayer;
            Game.Phases.CurrentSubPhase.StartSubPhase();
        }

        public override bool ThisShipCanBeSelected(Ship.GenericShip ship)
        {
            bool result = false;

            if ((ship.Owner.PlayerNo == RequiredPlayer) && (ship.PilotSkill == RequiredPilotSkill))
            {
                result = true;
            }
            else
            {
                Game.UI.ShowError("Ship cannot be selected:\n Need " + RequiredPlayer + " and pilot skill " + RequiredPilotSkill);
            }
            return result;
        }

        public override int CountActiveButtons(Ship.GenericShip ship)
        {
            int result = 0;
            if (Game.Selection.ThisShip.AssignedManeuver != null)
            {
                Game.UI.panelContextMenu.transform.Find("MovePerformButton").gameObject.SetActive(true);
                result++;
            }
            else
            {
                Game.UI.ShowError("This ship has already executed his maneuver");
            };
            return result;
        }

    }

}
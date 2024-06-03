using RoR2.CharacterAI;
using System.Collections.Generic;
using UnityEngine;

namespace AlliesAvoidImplosions
{
    internal class GTFOHController : MonoBehaviour
    {
        private readonly static List<GTFOHController> instancesList = [];
        private BaseAI ai;

        private void Awake()
        {
            instancesList.Add(this);
            ai = GetComponent<BaseAI>();
        }

        private void OnDestroy()
        {
            instancesList.Remove(this);
        }

        private void FixedUpdate()
        {
            if (Hooks.implosions.Count == 0)
            {
                enabled = false;
                return;
            }
            var body = ai.body;
            if (body != null)
            {
                GameObject go = null;
                var minDistance = float.MaxValue;
                foreach (var implosion in Hooks.implosions)
                {
                    var distance = Vector3.Distance(implosion.transform.position, body.transform.position);
                    if (distance < Configuration.evasionDistance.Value && distance < minDistance)
                    {
                        minDistance = distance;
                        go = implosion;
                    }
                }
                if (ai.customTarget.gameObject != go)
                {
                    ai.customTarget.gameObject = go;
                    // Switch drivers instantly or else the minion may spend too long inside the implosion to escape
                    ai.BeginSkillDriver(ai.EvaluateSkillDrivers());
                }
            }
        }

        internal static void EnableAll()
        {
            foreach (var controller in instancesList)
            {
                controller.enabled = true;
            }
        }
    }
}
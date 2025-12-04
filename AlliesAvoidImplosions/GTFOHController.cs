using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using UnityEngine;

namespace AlliesAvoidImplosions;

internal class GTFOHController : MonoBehaviour
{
    private readonly static List<GTFOHController> instancesList = [];
    private BaseAI ai;

    public AISkillDriver skillDriver;

    private void Awake()
    {
        instancesList.Add(this);
        ai = GetComponent<BaseAI>();
    }

    private void OnDestroy()
    {
        instancesList.Remove(this);
    }

    private void OnEnable()
    {
        if (skillDriver)
        {
            skillDriver.enabled = true;
        }
        // The Transport Drone interferes with our customTarget, so we need to disable it for now
        var body = ai.body;
        if (body && body.TryGetComponent<HaulerDroneController>(out var haulerController))
        {
            haulerController.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (skillDriver)
        {
            skillDriver.enabled = false;
        }
        var body = ai.body;
        if (body && body.TryGetComponent<HaulerDroneController>(out var haulerController))
        {
            haulerController.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        if (Hooks.implosions.Count == 0)
        {
            enabled = false;
            return;
        }
        var body = ai.body;
        if (body)
        {
            GameObject go = null;
            var minDistance = float.MaxValue;
            foreach (var implosion in Hooks.implosions)
            {
                var distance = Vector3.Distance(implosion.transform.position, body.transform.position);
                if (distance < Configs.EvasionDistance.Value && distance < minDistance)
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
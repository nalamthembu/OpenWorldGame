using System.Collections.Generic;
using UnityEngine;

public class VehicleBodyKitManager : MonoBehaviour
{
    public VehicleBodyKitScriptable bodyKit;
    public VehiclePaintjobScriptable paintJob;
    public TyreScriptable Tyres;
    public RimScriptable Rims;
    public KitIndiceSettings kitIndiceSettings;
    public PaintJobSettings paintJobSettings;
    public NonCustomisableParts nonCustomParts;
    private Vehicle vehicle;

    //"r" prefix means real-time or spawned at runtime
    private GameObject rbodyKit;
    private List<Material> rMaterials;

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();

        InitialiseBodyKit();
    }

    [ContextMenu("DEBUG_ReInitialiseBodyKit")]
    public void InitialiseBodyKit()
    {
        if (rbodyKit != null)
            Destroy(rbodyKit);

        rbodyKit = new("Body_Kit");
        rMaterials = new();

        AddMaterialToList(transform.GetComponent<Renderer>().material);

        rbodyKit.transform.SetParent(transform);
        rbodyKit.transform.localPosition = Vector3.zero;
        rbodyKit.transform.localEulerAngles = Vector3.zero;

        //Fenders
        InitialisePart(bodyKit.fenders[kitIndiceSettings.fender].mesh, rbodyKit.transform);
        if (bodyKit.fenders[kitIndiceSettings.fender].additionalMeshes.Length > 0)
        {
            InitialisePart(bodyKit.fenders[kitIndiceSettings.fender].additionalMeshes[kitIndiceSettings.fender], rbodyKit.transform);
        }

        InitialisePart(bodyKit.bonnets[kitIndiceSettings.bonnet].mesh, rbodyKit.transform);
        InitialisePart(bodyKit.frontBumpers[kitIndiceSettings.frontBumper].mesh, rbodyKit.transform);
        InitialisePart(bodyKit.rearBumpers[kitIndiceSettings.rearBumper].mesh, rbodyKit.transform);
        //InitialisePart(bodyKit.rollCages[kitIndiceSettings.rollCage].mesh, rbodyKit.transform); //Something wrong here.
        InitialisePart(bodyKit.roofScoops[kitIndiceSettings.roofScoop].mesh, rbodyKit.transform);
        InitialisePart(bodyKit.sideSkirts[kitIndiceSettings.sideSkirt].mesh, rbodyKit.transform);
        InitialisePart(bodyKit.spoilers[kitIndiceSettings.spoiler].mesh, rbodyKit.transform, bodyKit.spoilers[kitIndiceSettings.spoiler].location);

        //Init Rims

        for (int i = 0; i < vehicle.AllWheels.Count; i++)
        {
            vehicle.AllWheels[i].Rim = bodyKit.rims[kitIndiceSettings.rims];
            vehicle.AllWheels[i].SpawnWheelAndRim();
        }


        for (int i = 0; i < nonCustomParts.parts.Length; i++)
        {
            AddMaterialToList(nonCustomParts.parts[i].GetComponent<Renderer>().material);
        }

        PaintAllParts();
    }

    private void InitialisePart(GameObject partGO, Transform parent, Vector3 pos = default, Quaternion rot = default)
    {
        GameObject rPart;

        if (pos == default || rot == default)
            rPart = Instantiate(partGO, parent);
        else
        {
            rPart = Instantiate(partGO, pos, rot, parent);
            rPart.transform.localPosition = pos;
        }

        if (rPart.TryGetComponent<Renderer>(out var r))
        {
            Material realTimeMaterial = new(r.sharedMaterial);

            r.GetComponent<Renderer>().material = realTimeMaterial;

            AddMaterialToList(r.material);
        }
    }

    [ContextMenu("DEBUG_Repaint")]
    public void InitialisePaintJob() => PaintAllParts();

    private void AddMaterialToList(Material mat) => rMaterials.Add(mat);

    private void PaintAllParts()
    {
        for (int i = 0; i < rMaterials.Count; i++)
            rMaterials[i].mainTexture = paintJob.paintJobs[paintJobSettings.paintJobIndex].texture;
    }

    public KitIndiceSettings RandomKit()
    {
        KitIndiceSettings kitSet = new()
        {
            bonnet = Random.Range(0, bodyKit.bonnets.Length),
            fender = Random.Range(0, bodyKit.fenders.Length),
            frontBumper = Random.Range(0, bodyKit.frontBumpers.Length),
            rearBumper = Random.Range(0, bodyKit.rearBumpers.Length),
            rollCage = Random.Range(0, bodyKit.rearBumpers.Length),
            roofScoop = Random.Range(0, bodyKit.roofScoops.Length),
            sideSkirt = Random.Range(0, bodyKit.sideSkirts.Length),
            spoiler = Random.Range(0, bodyKit.spoilers.Length),
            rims = Random.Range(0, bodyKit.rims.Length)
        };

        return kitSet;
    }

    public void RandomisePaintJob() => paintJobSettings.paintJobIndex = Random.Range(0, paintJob.paintJobs.Length);
}

[System.Serializable]
public struct KitIndiceSettings
{
    public int bonnet;
    public int fender;
    public int frontBumper;
    public int rearBumper;
    public int rollCage;
    public int roofScoop;
    public int sideSkirt;
    public int spoiler;
    public int rims;
}

[System.Serializable]
public struct PaintJobSettings
{
    public int paintJobIndex;
}

[System.Serializable]
public struct NonCustomisableParts
{
    public GameObject[] parts;
}
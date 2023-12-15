using UnityEngine;
using System.Collections.Generic;

public class VehicleBodyKitManager : MonoBehaviour
{
    public VehicleBodyKitScriptable bodyKit;
    public VehiclePaintjobScriptable paintJob;
    public TyreScriptable Tyres;
    public RimScriptable Rims;
    public KitIndiceSettings kitIndiceSettings;
    public PaintJobSettings paintJobSettings;
    public NonCustomisableParts nonCustomParts;

    //"r" prefix means real-time or spawned at runtime
    private GameObject rbodyKit;
    private List<Material> rMaterials;

    private void Awake()
    {
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
        KitIndiceSettings kitSet;

        kitSet.bonnet = Random.Range(0, bodyKit.bonnets.Length);
        kitSet.fender = Random.Range(0, bodyKit.fenders.Length);
        kitSet.frontBumper = Random.Range(0, bodyKit.frontBumpers.Length);
        kitSet.rearBumper = Random.Range(0, bodyKit.rearBumpers.Length);
        kitSet.rollCage = Random.Range(0, bodyKit.rearBumpers.Length);
        kitSet.roofScoop = Random.Range(0, bodyKit.roofScoops.Length);
        kitSet.sideSkirt = Random.Range(0, bodyKit.sideSkirts.Length);
        kitSet.spoiler = Random.Range(0, bodyKit.spoilers.Length);

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
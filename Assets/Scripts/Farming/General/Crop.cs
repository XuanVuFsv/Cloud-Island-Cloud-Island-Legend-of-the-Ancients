using System;
using System.Collections.Generic;
using UnityEngine;
using VitsehLand.Scripts.Construction.Garden;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Farming.ObjectState.Crop;

[System.Serializable]
public class CropProperties
{
    [Header("Effect")]
    public GameObject trailTracer;
    public GameObject bulletObject;
    public ParticleSystem hitEffectPrefab;

    public GameObject wholePlantPrefab;

    public int wateringStack;
}

public class Crop : Suckable
{
    public enum PlantState
    {
        Seed = 0,
        Stored = 1,
        GrowingBody = 2,
        Whole = 3
    }
    //public PlantState plantState;

    public CropProperties plantData;

    public GardenManager ownerGarden;
    public GardenSlotProperties ownerSlot;

    [Header("Plant Components")]
    public GameObject growingBody;
    //public GameObject orginalBody;
    public Crop orginalPlant;

    //public int growingTime = 3;
    public int elapsedTime = 0;

    public bool onTree = false;
    public bool nonPlanting;

    public DateTime startGrowingTime;
    public DateTime endGrowingTime;

    public List<Transform> wholePlantPoss = new List<Transform>();
    public List<Crop> wholePlants = new List<Crop>();
    public GameObject seedOuterEffect, wholeOuterEffect;

    public GameObject defaultModelPlant;
    public int wholePlantCount = 0;
    public bool inCrafting;

    public float startTimeDestroy;
    public float destroyedTime;
    public bool startDestroyedTimer;

    public int minDestroyTime = 50;
    public int maxDestroyTime = 100;

    public bool isFromHarvesting = false;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("Start a Plant");
        rigid = GetComponent<Rigidbody>();
        suckableCollider = GetComponent<Collider>();

        SetState(new CropSeed(this));

        //growingTime = ammoStats.totalGrowingTime;
    }

    public bool HaveWholeCrop()
    {
        return wholePlants.Count > 0;
    }

    public bool CanSuckUp()
    {
        //return plantState == PlantState.Seed || plantState == PlantState.Whole;
        return state is CropSeed || state is CropWhole;
    }

    public override void ChangeToStored()
    {
        EndCurrentState();
        SetState(new CropStored(this));
    }

    public override void ChangeToUnStored()
    {
        EndCurrentState();
        SetState(new CropSeed(this));
    }

    public void ChangeToGrowingBody()
    {
        EndCurrentState();
        SetState(new CropGrowing(this));
    }   

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "CheckingHarvest" && state is CropWhole)
        {
            state.End();
        }
    }

    private void OnDisable()
    {
        elapsedTime = (int)DateTime.UtcNow.Subtract(startGrowingTime.ToLocalTime()).TotalSeconds;
        //Debug.Log(elapsedTime);
    }
}

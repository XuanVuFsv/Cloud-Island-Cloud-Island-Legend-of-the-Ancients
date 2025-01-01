using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VitsehLand.Scripts.Crafting;
using VitsehLand.Scripts.Farming.General;
using VitsehLand.Scripts.Weapon.General;

namespace VitsehLand.Scripts.Stats
{
    [CreateAssetMenu(fileName = "New Collectable Object", menuName = "Collectable Object")]
    public class CollectableObjectStat : ScriptableObject
    {
        [Flags]
        public enum CollectableObjectComponentBitmaskEnum
        {
            None = 0,
            FarmingCropStat = 1 << 1,
            AttackingCropStat = 1 << 2,
            CraftedProductStat = 1 << 3,
            NaturalResourceStat = 1 << 4,
            All = FarmingCropStat | AttackingCropStat | CraftedProductStat | NaturalResourceStat
        }
        List<string> componentEnums;

        string namespaceName;

        [PropertySpace(SpaceBefore = 20, SpaceAfter = 10)]
        [Title("Choose Collectable Object Components")]
        [OnValueChanged("UpdateContainingComponents")]
        public CollectableObjectComponentBitmaskEnum selectedComponents;
        // Convert componentEnums to a list of component names
        List<string> strSelectedComponents;
        // Convert containing components to a list of component names
        List<string> strComponents;

        public void UpdateContainingComponents()
        {
            componentEnums =  Enum.GetNames(typeof(CollectableObjectComponentBitmaskEnum)).ToList();
            namespaceName = (new CollectableObjectStatComponent()).GetType().Namespace + ".";

            //Add valid enum names from CollectableObjectComponentBitmaskEnum to list of selected component names
            if (selectedComponents.Equals(CollectableObjectComponentBitmaskEnum.All))
            {
                //All option
                //strSelectedComponents.Clear();
                for (int i = 1; i < componentEnums.Count - 1; i++)
                {
                    if (strSelectedComponents.Contains(componentEnums[i])) continue;
                    strSelectedComponents.Add(componentEnums[i]);
                }
            }
            else if (!selectedComponents.Equals(CollectableObjectComponentBitmaskEnum.None))
            {
                //Other options
                strSelectedComponents = selectedComponents.ToString().Split(", ").ToList();
            }

            //Add or Remove component of components based on selected components
            if (selectedComponents.Equals(CollectableObjectComponentBitmaskEnum.None))
            {
                //Clear for None option
                components.Clear();
                strSelectedComponents.Clear();
                strComponents.Clear();
            }
            else
            {
                //Get list of containing component names to compare with the list of selected component names
                //Then remove non-selected components or add newly selected components
                strComponents = components.Select(item => item.GetType().ToString().Replace(namespaceName, "")).ToList();

                //Removal check
                for (int i = 0; i < strComponents.Count; i++)
                {
                    //Stop removal check if there are no components or if all components will be added
                    if (strComponents[i] == null || selectedComponents.Equals(CollectableObjectComponentBitmaskEnum.All)) break;

                    //Remove components that are no longer selected
                    if (!strSelectedComponents.Contains(strComponents[i]))
                    {
                        components.Remove(components[i]);
                        strComponents.Remove(strComponents[i]);
                    }
                }

                //Addition check
                foreach (var component in strSelectedComponents)
                {
                    //Check and add all components that exist in the list of selected components
                    if (!strComponents.Contains(component))
                    {
                        components.Add((CollectableObjectStatComponent)Activator.CreateInstance(Type.GetType(namespaceName + component)));
                        strComponents.Add(component);
                    }
                }
            }
        }

        [PropertySpace(SpaceBefore = 20, SpaceAfter = 20)]
        [Title("Containing Components")]
        [InfoBox("Components are groups of data that the game uses to process the corresponding actions of the player with a Collectable Object. Not having Component A,B or C means that the player cannot perform the corresponding action on that Collectable Object.")]
        [SerializeReference]
        public List<CollectableObjectStatComponent> components;
        public Dictionary<string, CollectableObjectStatComponent> dictComponents;

        [Button("AssignStatFromOldModelToComponentModel")]

        void AssignStat()
        {
            foreach (var component in components)
            {
                maxQuantityStored = maxCount = 9999;

                if (component is AttackingCropStat)
                {
                    (component as AttackingCropStat).shootingHandleType = (AttackingCropStat.ShootingHandleType)(int)shootingHandleType;
                    (component as AttackingCropStat).bulletEffectComponent = (AttackingCropStat.BulletEffectComponent)(int)bulletEffectComponent;
                    (component as AttackingCropStat).zoomType = (AttackingCropStat.ZoomType)(int)zoomType;

                    (component as AttackingCropStat).bulletObject = bulletObject;
                    (component as AttackingCropStat).trailTracer = trailTracer;
                    (component as AttackingCropStat).hitEffectPrefab = hitEffectPrefab;

                    (component as AttackingCropStat).force = force;
                    (component as AttackingCropStat).range = range;
                    (component as AttackingCropStat).fireRate = fireRate;

                    (component as AttackingCropStat).damage = damage;
                    (component as AttackingCropStat).damageDealToSpecificEnemy = damageDealToSpecificEnemy;

                    (component as AttackingCropStat).reloadSpeed = reloadSpeed;

                    (component as AttackingCropStat).amplitudeGainImpulse = amplitudeGainImpulse;
                    (component as AttackingCropStat).multiplierRecoilOnAim = multiplierRecoilOnAim;
                    (component as AttackingCropStat).multiplierForAmmo = multiplierForAmmo;

                    (component as AttackingCropStat).recoilPattern = recoildPattern.Select(value => value).ToList();
                    (component as AttackingCropStat).bulletDirectionPattern = bulletDirectionPattern.Select(value => value).ToList();

                    (component as AttackingCropStat).bulletCount = bulletCount;
                }

                if (component is FarmingCropStat)
                {
                    (component as FarmingCropStat).bodyType = (FarmingCropStat.BodyType)(int)bodyType;
                    (component as FarmingCropStat).growingBody = growingBody;
                    (component as FarmingCropStat).totalGrowingTime = totalGrowingTime;
                    (component as FarmingCropStat).wateringTime = wateringTime;
                    (component as FarmingCropStat).requiredLevel = requiredLevel;
                    (component as FarmingCropStat).description = description;
                    (component as FarmingCropStat).gemEarnWhenHaverst = gemEarnWhenHaverst;
                }

                if (component is CraftedProductStat)
                {
                    (component as CraftedProductStat).recipe = recipe;
                    (component as CraftedProductStat).totalProducingTime = totalProducingTime;
                    (component as CraftedProductStat).cost = cost;
                }
            }
        }

        [Title("Base Collectable Object Attributes")]
        [InfoBox("A collectable object is set to Weapon Slot 3 by default. It will be assigned to Weapon Slot 1 or Weapon Slot 2 if it can be used in one of those respective slots.")]
        public ActiveWeapon.WeaponSlot weaponSlot;
        public GameObjectType.FilteredType filteredType;
        public GameObjectType.FeaturedType featuredType;

        public Sprite icon;

        public string baseId;
        public string collectableObjectName;
        public int maxCount;

        [InfoBox("Warning: It will be created later. List of object can store collectable object and its max quantity stored", InfoMessageType.Warning)]
        public int maxQuantityStored;

        public CollectableObjectStatComponent GetCollectableObjectStatComponent<T>() where T : CollectableObjectStatComponent
        {
            var component = components.OfType<T>().FirstOrDefault();
            if (component == null) throw new InvalidOperationException($"No component of type {typeof(T).Name} found.");

            return component;
        }

        /**/
        public int amplitudeGainImpulse;

        public int force;
        public float fireRate;

        //public Crop cropPrefab;

        public enum ShootingHandleType
        {
            None = -1,
            Raycast = 0,
            InstantiateBullet = 1,
            Both = 2
        }
        public ShootingHandleType shootingHandleType;
        public GameObject bulletObject;
       
        public enum BulletEffectComponent
        {
            None = -1,
            PlaceHole = 0,
            InstantiateTrail = 1,
            Both = 2
        }
        public BulletEffectComponent bulletEffectComponent;

        public GameObject trailTracer;
        public ParticleSystem hitEffectPrefab;

        public int damage;
        public int damageDealToSpecificEnemy;

        public enum ZoomType
        {
            NoZoom = 0,
            CanZoom = 1,
            HasScope = 2
        }
        public ZoomType zoomType;

        public int range;

        public float reloadSpeed;
        public float multiplierRecoilOnAim;
        public float multiplierForAmmo;
        public List<Vector2> recoildPattern;

        public int bulletCount = 1;
        public List<Vector3> bulletDirectionPattern;

        public int totalGrowingTime;

        public enum BodyType
        {
            None = -1,
            Tree = 0,
            Vegetable = 1,
            Climbing = 2
        }
        public BodyType bodyType;
        public int wateringTime;
        public int requiredLevel;
        public GameObject growingBody;

        public string description;
        public int gemEarnWhenHaverst;
        public float totalProducingTime;
        public RecipeData recipe;
        public int cost;
        /**/
    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Game.Rust;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Oxide.Plugins
{
    [Info("Chinook Drop Randomizer", "shinnova/Arainrr", "1.5.2")]
    [Description("Make the chinook drop location more random")]
    public class ChinookDropRandomizer : RustPlugin
    {
        private Dictionary<string, List<Vector3>> monumentList;

        private readonly Dictionary<string, float> defaultMonumentSizes = new Dictionary<string, float>()
        {
            ["Harbor"] = 125f,
            ["Giant Excavator Pit"] = 180f,
            ["Launch Site"] = 265f,
            ["Train Yard"] = 130f,
            ["Power Plant"] = 150f,
            ["Junkyard"] = 150f,
            ["Airfield"] = 200f,
            ["Water Treatment Plant"] = 190f,
            ["Bandit Camp"] = 80f,
            ["Sewer Branch"] = 80f,
            ["Oxum's Gas Station"] = 40f,
            ["Satellite Dish"] = 95f,
            ["Abandoned Supermarket"] = 30f,
            ["The Dome"] = 65f,
            ["Abandoned Cabins"] = 50f,
            ["Large Oil Rig"] = 100f,
            ["Oil Rig"] = 50f,
            ["Lighthouse"] = 40f,
            ["Outpost"] = 115f,
            ["HQM Quarry"] = 30f,
            ["Stone Quarry"] = 30f,
            ["Sulfur Quarry"] = 30f,
            ["Mining Outpost"] = 40f,
            ["Military Tunnel"] = 120f,
        };

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            if (!configData.blockDefaultDrop)
            {
                //Unsubscribe(nameof(CanHelicopterDropCrate));
            }
        }

        private void OnAIStateEnter(ChinookAIBrain.BasicAIState aiState,
            CH47HelicopterAIController chinook)
        {
            var message = $"OnAIStateEnter  {GetTypeName(aiState)} - {aiState.brain._currentState}";
            LogToFile($"{chinook.net.ID}", message, this, false);
            PrintWarning(message);
        }

        private void OnAIStateLeave(ChinookAIBrain.BasicAIState aiState,
            CH47HelicopterAIController chinook)
        {
            var message = $"OnAIStateLeave  {GetTypeName(aiState)} - {aiState.brain._currentState}";
            LogToFile($"{chinook.net.ID}", message, this, false);
            PrintWarning(message);
        }

        private void OnServerInitialized()
        {
            monumentList =
                TerrainMeta.Path?.Monuments?.Where(x => x.shouldDisplayOnMap)
                    .GroupBy(x => x.displayPhrase.english.Replace("\n", ""))
                    .ToDictionary(x => x.Key, y => y.Select(x => x.transform.position).ToList()) ??
                new Dictionary<string, List<Vector3>>();
            UpdateConfig();
            Subscribe(nameof(OnEntitySpawned));
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                OnEntitySpawned(baseNetworkable as CH47HelicopterAIController);
            }

            foreach (var ch47HelicopterAiController in BaseNetworkable.serverEntities.OfType<CH47HelicopterAIController>())
            {
                ch47HelicopterAiController.Kill();
            }
            var player = RustCore.FindPlayerById(76561198410133020);
            var chinook = GameManager.server.CreateEntity("assets/prefabs/npc/ch47/ch47scientists.entity.prefab") as CH47HelicopterAIController;
            chinook.TriggeredEventSpawn();
            if (false) Call(chinook);
            else chinook.Spawn();
            UnityEngine.Object.Destroy(chinook.GetComponent<CH47AIBrain>());
            var ch47AiBrain = chinook.gameObject.AddComponent<ChinookAIBrain>();
            ch47AiBrain.chinookOptions = new ChinookOptions();

            int i = 0;
            int lastState = 0;
            chinook.InvokeRepeating(() =>
            {
                i++;
                var message = $"{i} - CurrentState ({GetTypeName(ch47AiBrain.GetCurrentState())}) - {string.Join(", ", from aiState in ch47AiBrain.AIStates.Where(x => x != null) select $"{GetTypeName(aiState)}: {aiState.GetWeight()}")}";
                LogToFile($"{chinook.net.ID}", message, this, false);
                PrintError(message);
            }, 1f, 1f);
        }

        public void Call(CH47HelicopterAIController component)
        {
            Vector3 size = TerrainMeta.Size;
            CH47LandingZone closest = CH47LandingZone.GetClosest(component.transform.position);
            Vector3 zero = Vector3.zero;
            zero.y = closest.transform.position.y;
            Vector3 a = Vector3Ex.Direction2D(closest.transform.position, zero);
            Vector3 position = closest.transform.position + a * 200f;
            position.y = closest.transform.position.y;
            component.transform.position = position;
            component.SetLandingTarget(closest.transform.position);
            component.Spawn();
        }

        private static string GetTypeName(ChinookAIBrain.BasicAIState basicAiState)
        {
            return basicAiState.GetType().ToString().Replace("Oxide.Plugins.ChinookDropRandomizer+ChinookAIBrain+", "");
        }

        private void UpdateConfig()
        {
            foreach (var monumentName in monumentList.Keys)
            {
                float monumentSize;
                defaultMonumentSizes.TryGetValue(monumentName, out monumentSize);
                if (!configData.monumentsS.ContainsKey(monumentName))
                {
                    configData.monumentsS.Add(monumentName, new ConfigData.MonumentS { enabled = true, monumentSize = monumentSize });
                }
            }
            SaveConfig();
        }

        private void OnEntitySpawned(CH47HelicopterAIController chinook)
        {
            if (chinook == null) return;
            if (chinook.landingTarget != Vector3.zero || chinook.numCrates <= 0) return;
            //timer.Once(configData.dropDelay, () => TryDropCrate(chinook));
        }

        //private object CanHelicopterDropCrate(CH47HelicopterAIController chinook) => false;

        private static Vector3 GetGroundPosition(Vector3 position)
        {
            RaycastHit hitInfo;
            position.y -= 5f;
            position.y = Physics.Raycast(position, Vector3.down, out hitInfo, 300f, Rust.Layers.Solid)
                ? hitInfo.point.y
                : TerrainMeta.HeightMap.GetHeight(position);
            return position;
        }

        private static bool AboveWater(Vector3 location)
        {
            var groundPos = GetGroundPosition(location);
            return groundPos.y <= 0;
        }

        private bool AboveMonument(Vector3 location)
        {
            ConfigData.MonumentS monumentS;
            foreach (var entry in monumentList)
            {
                if (configData.monumentsS.TryGetValue(entry.Key, out monumentS) && monumentS.enabled)
                {
                    foreach (var monumentPos in entry.Value)
                    {
                        if (Vector3Ex.Distance2D(monumentPos, location) < monumentS.monumentSize)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void TryDropCrate(CH47HelicopterAIController chinook)
        {
            timer.Once(Random.Range(configData.minTime, configData.maxTime), () =>
            {
                if (chinook == null || chinook.IsDestroyed) return;
                if (chinook.numCrates > 0)
                {
                    if (!configData.checkWater || !AboveWater(chinook.transform.position))
                    {
                        if (!configData.checkMonument || !AboveMonument(chinook.transform.position))
                        {
                            if (BasePlayer.activePlayerList.Count >= configData.minPlayers)
                            {
                                chinook.DropCrate();
                                if (chinook.numCrates == 0) return;
                            }
                        }
                    }
                    TryDropCrate(chinook);
                }
            });
        }

        #region AI

        public class ChinookAIBrain : BaseAIBrain<CH47HelicopterAIController>
        {
            public const int ChinookState_Idle = 1;
            public const int ChinookState_Patrol = 2;
            public const int ChinookState_Land = 3;
            public const int ChinookState_DropCrate = 4;
            public const int ChinookState_Orbit = 5;
            public const int ChinookState_Egress = 6;

            public ChinookOptions chinookOptions;
            private float chinookAge;

            public override void InitializeAI()
            {
                base.InitializeAI();
                AIStates = new BasicAIState[7];
                AddState(new IdleState(), ChinookState_Idle);
                AddState(new PatrolState(), ChinookState_Patrol);
                AddState(new LandState(), ChinookState_Land);
                AddState(new DropCrateState(), ChinookState_DropCrate);
                AddState(new OrbitState(), ChinookState_Orbit);
                AddState(new EgressState(), ChinookState_Egress);
            }

            public void FixedUpdate()
            {
                if (baseEntity != null && !baseEntity.IsDestroyed)
                {
                    AIThink(Time.fixedDeltaTime);
                }
            }

            public override void AIThink(float delta)
            {
                chinookAge += delta;
                base.AIThink(delta);
            }

            public class ChinookBasicAIState : BasicAIState
            {
                private ChinookAIBrain _chinookAiBrain;
                protected ChinookOptions Options => Brain.chinookOptions;
                protected CH47HelicopterAIController Chinook => brain.GetEntity();

                protected ChinookAIBrain Brain
                {
                    get
                    {
                        if (_chinookAiBrain == null) _chinookAiBrain = brain.GetComponent<ChinookAIBrain>();
                        return _chinookAiBrain;
                    }
                }

                public override void StateEnter()
                {
                    Interface.Oxide.CallHook("OnAIStateEnter", this, Chinook);
                    base.StateEnter();
                }

                public override void StateLeave()
                {
                    base.StateLeave();
                    Interface.Oxide.CallHook("OnAIStateLeave", this, Chinook);
                }
            }

            //静止状态
            public class IdleState : ChinookBasicAIState
            {
                public override float GetWeight()
                {
                    return 0.1f;
                }

                public override void StateEnter()
                {
                    Chinook.SetMoveTarget(Chinook.GetPosition() + Chinook.rigidBody.velocity.normalized * 10f);
                    base.StateEnter();
                }
            }

            //巡逻状态
            public class PatrolState : ChinookBasicAIState
            {
                public List<Vector3> visitedPoints = new List<Vector3>();//访问点
                public static float patrolApproachDist = 75f;//巡逻方式的距离

                //在巡逻的目的地
                public bool AtPatrolDestination()
                {
                    return Vector3Ex.Distance2D(brain.mainInterestPoint, Chinook.GetPosition()) < patrolApproachDist;
                }

                public override bool CanInterrupt()
                {
                    if (base.CanInterrupt())
                    {
                        return AtPatrolDestination();
                    }
                    return false;
                }

                public override float GetWeight()
                {
                    if (IsInState())
                    {
                        //到达了巡逻目的地，并且时间大于两秒
                        if (AtPatrolDestination() && TimeInState() > 2f)
                        {
                            return 0f;
                        }
                        return 3f;
                    }
                    float num = Mathf.InverseLerp(70f, 120f, TimeSinceState()) * 5f;
                    return 1f + num;
                }

                public MonumentInfo GetRandomValidMonumentInfo()
                {
                    int count = TerrainMeta.Path.Monuments.Count;
                    int num = Random.Range(0, count);
                    for (int i = 0; i < count; i++)
                    {
                        int num2 = i + num;
                        if (num2 >= count)
                        {
                            num2 -= count;
                        }
                        MonumentInfo monumentInfo = TerrainMeta.Path.Monuments[num2];
                        if (monumentInfo.Type != 0 && monumentInfo.Type != MonumentType.WaterWell && monumentInfo.Tier != MonumentTier.Tier0)
                        {
                            return monumentInfo;
                        }
                    }
                    return null;
                }

                public Vector3 GetRandomPatrolPoint()
                {
                    MonumentInfo monumentInfo = null;
                    if (TerrainMeta.Path != null && TerrainMeta.Path.Monuments != null && TerrainMeta.Path.Monuments.Count > 0)
                    {
                        int count = TerrainMeta.Path.Monuments.Count;
                        int num = Random.Range(0, count);
                        for (int i = 0; i < count; i++)
                        {
                            int num2 = i + num;
                            if (num2 >= count)
                            {
                                num2 -= count;
                            }
                            MonumentInfo monumentInfo2 = TerrainMeta.Path.Monuments[num2];
                            if (monumentInfo2.Type != 0 && monumentInfo2.Type != MonumentType.WaterWell && monumentInfo2.Tier != MonumentTier.Tier0 && (monumentInfo2.Tier & MonumentTier.Tier0) <= (MonumentTier)0)
                            {
                                bool flag = false;
                                foreach (Vector3 visitedPoint in visitedPoints)
                                {
                                    if (Vector3Ex.Distance2D(monumentInfo2.transform.position, visitedPoint) < 100f)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    monumentInfo = monumentInfo2;
                                    break;
                                }
                            }
                        }
                        if (monumentInfo == null)
                        {
                            visitedPoints.Clear();
                            monumentInfo = GetRandomValidMonumentInfo();
                        }
                    }
                    Vector3 result;
                    if (monumentInfo != null)
                    {
                        visitedPoints.Add(monumentInfo.transform.position);
                        result = monumentInfo.transform.position;
                    }
                    else
                    {
                        float x = TerrainMeta.Size.x;
                        float y = 30f;
                        result = Vector3Ex.Range(-1f, 1f);
                        result.y = 0f;
                        result.Normalize();
                        result *= x * Random.Range(0f, 0.75f);
                        result.y = y;
                    }
                    return result;
                }

                public override void StateEnter()
                {
                    base.StateEnter();
                    Vector3 randomPatrolPoint = GetRandomPatrolPoint();
                    brain.mainInterestPoint = randomPatrolPoint;
                    float num = Mathf.Max(TerrainMeta.WaterMap.GetHeight(randomPatrolPoint), TerrainMeta.HeightMap.GetHeight(randomPatrolPoint));
                    float num2 = num;
                    RaycastHit hitInfo;
                    if (Physics.SphereCast(randomPatrolPoint + Vector3.up * 200f, 20f, Vector3.down, out hitInfo, 300f, 1218511105))
                    {
                        num2 = Mathf.Max(hitInfo.point.y, num);
                    }
                    brain.mainInterestPoint.y = num2 + 30f;
                }

                public override void StateThink(float delta)
                {
                    base.StateThink(delta);
                    Chinook.SetMoveTarget(brain.mainInterestPoint);
                }
            }

            //盘旋状态
            public class OrbitState : ChinookBasicAIState
            {
                public Vector3 GetOrbitCenter() => brain.mainInterestPoint;

                public override float GetWeight()
                {
                    if (IsInState())
                    {
                        float num = 1f - Mathf.InverseLerp(120f, 180f, TimeInState());
                        return 5f * num;
                    }
                    if (brain._currentState == ChinookState_Patrol && Vector3Ex.Distance2D(brain.mainInterestPoint, Chinook.GetPosition()) <= PatrolState.patrolApproachDist * 1.1f)
                    {
                        return 5f;
                    }
                    return 0f;
                }

                public override void StateEnter()
                {
                    Chinook.EnableFacingOverride(true);
                    Chinook.InitiateAnger();
                    base.StateEnter();
                }

                public override void StateThink(float delta)
                {
                    Vector3 orbitCenter = GetOrbitCenter();
                    CH47HelicopterAIController entity = Chinook;
                    Vector3 position = entity.GetPosition();
                    Vector3 vector = Vector3Ex.Direction2D(orbitCenter, position);
                    Vector3 vector2 = Vector3.Cross(Vector3.up, vector);
                    float d = Vector3.Dot(Vector3.Cross(entity.transform.right, Vector3.up), vector2) < 0f ? -1f : 1f;
                    float d2 = 75f;
                    Vector3 normalized = (-vector + vector2 * d * 0.6f).normalized;
                    Vector3 vector3 = orbitCenter + normalized * d2;
                    entity.SetMoveTarget(vector3);
                    entity.SetAimDirection(Vector3Ex.Direction2D(vector3, position));
                    base.StateThink(delta);
                }

                public override void StateLeave()
                {
                    Chinook.EnableFacingOverride(false);
                    Chinook.CancelAnger();
                    base.StateLeave();
                }
            }

            public class DropCrateState : ChinookBasicAIState
            {
                private float nextDropTime;

                public override bool CanInterrupt()
                {
                    if (base.CanInterrupt())
                    {
                        return !CanDrop();
                    }
                    return false;
                }

                public bool CanDrop()
                {
                    if (Time.time > nextDropTime)
                    {
                        return Chinook.CanDropCrate();
                    }
                    return false;
                }

                public override float GetWeight()
                {
                    if (!CanDrop())
                    {
                        return 0f;
                    }
                    if (IsInState())
                    {
                        return 10000f;
                    }
                    if (brain._currentState == ChinookState_Orbit && brain.GetCurrentState().TimeInState() > 60f)
                    {
                        var closest = CH47DropZone.GetClosest(brain.mainInterestPoint);
                        if (closest && Vector3Ex.Distance2D(closest.transform.position, brain.mainInterestPoint) < 200f)
                        {
                            var component = brain.GetComponent<ChinookAIBrain>();
                            if (component != null)
                            {
                                float num = Mathf.InverseLerp(300f, 600f, component.chinookAge);
                                return 1000f * num;
                            }
                        }
                    }
                    return 0f;
                }

                public override void StateEnter()
                {
                    Chinook.SetDropDoorOpen(true);
                    Chinook.EnableFacingOverride(false);
                    CH47DropZone closest = CH47DropZone.GetClosest(Chinook.transform.position);
                    if (closest == null)
                    {
                        nextDropTime = Time.time + 60f;
                    }
                    brain.mainInterestPoint = closest.transform.position;
                    Chinook.SetMoveTarget(brain.mainInterestPoint);
                    base.StateEnter();
                }

                public override void StateThink(float delta)
                {
                    base.StateThink(delta);
                    if (CanDrop() && Vector3Ex.Distance2D(brain.mainInterestPoint, Chinook.transform.position) < 5f && Chinook.rigidBody.velocity.magnitude < 5f)
                    {
                        Chinook.DropCrate();
                        nextDropTime = Time.time + 120f;
                    }
                }

                public override void StateLeave()
                {
                    Chinook.SetDropDoorOpen(false);
                    nextDropTime = Time.time + 60f;
                    base.StateLeave();
                }
            }

            public class EgressState : ChinookBasicAIState
            {
                private bool killing;
                private bool egressAltitudeAchieved;

                public override bool CanInterrupt()
                {
                    return false;
                }

                public override float GetWeight()
                {
                    if (Chinook.OutOfCrates() && !Chinook.ShouldLand())
                    {
                        return 10000f;
                    }
                    var component = brain.GetComponent<ChinookAIBrain>();
                    if (component != null)
                    {
                        if (!(component.chinookAge > Options.maxAge))
                        {
                            return 0f;
                        }
                        return 10000f;
                    }
                    return 0f;
                }

                public override void StateEnter()
                {
                    Chinook.EnableFacingOverride(false);
                    Transform transform = Chinook.transform;
                    Rigidbody rigidBody = Chinook.rigidBody;
                    Vector3 rhs = (rigidBody.velocity.magnitude < 0.1f) ? transform.forward : rigidBody.velocity.normalized;
                    Vector3 a = Vector3.Cross(Vector3.Cross(transform.up, rhs), Vector3.up);
                    brain.mainInterestPoint = transform.position + a * 8000f;
                    brain.mainInterestPoint.y = 100f;
                    Chinook.SetMoveTarget(brain.mainInterestPoint);
                    base.StateEnter();
                }

                public override void StateThink(float delta)
                {
                    base.StateThink(delta);
                    if (killing)
                    {
                        return;
                    }
                    var position = Chinook.GetPosition();
                    if (position.y < 85f && !egressAltitudeAchieved)
                    {
                        var closest = CH47LandingZone.GetClosest(position);
                        if (closest != null && Vector3Ex.Distance2D(closest.transform.position, position) < 20f)
                        {
                            float num = 0f;
                            if (TerrainMeta.HeightMap != null && TerrainMeta.WaterMap != null)
                            {
                                num = Mathf.Max(TerrainMeta.WaterMap.GetHeight(position), TerrainMeta.HeightMap.GetHeight(position));
                            }
                            num += 100f;
                            var moveTarget = position;
                            moveTarget.y = num;
                            Chinook.SetMoveTarget(moveTarget);
                            return;
                        }
                    }
                    egressAltitudeAchieved = true;
                    Chinook.SetMoveTarget(brain.mainInterestPoint);
                    if (TimeInState() > 300f)
                    {
                        Chinook.Invoke(Chinook.DelayedKill, 2f);
                        killing = true;
                    }
                }
            }

            //降落状态
            public class LandState : ChinookBasicAIState
            {
                private float landedForSeconds;
                private float lastLandtime;
                private float landingHeight = 20f;
                private float nextDismountTime;

                public override float GetWeight()
                {
                    if (!Chinook.ShouldLand())
                    {
                        return 0f;
                    }
                    float num = Time.time - lastLandtime;
                    if (IsInState() && landedForSeconds < 12f)
                    {
                        return 1000f;
                    }
                    if (!IsInState() && num > 10f)
                    {
                        return 9000f;
                    }
                    return 0f;
                }

                public override void StateThink(float delta)
                {
                    Vector3 position = Chinook.transform.position;
                    Vector3 forward = Chinook.transform.forward;
                    CH47LandingZone closest = CH47LandingZone.GetClosest(Chinook.landingTarget);
                    if (!closest)
                    {
                        return;
                    }
                    float magnitude = Chinook.rigidBody.velocity.magnitude;
                    Vector3.Distance(closest.transform.position, position);
                    float num = Vector3Ex.Distance2D(closest.transform.position, position);
                    Mathf.InverseLerp(1f, 20f, num);
                    bool enabled = num < 40f;
                    bool altitudeProtection = num > 15f && position.y < closest.transform.position.y + 10f;
                    Chinook.EnableFacingOverride(enabled);
                    Chinook.SetAltitudeProtection(altitudeProtection);
                    bool num2 = Mathf.Abs(closest.transform.position.y - position.y) < 3f && num <= 5f && magnitude < 1f;
                    if (num2)
                    {
                        landedForSeconds += delta;
                        if (lastLandtime == 0f)
                        {
                            lastLandtime = Time.time;
                        }
                    }
                    float num3 = 1f - Mathf.InverseLerp(0f, 7f, num);
                    landingHeight -= 4f * num3 * Time.deltaTime;
                    if (landingHeight < -5f)
                    {
                        landingHeight = -5f;
                    }
                    Chinook.SetAimDirection(closest.transform.forward);
                    Vector3 moveTarget = brain.mainInterestPoint + new Vector3(0f, landingHeight, 0f);
                    if (num < 100f && num > 15f)
                    {
                        Vector3 vector = Vector3Ex.Direction2D(closest.transform.position, position);
                        RaycastHit hitInfo;
                        if (Physics.SphereCast(position, 15f, vector, out hitInfo, num, 1218511105))
                        {
                            Vector3 a = Vector3.Cross(vector, Vector3.up);
                            moveTarget = hitInfo.point + a * 50f;
                        }
                    }
                    Chinook.SetMoveTarget(moveTarget);
                    if (num2)
                    {
                        if (landedForSeconds > 1f && Time.time > nextDismountTime)
                        {
                            foreach (BaseVehicle.MountPointInfo mountPoint in Chinook.mountPoints)
                            {
                                if ((bool)mountPoint.mountable && mountPoint.mountable.IsMounted())
                                {
                                    nextDismountTime = Time.time + 0.5f;
                                    mountPoint.mountable.DismountAllPlayers();
                                    break;
                                }
                            }
                        }
                        if (landedForSeconds > 8f)
                        {
                            brain.GetComponent<ChinookAIBrain>().chinookAge = float.PositiveInfinity;
                        }
                    }
                }

                public override void StateEnter()
                {
                    brain.mainInterestPoint = Chinook.landingTarget;
                    landingHeight = 15f;
                    base.StateEnter();
                }

                public override void StateLeave()
                {
                    Chinook.EnableFacingOverride(false);
                    Chinook.SetAltitudeProtection(true);
                    Chinook.SetMinHoverHeight(30f);
                    landedForSeconds = 0f;
                    base.StateLeave();
                }

                public override bool CanInterrupt()
                {
                    return true;
                }
            }
        }

        public class ChinookOptions
        {
            public float maxAge = 1800f;
        }

        #endregion AI

        #region ConfigurationFile

        private ConfigData configData;

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Prevent the game from handling chinook drops")]
            public bool blockDefaultDrop = false;

            [JsonProperty(PropertyName = "Time before chinook starts trying to drop (seconds)")]
            public float dropDelay = 200f;

            [JsonProperty(PropertyName = "Minimum time until drop (seconds)")]
            public float minTime = 50f;

            [JsonProperty(PropertyName = "Maximum time until drop (seconds)")]
            public float maxTime = 100f;

            [JsonProperty(PropertyName = "Minimum number of online players to drop")]
            public int minPlayers = 0;

            [JsonProperty(PropertyName = "Don't drop above water")]
            public bool checkWater = true;

            [JsonProperty(PropertyName = "Don't drop above monuments")]
            public bool checkMonument = false;

            [JsonProperty(PropertyName = "What monuments to check (only works if monument checking is enabled)")]
            public Dictionary<string, MonumentS> monumentsS = new Dictionary<string, MonumentS>();

            public class MonumentS
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled;

                [JsonProperty(PropertyName = "Monument size")]
                public float monumentSize;
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                    LoadDefaultConfig();
            }
            catch
            {
                PrintError("Config has corrupted or incorrectly formatted");
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            configData = new ConfigData();
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        #endregion ConfigurationFile
    }
}
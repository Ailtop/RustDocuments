#define UNITY_ASSERTIONS
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GrowableEntity : BaseCombatEntity, IInstanceDataReceiver
{
	public const float artificalLightQuality = 1f;

	public const float planterGroundModifierBase = 0.6f;

	public const float fertilizerGroundModifierBonus = 0.4f;

	public const float growthGeneSpeedMultiplier = 0.25f;

	public const float waterGeneRequirementMultiplier = 0.1f;

	public const float hardinessGeneModifierBonus = 0.2f;

	public const float hardinessGeneTemperatureModifierBonus = 0.05f;

	public const float baseYieldIncreaseMultiplier = 1f;

	public const float yieldGeneBonusMultiplier = 0.25f;

	public const float maxNonPlanterGroundQuality = 0.6f;

	public const float deathRatePerQuality = 0.1f;

	public TimeCachedValue<float> sunExposure;

	public TimeCachedValue<float> artificialLightExposure;

	public TimeCachedValue<float> artificialTemperatureExposure;

	private static Queue<GrowableEntity> queuedForUpdate = new Queue<GrowableEntity>();

	[ServerVar]
	[Help("How many miliseconds to budget for processing growable quality updates per frame")]
	public static float framebudgetms = 0.25f;

	public bool underWater;

	public int seasons;

	public int harvests;

	public float terrainTypeValue;

	public float yieldPool;

	public PlanterBox planter;

	public PlantProperties Properties;

	public ItemDefinition SourceItemDef;

	public float stageAge;

	public GrowableGenes Genes = new GrowableGenes();

	public const float startingHealth = 10f;

	public float CurrentTemperature
	{
		get
		{
			if (GetPlanter() != null)
			{
				return GetPlanter().GetPlantTemperature();
			}
			return Climate.GetTemperature(base.transform.position) + (artificialTemperatureExposure?.Get(false) ?? 0f);
		}
	}

	public PlantProperties.State State
	{
		get;
		set;
	}

	public float Age
	{
		get;
		private set;
	}

	public float LightQuality
	{
		get;
		set;
	}

	public float GroundQuality
	{
		get;
		set;
	} = 1f;


	public float WaterQuality
	{
		get;
		set;
	}

	public float WaterConsumption
	{
		get;
		set;
	}

	public bool Fertilized
	{
		get;
		set;
	}

	public float TemperatureQuality
	{
		get;
		set;
	}

	public float OverallQuality
	{
		get;
		set;
	}

	public float Yield
	{
		get;
		set;
	}

	public float StageProgressFraction => stageAge / currentStage.lifeLengthSeconds;

	public PlantProperties.Stage currentStage => Properties.stages[(int)State];

	public static float ThinkDeltaTime => ConVar.Server.planttick;

	public float growDeltaTime => ConVar.Server.planttick * ConVar.Server.planttickscale;

	public int CurrentPickAmount => Mathf.RoundToInt(CurrentPickAmountFloat);

	public float CurrentPickAmountFloat => (currentStage.resources + Yield) * (float)Properties.pickupMultiplier;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GrowableEntity.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 598660365 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - RPC_PickFruit ");
				}
				using (TimeWarning.New("RPC_PickFruit"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(598660365u, "RPC_PickFruit", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(598660365u, "RPC_PickFruit", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_PickFruit(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_PickFruit");
					}
				}
				return true;
			}
			if (rpc == 1959480148 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - RPC_RemoveDying ");
				}
				using (TimeWarning.New("RPC_RemoveDying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1959480148u, "RPC_RemoveDying", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_RemoveDying(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_RemoveDying");
					}
				}
				return true;
			}
			if (rpc == 2222960834u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - RPC_TakeClone ");
				}
				using (TimeWarning.New("RPC_TakeClone"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2222960834u, "RPC_TakeClone", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							RPC_TakeClone(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_TakeClone");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void QueueForQualityUpdate()
	{
		queuedForUpdate.Enqueue(this);
	}

	public static void BudgetedUpdate()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = framebudgetms / 1000f;
		while (queuedForUpdate.Count > 0 && UnityEngine.Time.realtimeSinceStartup < realtimeSinceStartup + num)
		{
			GrowableEntity growableEntity = queuedForUpdate.Dequeue();
			if (BaseEntityEx.IsValid(growableEntity))
			{
				growableEntity.CalculateQualities_Water();
				growableEntity.SendNetworkUpdate();
			}
		}
	}

	public void CalculateQualities(bool firstTime, bool forceArtificialLightUpdates = false, bool forceArtificialTemperatureUpdates = false)
	{
		if (!IsDead())
		{
			if (sunExposure == null)
			{
				sunExposure = new TimeCachedValue<float>
				{
					refreshCooldown = 30f,
					refreshRandomRange = 5f,
					updateValue = SunRaycast
				};
			}
			if (artificialLightExposure == null)
			{
				artificialLightExposure = new TimeCachedValue<float>
				{
					refreshCooldown = 60f,
					refreshRandomRange = 5f,
					updateValue = CalculateArtificialLightExposure
				};
			}
			if (artificialTemperatureExposure == null)
			{
				artificialTemperatureExposure = new TimeCachedValue<float>
				{
					refreshCooldown = 60f,
					refreshRandomRange = 5f,
					updateValue = CalculateArtificialTemperature
				};
			}
			if (forceArtificialTemperatureUpdates)
			{
				artificialTemperatureExposure.ForceNextRun();
			}
			CalculateLightQuality(forceArtificialLightUpdates | firstTime);
			CalculateWaterQuality();
			CalculateWaterConsumption();
			CalculateGroundQuality(firstTime);
			CalculateTemperatureQuality();
			CalculateOverallQuality();
		}
	}

	public void CalculateQualities_Water()
	{
		CalculateWaterQuality();
		CalculateWaterConsumption();
		CalculateOverallQuality();
	}

	public void CalculateLightQuality(bool forceArtificalUpdate)
	{
		float num = Mathf.Clamp01(Properties.timeOfDayHappiness.Evaluate(TOD_Sky.Instance.Cycle.Hour));
		if (!ConVar.Server.plantlightdetection)
		{
			LightQuality = num;
			return;
		}
		LightQuality = CalculateSunExposure(forceArtificalUpdate) * num;
		if (LightQuality <= 0f)
		{
			LightQuality = GetArtificialLightExposure(forceArtificalUpdate);
		}
		LightQuality = RemapValue(LightQuality, 0f, Properties.OptimalLightQuality, 0f, 1f);
	}

	public float CalculateSunExposure(bool force)
	{
		if (TOD_Sky.Instance.IsNight)
		{
			return 0f;
		}
		if (GetPlanter() != null)
		{
			return GetPlanter().GetSunExposure();
		}
		return sunExposure?.Get(force) ?? 0f;
	}

	public float SunRaycast()
	{
		return SunRaycast(base.transform.position + new Vector3(0f, 1f, 0f));
	}

	public float GetArtificialLightExposure(bool force)
	{
		if (GetPlanter() != null)
		{
			return GetPlanter().GetArtificialLightExposure();
		}
		return artificialLightExposure?.Get(force) ?? 0f;
	}

	private float CalculateArtificialLightExposure()
	{
		return CalculateArtificialLightExposure(base.transform);
	}

	public static float CalculateArtificialLightExposure(Transform forTransform)
	{
		float result = 0f;
		List<CeilingLight> obj = Facepunch.Pool.GetList<CeilingLight>();
		Vis.Entities(forTransform.position + new Vector3(0f, ConVar.Server.ceilingLightHeightOffset, 0f), ConVar.Server.ceilingLightGrowableRange, obj, 256);
		foreach (CeilingLight item in obj)
		{
			if (item.IsOn())
			{
				result = 1f;
				break;
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public static float SunRaycast(Vector3 checkPosition)
	{
		Vector3 normalized = (TOD_Sky.Instance.Components.Sun.transform.position - checkPosition).normalized;
		RaycastHit hitInfo;
		if (!UnityEngine.Physics.Raycast(checkPosition, normalized, out hitInfo, 100f, 10551297))
		{
			return 1f;
		}
		return 0f;
	}

	public void CalculateWaterQuality()
	{
		if (GetPlanter() != null)
		{
			float soilSaturationFraction = planter.soilSaturationFraction;
			if (soilSaturationFraction > ConVar.Server.optimalPlanterQualitySaturation)
			{
				WaterQuality = RemapValue(soilSaturationFraction, ConVar.Server.optimalPlanterQualitySaturation, 1f, 1f, 0.6f);
			}
			else
			{
				WaterQuality = RemapValue(soilSaturationFraction, 0f, ConVar.Server.optimalPlanterQualitySaturation, 0f, 1f);
			}
		}
		else
		{
			switch (TerrainMeta.BiomeMap.GetBiomeMaxType(base.transform.position))
			{
			case 8:
				WaterQuality = 0.1f;
				break;
			case 1:
			case 2:
			case 4:
				WaterQuality = 0.3f;
				break;
			default:
				WaterQuality = 0f;
				break;
			}
		}
		WaterQuality = Mathf.Clamp01(WaterQuality);
		WaterQuality = RemapValue(WaterQuality, 0f, Properties.OptimalWaterQuality, 0f, 1f);
	}

	public void CalculateGroundQuality(bool firstCheck)
	{
		if (underWater && !firstCheck)
		{
			GroundQuality = 0f;
			return;
		}
		if (firstCheck)
		{
			Vector3 position = base.transform.position;
			if (WaterLevel.Test(position, true, this))
			{
				underWater = true;
				GroundQuality = 0f;
				return;
			}
			underWater = false;
			terrainTypeValue = GetGroundTypeValue(position);
		}
		if (GetPlanter() != null)
		{
			GroundQuality = 0.6f;
			GroundQuality += (Fertilized ? 0.4f : 0f);
		}
		else
		{
			GroundQuality = terrainTypeValue;
			float num = (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Hardiness) * 0.2f;
			float b = GroundQuality + num;
			GroundQuality = Mathf.Min(0.6f, b);
		}
		GroundQuality = RemapValue(GroundQuality, 0f, Properties.OptimalGroundQuality, 0f, 1f);
	}

	public float GetGroundTypeValue(Vector3 pos)
	{
		switch (TerrainMeta.SplatMap.GetSplatMaxType(pos))
		{
		case 16:
			return 0.3f;
		case 2:
			return 0f;
		case 8:
			return 0f;
		case 64:
			return 0f;
		case 1:
			return 0.3f;
		case 32:
			return 0.2f;
		case 4:
			return 0f;
		case 128:
			return 0f;
		default:
			return 0.5f;
		}
	}

	public void CalculateTemperatureQuality()
	{
		TemperatureQuality = Mathf.Clamp01(Properties.temperatureHappiness.Evaluate(CurrentTemperature));
		float num = (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Hardiness) * 0.05f;
		TemperatureQuality = Mathf.Clamp01(TemperatureQuality + num);
		TemperatureQuality = RemapValue(TemperatureQuality, 0f, Properties.OptimalTemperatureQuality, 0f, 1f);
	}

	public float CalculateOverallQuality()
	{
		float a = 1f;
		if (ConVar.Server.useMinimumPlantCondition)
		{
			a = Mathf.Min(a, LightQuality);
			a = Mathf.Min(a, WaterQuality);
			a = Mathf.Min(a, GroundQuality);
			a = Mathf.Min(a, TemperatureQuality);
		}
		else
		{
			a = LightQuality * WaterQuality * GroundQuality * TemperatureQuality;
		}
		OverallQuality = a;
		return OverallQuality;
	}

	public void CalculateWaterConsumption()
	{
		float num = Properties.temperatureWaterRequirementMultiplier.Evaluate(CurrentTemperature);
		float num2 = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.WaterRequirement) * 0.1f;
		WaterConsumption = Properties.WaterIntake * num * num2;
	}

	private float CalculateArtificialTemperature()
	{
		return CalculateArtificialTemperature(base.transform);
	}

	public static float CalculateArtificialTemperature(Transform forTransform)
	{
		Vector3 position = forTransform.position;
		List<GrowableHeatSource> obj = Facepunch.Pool.GetList<GrowableHeatSource>();
		Vis.Components(position, ConVar.Server.artificialTemperatureGrowableRange, obj, 256);
		float num = 0f;
		foreach (GrowableHeatSource item in obj)
		{
			num = Mathf.Max(item.ApplyHeat(position), num);
		}
		Facepunch.Pool.FreeList(ref obj);
		return num;
	}

	public int CalculateMarketValue()
	{
		int baseMarketValue = Properties.BaseMarketValue;
		int num = Genes.GetPositiveGeneCount() * 10;
		int num2 = Genes.GetNegativeGeneCount() * -10;
		baseMarketValue += num;
		baseMarketValue += num2;
		return Mathf.Max(0, baseMarketValue);
	}

	private static float RemapValue(float inValue, float minA, float maxA, float minB, float maxB)
	{
		if (inValue >= maxA)
		{
			return maxB;
		}
		float t = Mathf.InverseLerp(minA, maxA, inValue);
		return Mathf.Lerp(minB, maxB, t);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(RunUpdate, ThinkDeltaTime, ThinkDeltaTime, ThinkDeltaTime * 0.1f);
		base.health = 10f;
		ResetSeason();
		Genes.GenerateRandom(this);
		if (!Rust.Application.isLoadingSave)
		{
			CalculateQualities(true);
		}
	}

	public PlanterBox GetPlanter()
	{
		if (planter == null)
		{
			BaseEntity parentEntity = GetParentEntity();
			if (parentEntity != null)
			{
				planter = (parentEntity as PlanterBox);
			}
		}
		return planter;
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		planter = (newParent as PlanterBox);
		if (!Rust.Application.isLoadingSave && planter != null)
		{
			planter.FertilizeGrowables();
		}
		CalculateQualities(true);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		CalculateQualities(true);
	}

	public void ResetSeason()
	{
		Yield = 0f;
		yieldPool = 0f;
	}

	public void RunUpdate()
	{
		if (!IsDead())
		{
			CalculateQualities(false);
			float overallQuality = CalculateOverallQuality();
			float actualStageAgeIncrease = UpdateAge(overallQuality);
			UpdateHealthAndYield(overallQuality, actualStageAgeIncrease);
			if (base.health <= 0f)
			{
				Die();
				return;
			}
			UpdateState();
			ConsumeWater();
			SendNetworkUpdate();
		}
	}

	public float UpdateAge(float overallQuality)
	{
		Age += growDeltaTime;
		float num = currentStage.IgnoreConditions ? 1f : (Mathf.Max(overallQuality, 0f) * GetGrowthBonus(overallQuality));
		float num2 = growDeltaTime * num;
		stageAge += num2;
		return num2;
	}

	public void UpdateHealthAndYield(float overallQuality, float actualStageAgeIncrease)
	{
		if (GetPlanter() == null && UnityEngine.Random.Range(0f, 1f) <= ConVar.Server.nonPlanterDeathChancePerTick)
		{
			base.health = 0f;
			return;
		}
		if (overallQuality <= 0f)
		{
			ApplyDeathRate();
		}
		base.health += overallQuality * currentStage.health * growDeltaTime;
		if (yieldPool > 0f)
		{
			float num = currentStage.yield / (currentStage.lifeLengthSeconds / growDeltaTime);
			float num2 = Mathf.Min(yieldPool, num * (actualStageAgeIncrease / growDeltaTime));
			yieldPool -= num;
			float num3 = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield) * 0.25f;
			Yield += num2 * 1f * num3;
		}
	}

	public void ApplyDeathRate()
	{
		float num = 0f;
		if (WaterQuality <= 0f)
		{
			num += 0.1f;
		}
		if (LightQuality <= 0f)
		{
			num += 0.1f;
		}
		if (GroundQuality <= 0f)
		{
			num += 0.1f;
		}
		if (TemperatureQuality <= 0f)
		{
			num += 0.1f;
		}
		base.health -= num;
	}

	public float GetGrowthBonus(float overallQuality)
	{
		float result = 1f + (float)Genes.GetGeneTypeCount(GrowableGenetics.GeneType.GrowthSpeed) * 0.25f;
		if (overallQuality <= 0f)
		{
			result = 1f;
		}
		return result;
	}

	public PlantProperties.State UpdateState()
	{
		if (stageAge <= currentStage.lifeLengthSeconds)
		{
			return State;
		}
		if (State == PlantProperties.State.Dying)
		{
			Die();
			return PlantProperties.State.Dying;
		}
		if (currentStage.nextState <= State)
		{
			seasons++;
		}
		if (seasons >= Properties.MaxSeasons)
		{
			ChangeState(PlantProperties.State.Dying, true);
		}
		else
		{
			ChangeState(currentStage.nextState, true);
		}
		return State;
	}

	public void ConsumeWater()
	{
		if (State != PlantProperties.State.Dying && !(GetPlanter() == null))
		{
			int num = Mathf.CeilToInt(Mathf.Min(planter.soilSaturation, WaterConsumption));
			if ((float)num > 0f)
			{
				planter.ConsumeWater(num, this);
			}
		}
	}

	public void Fertilize()
	{
		if (!Fertilized)
		{
			Fertilized = true;
			CalculateQualities(false);
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_TakeClone(RPCMessage msg)
	{
		TakeClones(msg.player);
	}

	public void TakeClones(BasePlayer player)
	{
		if (player == null || !CanClone() || Interface.CallHook("CanTakeCutting", player, this) != null)
		{
			return;
		}
		int num = Properties.BaseCloneCount + Genes.GetGeneTypeCount(GrowableGenetics.GeneType.Yield) / 2;
		if (num > 0)
		{
			Item item = ItemManager.Create(Properties.CloneItem, num, 0uL);
			GrowableGeneEncoding.EncodeGenesToItem(this, item);
			player.GiveItem(item, GiveItemReason.PickedUp);
			if (Properties.pickEffect.isValid)
			{
				Effect.server.Run(Properties.pickEffect.resourcePath, base.transform.position, Vector3.up);
			}
			Die();
		}
	}

	public void PickFruit(BasePlayer player)
	{
		if (!CanPick() || Interface.CallHook("OnGrowableGather", this, player) != null)
		{
			return;
		}
		harvests++;
		GiveFruit(player, CurrentPickAmount);
		ResetSeason();
		if (Properties.pickEffect.isValid)
		{
			Effect.server.Run(Properties.pickEffect.resourcePath, base.transform.position, Vector3.up);
		}
		if (harvests >= Properties.maxHarvests)
		{
			if (Properties.disappearAfterHarvest)
			{
				Die();
			}
			else
			{
				ChangeState(PlantProperties.State.Dying, true);
			}
		}
		else
		{
			ChangeState(PlantProperties.State.Mature, true);
		}
	}

	public void GiveFruit(BasePlayer player, int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		bool enabled = Properties.pickupItem.condition.enabled;
		if (enabled)
		{
			for (int i = 0; i < amount; i++)
			{
				GiveFruit(player, 1, enabled);
			}
		}
		else
		{
			GiveFruit(player, amount, enabled);
		}
	}

	public void GiveFruit(BasePlayer player, int amount, bool applyCondition)
	{
		Item item = ItemManager.Create(Properties.pickupItem, amount, 0uL);
		if (applyCondition)
		{
			item.conditionNormalized = Properties.fruitVisualScaleCurve.Evaluate(StageProgressFraction);
		}
		if (player != null)
		{
			Interface.CallHook("OnGrowableGathered", this, item, player);
			player.GiveItem(item, GiveItemReason.PickedUp);
		}
		else
		{
			item.Drop(base.transform.position + Vector3.up * 0.5f, Vector3.up * 1f);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_PickFruit(RPCMessage msg)
	{
		PickFruit(msg.player);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_RemoveDying(RPCMessage msg)
	{
		RemoveDying(msg.player);
	}

	public void RemoveDying(BasePlayer receiver)
	{
		if (State == PlantProperties.State.Dying && !(Properties.removeDyingItem == null) && Interface.CallHook("OnRemoveDying", this, receiver) == null)
		{
			if (Properties.removeDyingEffect.isValid)
			{
				Effect.server.Run(Properties.removeDyingEffect.resourcePath, base.transform.position, Vector3.up);
			}
			Item item = ItemManager.Create(Properties.removeDyingItem, 1, 0uL);
			if (receiver != null)
			{
				receiver.GiveItem(item, GiveItemReason.PickedUp);
			}
			else
			{
				item.Drop(base.transform.position + Vector3.up * 0.5f, Vector3.up * 1f);
			}
			Die();
		}
	}

	[ServerVar(ServerAdmin = true)]
	public static void GrowAll(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.IsAdmin)
		{
			List<GrowableEntity> obj = Facepunch.Pool.GetList<GrowableEntity>();
			Vis.Entities(basePlayer.ServerPosition, 6f, obj);
			foreach (GrowableEntity item in obj)
			{
				if (item.isServer)
				{
					item.ChangeState(item.currentStage.nextState, false);
				}
			}
			Facepunch.Pool.FreeList(ref obj);
		}
	}

	public void ReceiveInstanceData(ProtoBuf.Item.InstanceData data)
	{
		GrowableGeneEncoding.DecodeIntToGenes(data.dataInt, Genes);
		GrowableGeneEncoding.DecodeIntToPreviousGenes(data.dataInt, Genes);
	}

	public override void ResetState()
	{
		base.ResetState();
		State = PlantProperties.State.Seed;
	}

	public bool CanPick()
	{
		return currentStage.resources > 0f;
	}

	public bool CanTakeSeeds()
	{
		if (currentStage.resources > 0f)
		{
			return Properties.SeedItem != null;
		}
		return false;
	}

	public bool CanClone()
	{
		if (currentStage.resources > 0f)
		{
			return Properties.CloneItem != null;
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.growableEntity = Facepunch.Pool.Get<ProtoBuf.GrowableEntity>();
		info.msg.growableEntity.state = (int)State;
		info.msg.growableEntity.totalAge = Age;
		info.msg.growableEntity.stageAge = stageAge;
		info.msg.growableEntity.yieldFraction = Yield;
		info.msg.growableEntity.yieldPool = yieldPool;
		info.msg.growableEntity.fertilized = Fertilized;
		if (Genes != null)
		{
			Genes.Save(info);
		}
		if (!info.forDisk)
		{
			info.msg.growableEntity.lightModifier = LightQuality;
			info.msg.growableEntity.groundModifier = GroundQuality;
			info.msg.growableEntity.waterModifier = WaterQuality;
			info.msg.growableEntity.happiness = OverallQuality;
			info.msg.growableEntity.temperatureModifier = TemperatureQuality;
			info.msg.growableEntity.waterConsumption = WaterConsumption;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.growableEntity != null)
		{
			Age = info.msg.growableEntity.totalAge;
			stageAge = info.msg.growableEntity.stageAge;
			Yield = info.msg.growableEntity.yieldFraction;
			Fertilized = info.msg.growableEntity.fertilized;
			yieldPool = info.msg.growableEntity.yieldPool;
			Genes.Load(info);
			ChangeState((PlantProperties.State)info.msg.growableEntity.state, false, true);
		}
		else
		{
			Genes.GenerateRandom(this);
		}
	}

	public void ChangeState(PlantProperties.State state, bool resetAge, bool loading = false)
	{
		if (base.isServer && State == state)
		{
			return;
		}
		State = state;
		if (!base.isServer)
		{
			return;
		}
		if (!loading)
		{
			if (currentStage.resources > 0f)
			{
				yieldPool = currentStage.yield;
			}
			if (state == PlantProperties.State.Crossbreed)
			{
				if (Properties.CrossBreedEffect.isValid)
				{
					Effect.server.Run(Properties.CrossBreedEffect.resourcePath, base.transform.position, Vector3.up);
				}
				GrowableGenetics.CrossBreed(this);
			}
			SendNetworkUpdate();
		}
		if (resetAge)
		{
			stageAge = 0f;
		}
	}
}

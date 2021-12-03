using System;
using Oxide.Core;
using Rust;
using Rust.Modular;
using UnityEngine;

public class VehicleModuleEngine : VehicleModuleStorage
{
	[Serializable]
	public class Engine
	{
		[Header("Engine Stats")]
		public int engineKW;

		public float idleFuelPerSec = 0.25f;

		public float maxFuelPerSec = 0.25f;

		[Header("Engine Audio")]
		public EngineAudioSet audioSet;

		[Header("Engine FX")]
		public ParticleSystemContainer[] engineParticles;

		public ParticleSystem[] exhaustSmoke;

		public ParticleSystem[] exhaustBackfire;

		public float exhaustSmokeMinOpacity = 0.1f;

		public float exhaustSmokeMaxOpacity = 0.7f;

		public float exhaustSmokeChangeRate = 0.5f;
	}

	[SerializeField]
	public Engine engine;

	private const float FORCE_MULTIPLIER = 12.75f;

	private const float HEALTH_PERFORMANCE_FRACTION = 0.25f;

	private const float LOW_PERFORMANCE_THRESHOLD = 0.5f;

	private Sound badPerformanceLoop;

	private SoundModulation.Modulator badPerformancePitchModulator;

	private float prevSmokePercent;

	private const float MIN_FORCE_BIAS = 0.0002f;

	private const float MAX_FORCE_BIAS = 0.7f;

	public override bool HasAnEngine => true;

	public bool IsUsable { get; set; }

	public float PerformanceFractionAcceleration { get; set; }

	public float PerformanceFractionTopSpeed { get; set; }

	public float PerformanceFractionFuelEconomy { get; set; }

	public float OverallPerformanceFraction { get; set; }

	public bool AtLowPerformance => OverallPerformanceFraction <= 0.5f;

	public int KW => engine.engineKW;

	public EngineAudioSet AudioSet => engine.audioSet;

	private bool EngineIsOn
	{
		get
		{
			if (base.Car != null)
			{
				return base.Car.CurEngineState == VehicleEngineController<ModularCar>.EngineState.On;
			}
			return false;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		RefreshPerformanceStats(GetContainer() as EngineStorage);
	}

	public override void OnEngineStateChanged(VehicleEngineController<ModularCar>.EngineState oldState, VehicleEngineController<ModularCar>.EngineState newState)
	{
		base.OnEngineStateChanged(oldState, newState);
		RefreshPerformanceStats(GetContainer() as EngineStorage);
	}

	public override float GetMaxDriveForce()
	{
		if (!IsUsable)
		{
			return 0f;
		}
		return (float)engine.engineKW * 12.75f * PerformanceFractionTopSpeed;
	}

	public void RefreshPerformanceStats(EngineStorage engineStorage)
	{
		if (Interface.CallHook("OnEngineStatsRefresh", this, engineStorage) == null)
		{
			if (engineStorage == null)
			{
				IsUsable = false;
				PerformanceFractionAcceleration = 0f;
				PerformanceFractionTopSpeed = 0f;
				PerformanceFractionFuelEconomy = 0f;
			}
			else
			{
				IsUsable = engineStorage.isUsable;
				PerformanceFractionAcceleration = GetPerformanceFraction(engineStorage.accelerationBoostPercent);
				PerformanceFractionTopSpeed = GetPerformanceFraction(engineStorage.topSpeedBoostPercent);
				PerformanceFractionFuelEconomy = GetPerformanceFraction(engineStorage.fuelEconomyBoostPercent);
			}
			OverallPerformanceFraction = (PerformanceFractionAcceleration + PerformanceFractionTopSpeed + PerformanceFractionFuelEconomy) / 3f;
			Interface.CallHook("OnEngineStatsRefreshed", this, engineStorage);
		}
	}

	public float GetPerformanceFraction(float statBoostPercent)
	{
		if (!IsUsable)
		{
			return 0f;
		}
		float num = Mathf.Lerp(0f, 0.25f, base.healthFraction);
		float num2 = ((base.healthFraction != 0f) ? (statBoostPercent * 0.75f) : 0f);
		return num + num2;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		RefreshPerformanceStats(GetContainer() as EngineStorage);
	}

	public override void VehicleFixedUpdate()
	{
		if (isSpawned && base.IsOnAVehicle)
		{
			base.VehicleFixedUpdate();
			if (base.Vehicle.IsMovingOrOn && !(base.Car == null) && base.Car.CurEngineState == VehicleEngineController<ModularCar>.EngineState.On)
			{
				float num = Mathf.Lerp(engine.idleFuelPerSec, engine.maxFuelPerSec, Mathf.Abs(base.Car.GetThrottleInput()));
				num /= PerformanceFractionFuelEconomy;
				base.Car.TickFuel(num);
			}
		}
	}

	public override float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float maxDriveForce = GetMaxDriveForce();
		float num = BiasedLerp(bias: Mathf.Lerp(0.0002f, 0.7f, PerformanceFractionAcceleration), x: 1f - absSpeed / topSpeed);
		return maxDriveForce * num;
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (info.damageTypes.GetMajorityDamageType() != DamageType.Decay)
		{
			float num = info.damageTypes.Total();
			EngineStorage engineStorage = GetContainer() as EngineStorage;
			if (engineStorage != null && num > 0f)
			{
				engineStorage.OnModuleDamaged(num);
			}
		}
	}

	public override void OnHealthChanged(float oldValue, float newValue)
	{
		base.OnHealthChanged(oldValue, newValue);
		if (base.isServer)
		{
			RefreshPerformanceStats(GetContainer() as EngineStorage);
		}
	}

	public override void AdminFixUp(int tier)
	{
		base.AdminFixUp(tier);
		EngineStorage engineStorage = GetContainer() as EngineStorage;
		engineStorage.AdminAddParts(tier);
		RefreshPerformanceStats(engineStorage);
	}

	private float BiasedLerp(float x, float bias)
	{
		float num = ((!(bias <= 0.5f)) ? (1f - Bias(1f - Mathf.Abs(x), 1f - bias)) : Bias(Mathf.Abs(x), bias));
		if (!(x < 0f))
		{
			return num;
		}
		return 0f - num;
	}

	private float Bias(float x, float bias)
	{
		if (x <= 0f || bias <= 0f)
		{
			return 0f;
		}
		if (x >= 1f || bias >= 1f)
		{
			return 1f;
		}
		if (bias == 0.5f)
		{
			return x;
		}
		float p = Mathf.Log(bias) * -1.4427f;
		return Mathf.Pow(x, p);
	}
}

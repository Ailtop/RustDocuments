using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class Time : BaseHandler<AppEmpty>
{
	public override void Execute()
	{
		TOD_Sky instance = TOD_Sky.Instance;
		TOD_Time time = instance.Components.Time;
		AppTime appTime = Pool.Get<AppTime>();
		appTime.dayLengthMinutes = time.DayLengthInMinutes;
		appTime.timeScale = (time.ProgressTime ? UnityEngine.Time.timeScale : 0f);
		appTime.sunrise = instance.SunriseTime;
		appTime.sunset = instance.SunsetTime;
		appTime.time = instance.Cycle.Hour;
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.time = appTime;
		Send(appResponse);
	}
}

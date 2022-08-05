using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Network;
using Rust.Ai;
using UnityEngine;

public class ConsoleGen
{
	public static ConsoleSystem.Command[] All = new ConsoleSystem.Command[858]
	{
		new ConsoleSystem.Command
		{
			Name = "humanknownplayerslosupdateinterval",
			Parent = "aibrainsenses",
			FullName = "aibrainsenses.humanknownplayerslosupdateinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIBrainSenses.HumanKnownPlayersLOSUpdateInterval.ToString(),
			SetOveride = delegate(string str)
			{
				AIBrainSenses.HumanKnownPlayersLOSUpdateInterval = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "knownplayerslosupdateinterval",
			Parent = "aibrainsenses",
			FullName = "aibrainsenses.knownplayerslosupdateinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIBrainSenses.KnownPlayersLOSUpdateInterval.ToString(),
			SetOveride = delegate(string str)
			{
				AIBrainSenses.KnownPlayersLOSUpdateInterval = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "updateinterval",
			Parent = "aibrainsenses",
			FullName = "aibrainsenses.updateinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIBrainSenses.UpdateInterval.ToString(),
			SetOveride = delegate(string str)
			{
				AIBrainSenses.UpdateInterval = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "animalframebudgetms",
			Parent = "aithinkmanager",
			FullName = "aithinkmanager.animalframebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIThinkManager.animalframebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				AIThinkManager.animalframebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "framebudgetms",
			Parent = "aithinkmanager",
			FullName = "aithinkmanager.framebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIThinkManager.framebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				AIThinkManager.framebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "petframebudgetms",
			Parent = "aithinkmanager",
			FullName = "aithinkmanager.petframebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AIThinkManager.petframebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				AIThinkManager.petframebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "generate_paths",
			Parent = "baseboat",
			FullName = "baseboat.generate_paths",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseBoat.generate_paths.ToString(),
			SetOveride = delegate(string str)
			{
				BaseBoat.generate_paths = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxactivefireworks",
			Parent = "basefirework",
			FullName = "basefirework.maxactivefireworks",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseFirework.maxActiveFireworks.ToString(),
			SetOveride = delegate(string str)
			{
				BaseFirework.maxActiveFireworks = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forcefail",
			Parent = "basefishingrod",
			FullName = "basefishingrod.forcefail",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseFishingRod.ForceFail.ToString(),
			SetOveride = delegate(string str)
			{
				BaseFishingRod.ForceFail = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forcesuccess",
			Parent = "basefishingrod",
			FullName = "basefishingrod.forcesuccess",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseFishingRod.ForceSuccess.ToString(),
			SetOveride = delegate(string str)
			{
				BaseFishingRod.ForceSuccess = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "immediatehook",
			Parent = "basefishingrod",
			FullName = "basefishingrod.immediatehook",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseFishingRod.ImmediateHook.ToString(),
			SetOveride = delegate(string str)
			{
				BaseFishingRod.ImmediateHook = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "missionsenabled",
			Parent = "basemission",
			FullName = "basemission.missionsenabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseMission.missionsenabled.ToString(),
			SetOveride = delegate(string str)
			{
				BaseMission.missionsenabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "basenavmovementframeinterval",
			Parent = "basenavigator",
			FullName = "basenavigator.basenavmovementframeinterval",
			ServerAdmin = true,
			Description = "How many frames between base navigation movement updates",
			Variable = true,
			GetOveride = () => BaseNavigator.baseNavMovementFrameInterval.ToString(),
			SetOveride = delegate(string str)
			{
				BaseNavigator.baseNavMovementFrameInterval = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxstepupdistance",
			Parent = "basenavigator",
			FullName = "basenavigator.maxstepupdistance",
			ServerAdmin = true,
			Description = "The max step-up height difference for pet base navigation",
			Variable = true,
			GetOveride = () => BaseNavigator.maxStepUpDistance.ToString(),
			SetOveride = delegate(string str)
			{
				BaseNavigator.maxStepUpDistance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "navtypedistance",
			Parent = "basenavigator",
			FullName = "basenavigator.navtypedistance",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseNavigator.navTypeDistance.ToString(),
			SetOveride = delegate(string str)
			{
				BaseNavigator.navTypeDistance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "navtypeheightoffset",
			Parent = "basenavigator",
			FullName = "basenavigator.navtypeheightoffset",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseNavigator.navTypeHeightOffset.ToString(),
			SetOveride = delegate(string str)
			{
				BaseNavigator.navTypeHeightOffset = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stucktriggerduration",
			Parent = "basenavigator",
			FullName = "basenavigator.stucktriggerduration",
			ServerAdmin = true,
			Description = "How long we are not moving for before trigger the stuck event",
			Variable = true,
			GetOveride = () => BaseNavigator.stuckTriggerDuration.ToString(),
			SetOveride = delegate(string str)
			{
				BaseNavigator.stuckTriggerDuration = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "movementupdatebudgetms",
			Parent = "basepet",
			FullName = "basepet.movementupdatebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BasePet.movementupdatebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				BasePet.movementupdatebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "onlyqueuebasenavmovements",
			Parent = "basepet",
			FullName = "basepet.onlyqueuebasenavmovements",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BasePet.onlyQueueBaseNavMovements.ToString(),
			SetOveride = delegate(string str)
			{
				BasePet.onlyQueueBaseNavMovements = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "queuedmovementsallowed",
			Parent = "basepet",
			FullName = "basepet.queuedmovementsallowed",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BasePet.queuedMovementsAllowed.ToString(),
			SetOveride = delegate(string str)
			{
				BasePet.queuedMovementsAllowed = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "lifestoryframebudgetms",
			Parent = "baseplayer",
			FullName = "baseplayer.lifestoryframebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BasePlayer.lifeStoryFramebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				BasePlayer.lifeStoryFramebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "decayminutes",
			Parent = "baseridableanimal",
			FullName = "baseridableanimal.decayminutes",
			ServerAdmin = true,
			Description = "How long before a horse dies unattended",
			Variable = true,
			GetOveride = () => BaseRidableAnimal.decayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				BaseRidableAnimal.decayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "dungtimescale",
			Parent = "baseridableanimal",
			FullName = "baseridableanimal.dungtimescale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseRidableAnimal.dungTimeScale.ToString(),
			SetOveride = delegate(string str)
			{
				BaseRidableAnimal.dungTimeScale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "framebudgetms",
			Parent = "baseridableanimal",
			FullName = "baseridableanimal.framebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BaseRidableAnimal.framebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				BaseRidableAnimal.framebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deepwaterdecayminutes",
			Parent = "basesubmarine",
			FullName = "basesubmarine.deepwaterdecayminutes",
			ServerAdmin = true,
			Description = "How long before a submarine loses all its health while in deep water",
			Variable = true,
			GetOveride = () => BaseSubmarine.deepwaterdecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				BaseSubmarine.deepwaterdecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "basesubmarine",
			FullName = "basesubmarine.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a submarine loses all its health while outside. If it's in deep water, deepwaterdecayminutes is used",
			Variable = true,
			GetOveride = () => BaseSubmarine.outsidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				BaseSubmarine.outsidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "oxygenminutes",
			Parent = "basesubmarine",
			FullName = "basesubmarine.oxygenminutes",
			ServerAdmin = true,
			Description = "How long a submarine can stay underwater until players start taking damage from low oxygen",
			Variable = true,
			GetOveride = () => BaseSubmarine.oxygenminutes.ToString(),
			SetOveride = delegate(string str)
			{
				BaseSubmarine.oxygenminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "bear",
			FullName = "bear.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Bear.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Bear.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spinfrequencyseconds",
			Parent = "bigwheelgame",
			FullName = "bigwheelgame.spinfrequencyseconds",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => BigWheelGame.spinFrequencySeconds.ToString(),
			SetOveride = delegate(string str)
			{
				BigWheelGame.spinFrequencySeconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "boar",
			FullName = "boar.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Boar.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Boar.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "backtracklength",
			Parent = "boombox",
			FullName = "boombox.backtracklength",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => BoomBox.BacktrackLength.ToString(),
			SetOveride = delegate(string str)
			{
				BoomBox.BacktrackLength = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearradiobyuser",
			Parent = "boombox",
			FullName = "boombox.clearradiobyuser",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				BoomBox.ClearRadioByUser(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "serverurllist",
			Parent = "boombox",
			FullName = "boombox.serverurllist",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Saved = true,
			Description = "A list of radio stations that are valid on this server. Format: NAME,URL,NAME,URL,etc",
			Replicated = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => BoomBox.ServerUrlList.ToString(),
			SetOveride = delegate(string str)
			{
				BoomBox.ServerUrlList = str;
			},
			Default = ""
		},
		new ConsoleSystem.Command
		{
			Name = "egress_duration_minutes",
			Parent = "cargoship",
			FullName = "cargoship.egress_duration_minutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CargoShip.egress_duration_minutes.ToString(),
			SetOveride = delegate(string str)
			{
				CargoShip.egress_duration_minutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "event_duration_minutes",
			Parent = "cargoship",
			FullName = "cargoship.event_duration_minutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CargoShip.event_duration_minutes.ToString(),
			SetOveride = delegate(string str)
			{
				CargoShip.event_duration_minutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "event_enabled",
			Parent = "cargoship",
			FullName = "cargoship.event_enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CargoShip.event_enabled.ToString(),
			SetOveride = delegate(string str)
			{
				CargoShip.event_enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "loot_round_spacing_minutes",
			Parent = "cargoship",
			FullName = "cargoship.loot_round_spacing_minutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CargoShip.loot_round_spacing_minutes.ToString(),
			SetOveride = delegate(string str)
			{
				CargoShip.loot_round_spacing_minutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "loot_rounds",
			Parent = "cargoship",
			FullName = "cargoship.loot_rounds",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CargoShip.loot_rounds.ToString(),
			SetOveride = delegate(string str)
			{
				CargoShip.loot_rounds = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearcassettes",
			Parent = "cassette",
			FullName = "cassette.clearcassettes",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Cassette.ClearCassettes(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearcassettesbyuser",
			Parent = "cassette",
			FullName = "cassette.clearcassettesbyuser",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Cassette.ClearCassettesByUser(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxcassettefilesizemb",
			Parent = "cassette",
			FullName = "cassette.maxcassettefilesizemb",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Cassette.MaxCassetteFileSizeMB.ToString(),
			SetOveride = delegate(string str)
			{
				Cassette.MaxCassetteFileSizeMB = str.ToFloat();
			},
			Default = "5"
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "chicken",
			FullName = "chicken.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Chicken.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Chicken.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "hideobjects",
			Parent = "cinematicentity",
			FullName = "cinematicentity.hideobjects",
			ServerAdmin = true,
			Description = "Hides cinematic light source meshes (keeps lights visible)",
			Variable = true,
			GetOveride = () => CinematicEntity.HideObjects.ToString(),
			SetOveride = delegate(string str)
			{
				CinematicEntity.HideObjects = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clothloddist",
			Parent = "clothlod",
			FullName = "clothlod.clothloddist",
			ServerAdmin = true,
			Description = "distance cloth will simulate until",
			Variable = true,
			GetOveride = () => ClothLOD.clothLODDist.ToString(),
			SetOveride = delegate(string str)
			{
				ClothLOD.clothLODDist = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "lockoutcooldown",
			Parent = "codelock",
			FullName = "codelock.lockoutcooldown",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CodeLock.lockoutCooldown.ToString(),
			SetOveride = delegate(string str)
			{
				CodeLock.lockoutCooldown = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxfailedattempts",
			Parent = "codelock",
			FullName = "codelock.maxfailedattempts",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => CodeLock.maxFailedAttempts.ToString(),
			SetOveride = delegate(string str)
			{
				CodeLock.maxFailedAttempts = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "echo",
			Parent = "commands",
			FullName = "commands.echo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Commands.Echo(arg.FullString);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find",
			Parent = "commands",
			FullName = "commands.find",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Commands.Find(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_deleteugccontent",
			Parent = "global",
			FullName = "global.adminui_deleteugccontent",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_DeleteUGCContent(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_fullrefresh",
			Parent = "global",
			FullName = "global.adminui_fullrefresh",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_FullRefresh(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestfireworkpattern",
			Parent = "global",
			FullName = "global.adminui_requestfireworkpattern",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestFireworkPattern(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestplayerlist",
			Parent = "global",
			FullName = "global.adminui_requestplayerlist",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestPlayerList(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestserverconvars",
			Parent = "global",
			FullName = "global.adminui_requestserverconvars",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestServerConvars(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestserverinfo",
			Parent = "global",
			FullName = "global.adminui_requestserverinfo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestServerInfo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestugccontent",
			Parent = "global",
			FullName = "global.adminui_requestugccontent",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestUGCContent(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "adminui_requestugclist",
			Parent = "global",
			FullName = "global.adminui_requestugclist",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.AdminUI_RequestUGCList(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "allowadminui",
			Parent = "global",
			FullName = "global.allowadminui",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Description = "Controls whether the in-game admin UI is displayed to admins",
			Replicated = true,
			Variable = true,
			GetOveride = () => Admin.allowAdminUI.ToString(),
			SetOveride = delegate(string str)
			{
				Admin.allowAdminUI = str.ToBool();
			},
			Default = "True"
		},
		new ConsoleSystem.Command
		{
			Name = "ban",
			Parent = "global",
			FullName = "global.ban",
			ServerAdmin = true,
			Description = "ban <player> <reason> [optional duration]",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.ban(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "banid",
			Parent = "global",
			FullName = "global.banid",
			ServerAdmin = true,
			Description = "banid <steamid> <username> <reason> [optional duration]",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.banid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "banlist",
			Parent = "global",
			FullName = "global.banlist",
			ServerAdmin = true,
			Description = "List of banned users (sourceds compat)",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.banlist(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "banlistex",
			Parent = "global",
			FullName = "global.banlistex",
			ServerAdmin = true,
			Description = "List of banned users - shows reasons and usernames",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.banlistex(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bans",
			Parent = "global",
			FullName = "global.bans",
			ServerAdmin = true,
			Description = "List of banned users",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ServerUsers.User[] rval23 = Admin.Bans();
				arg.ReplyWithObject(rval23);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "buildinfo",
			Parent = "global",
			FullName = "global.buildinfo",
			ServerAdmin = true,
			Description = "Get information about this build",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				BuildInfo rval22 = Admin.BuildInfo();
				arg.ReplyWithObject(rval22);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "carstats",
			Parent = "global",
			FullName = "global.carstats",
			ServerAdmin = true,
			Description = "Get information about all the cars in the world",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.carstats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearugcentitiesinrange",
			Parent = "global",
			FullName = "global.clearugcentitiesinrange",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.clearugcentitiesinrange(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearugcentity",
			Parent = "global",
			FullName = "global.clearugcentity",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.clearugcentity(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clientperf",
			Parent = "global",
			FullName = "global.clientperf",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.clientperf(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clientperf_frametime",
			Parent = "global",
			FullName = "global.clientperf_frametime",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.clientperf_frametime(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "entid",
			Parent = "global",
			FullName = "global.entid",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.entid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "getugcinfo",
			Parent = "global",
			FullName = "global.getugcinfo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.getugcinfo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "injureplayer",
			Parent = "global",
			FullName = "global.injureplayer",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.injureplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "kick",
			Parent = "global",
			FullName = "global.kick",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.kick(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "kickall",
			Parent = "global",
			FullName = "global.kickall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.kickall(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "killplayer",
			Parent = "global",
			FullName = "global.killplayer",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.killplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "listid",
			Parent = "global",
			FullName = "global.listid",
			ServerAdmin = true,
			Description = "List of banned users, by ID (sourceds compat)",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.listid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "moderatorid",
			Parent = "global",
			FullName = "global.moderatorid",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.moderatorid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "mute",
			Parent = "global",
			FullName = "global.mute",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.mute(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "mutelist",
			Parent = "global",
			FullName = "global.mutelist",
			ServerAdmin = true,
			Description = "Print a list of currently muted players",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.mutelist(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ownerid",
			Parent = "global",
			FullName = "global.ownerid",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.ownerid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "playerlist",
			Parent = "global",
			FullName = "global.playerlist",
			ServerAdmin = true,
			Description = "Get a list of players",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.PlayerInfo[] rval21 = Admin.playerlist();
				arg.ReplyWithObject(rval21);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "players",
			Parent = "global",
			FullName = "global.players",
			ServerAdmin = true,
			Description = "Print out currently connected clients etc",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.players(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "recoverplayer",
			Parent = "global",
			FullName = "global.recoverplayer",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.recoverplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "removemoderator",
			Parent = "global",
			FullName = "global.removemoderator",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.removemoderator(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "removeowner",
			Parent = "global",
			FullName = "global.removeowner",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.removeowner(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "say",
			Parent = "global",
			FullName = "global.say",
			ServerAdmin = true,
			Description = "Sends a message in chat",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.say(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "serverinfo",
			Parent = "global",
			FullName = "global.serverinfo",
			ServerAdmin = true,
			Description = "Get a list of information about the server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.ServerInfoOutput serverInfoOutput = Admin.ServerInfo();
				arg.ReplyWithObject(serverInfoOutput);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "skipqueue",
			Parent = "global",
			FullName = "global.skipqueue",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.skipqueue(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleepingusers",
			Parent = "global",
			FullName = "global.sleepingusers",
			ServerAdmin = true,
			Description = "Show user info for players on server.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.sleepingusers(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleepingusersinrange",
			Parent = "global",
			FullName = "global.sleepingusersinrange",
			ServerAdmin = true,
			Description = "Show user info for sleeping players on server in range of the player.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.sleepingusersinrange(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stats",
			Parent = "global",
			FullName = "global.stats",
			ServerAdmin = true,
			Description = "Print out stats of currently connected clients",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.stats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "status",
			Parent = "global",
			FullName = "global.status",
			ServerAdmin = true,
			Description = "Print out currently connected clients",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.status(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teaminfo",
			Parent = "global",
			FullName = "global.teaminfo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval20 = Admin.teaminfo(arg);
				arg.ReplyWithObject(rval20);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "unban",
			Parent = "global",
			FullName = "global.unban",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.unban(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "unmute",
			Parent = "global",
			FullName = "global.unmute",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.unmute(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "users",
			Parent = "global",
			FullName = "global.users",
			ServerAdmin = true,
			Description = "Show user info for players on server.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.users(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "usersinrange",
			Parent = "global",
			FullName = "global.usersinrange",
			ServerAdmin = true,
			Description = "Show user info for players on server in range of the player.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.usersinrange(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "usersinrangeofplayer",
			Parent = "global",
			FullName = "global.usersinrangeofplayer",
			ServerAdmin = true,
			Description = "Show user info for players on server in range of the supplied player (eg. Jim 50)",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Admin.usersinrangeofplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "accuratevisiondistance",
			Parent = "ai",
			FullName = "ai.accuratevisiondistance",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.accuratevisiondistance.ToString(),
			SetOveride = delegate(string str)
			{
				AI.accuratevisiondistance = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "addignoreplayer",
			Parent = "ai",
			FullName = "ai.addignoreplayer",
			ServerAdmin = true,
			Description = "Add a player (or command user if no player is specified) to the AIs ignore list.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.addignoreplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "allowdesigning",
			Parent = "ai",
			FullName = "ai.allowdesigning",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Saved = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => AI.allowdesigning.ToString(),
			SetOveride = delegate(string str)
			{
				AI.allowdesigning = str.ToBool();
			},
			Default = "True"
		},
		new ConsoleSystem.Command
		{
			Name = "animal_ignore_food",
			Parent = "ai",
			FullName = "ai.animal_ignore_food",
			ServerAdmin = true,
			Description = "If animal_ignore_food is true, animals will not sense food sources or interact with them (server optimization). (default: true)",
			Variable = true,
			GetOveride = () => AI.animal_ignore_food.ToString(),
			SetOveride = delegate(string str)
			{
				AI.animal_ignore_food = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "brainstats",
			Parent = "ai",
			FullName = "ai.brainstats",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.brainstats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "frametime",
			Parent = "ai",
			FullName = "ai.frametime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.frametime.ToString(),
			SetOveride = delegate(string str)
			{
				AI.frametime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "groups",
			Parent = "ai",
			FullName = "ai.groups",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.groups.ToString(),
			SetOveride = delegate(string str)
			{
				AI.groups = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ignoreplayers",
			Parent = "ai",
			FullName = "ai.ignoreplayers",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.ignoreplayers.ToString(),
			SetOveride = delegate(string str)
			{
				AI.ignoreplayers = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "killanimals",
			Parent = "ai",
			FullName = "ai.killanimals",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.killanimals(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "killscientists",
			Parent = "ai",
			FullName = "ai.killscientists",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.killscientists(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "move",
			Parent = "ai",
			FullName = "ai.move",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.move.ToString(),
			SetOveride = delegate(string str)
			{
				AI.move = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_carve_height",
			Parent = "ai",
			FullName = "ai.nav_carve_height",
			ServerAdmin = true,
			Description = "The height of the carve volume. (default: 2)",
			Variable = true,
			GetOveride = () => AI.nav_carve_height.ToString(),
			SetOveride = delegate(string str)
			{
				AI.nav_carve_height = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_carve_min_base_size",
			Parent = "ai",
			FullName = "ai.nav_carve_min_base_size",
			ServerAdmin = true,
			Description = "The minimum size we allow a carving volume to be. (default: 2)",
			Variable = true,
			GetOveride = () => AI.nav_carve_min_base_size.ToString(),
			SetOveride = delegate(string str)
			{
				AI.nav_carve_min_base_size = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_carve_min_building_blocks_to_apply_optimization",
			Parent = "ai",
			FullName = "ai.nav_carve_min_building_blocks_to_apply_optimization",
			ServerAdmin = true,
			Description = "The minimum number of building blocks a building needs to consist of for this optimization to be applied. (default: 25)",
			Variable = true,
			GetOveride = () => AI.nav_carve_min_building_blocks_to_apply_optimization.ToString(),
			SetOveride = delegate(string str)
			{
				AI.nav_carve_min_building_blocks_to_apply_optimization = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_carve_size_multiplier",
			Parent = "ai",
			FullName = "ai.nav_carve_size_multiplier",
			ServerAdmin = true,
			Description = "The size multiplier applied to the size of the carve volume. The smaller the value, the tighter the skirt around foundation edges, but too small and animals can attack through walls. (default: 4)",
			Variable = true,
			GetOveride = () => AI.nav_carve_size_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.nav_carve_size_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_carve_use_building_optimization",
			Parent = "ai",
			FullName = "ai.nav_carve_use_building_optimization",
			ServerAdmin = true,
			Description = "If nav_carve_use_building_optimization is true, we attempt to reduce the amount of navmesh carves for a building. (default: false)",
			Variable = true,
			GetOveride = () => AI.nav_carve_use_building_optimization.ToString(),
			SetOveride = delegate(string str)
			{
				AI.nav_carve_use_building_optimization = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "navthink",
			Parent = "ai",
			FullName = "ai.navthink",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.navthink.ToString(),
			SetOveride = delegate(string str)
			{
				AI.navthink = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_alertness_drain_rate",
			Parent = "ai",
			FullName = "ai.npc_alertness_drain_rate",
			ServerAdmin = true,
			Description = "npc_alertness_drain_rate define the rate at which we drain the alertness level of an NPC when there are no enemies in sight. (Default: 0.01)",
			Variable = true,
			GetOveride = () => AI.npc_alertness_drain_rate.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_alertness_drain_rate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_alertness_to_aim_modifier",
			Parent = "ai",
			FullName = "ai.npc_alertness_to_aim_modifier",
			ServerAdmin = true,
			Description = "This is multiplied with the current alertness (0-10) to decide how long it will take for the NPC to deliberately miss again. (default: 0.33)",
			Variable = true,
			GetOveride = () => AI.npc_alertness_to_aim_modifier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_alertness_to_aim_modifier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_alertness_zero_detection_mod",
			Parent = "ai",
			FullName = "ai.npc_alertness_zero_detection_mod",
			ServerAdmin = true,
			Description = "npc_alertness_zero_detection_mod define the threshold of visibility required to detect an enemy when alertness is zero. (Default: 0.5)",
			Variable = true,
			GetOveride = () => AI.npc_alertness_zero_detection_mod.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_alertness_zero_detection_mod = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_cover_compromised_cooldown",
			Parent = "ai",
			FullName = "ai.npc_cover_compromised_cooldown",
			ServerAdmin = true,
			Description = "npc_cover_compromised_cooldown defines how long a cover point is marked as compromised before it's cleared again for selection. (default: 10)",
			Variable = true,
			GetOveride = () => AI.npc_cover_compromised_cooldown.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_cover_compromised_cooldown = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_cover_info_tick_rate_multiplier",
			Parent = "ai",
			FullName = "ai.npc_cover_info_tick_rate_multiplier",
			ServerAdmin = true,
			Description = "The rate at which we gather information about available cover points. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 20)",
			Variable = true,
			GetOveride = () => AI.npc_cover_info_tick_rate_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_cover_info_tick_rate_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_cover_path_vs_straight_dist_max_diff",
			Parent = "ai",
			FullName = "ai.npc_cover_path_vs_straight_dist_max_diff",
			ServerAdmin = true,
			Description = "npc_cover_path_vs_straight_dist_max_diff defines what the maximum difference between straight-line distance and path distance can be when evaluating cover points. (default: 2)",
			Variable = true,
			GetOveride = () => AI.npc_cover_path_vs_straight_dist_max_diff.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_cover_path_vs_straight_dist_max_diff = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_cover_use_path_distance",
			Parent = "ai",
			FullName = "ai.npc_cover_use_path_distance",
			ServerAdmin = true,
			Description = "If npc_cover_use_path_distance is set to true then npcs will look at the distance between the cover point and their target using the path between the two, rather than the straight-line distance.",
			Variable = true,
			GetOveride = () => AI.npc_cover_use_path_distance.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_cover_use_path_distance = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_deliberate_hit_randomizer",
			Parent = "ai",
			FullName = "ai.npc_deliberate_hit_randomizer",
			ServerAdmin = true,
			Description = "The percentage away from a maximum miss the randomizer is allowed to travel when shooting to deliberately hit the target (we don't want perfect hits with every shot). (default: 0.85f)",
			Variable = true,
			GetOveride = () => AI.npc_deliberate_hit_randomizer.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_deliberate_hit_randomizer = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_deliberate_miss_offset_multiplier",
			Parent = "ai",
			FullName = "ai.npc_deliberate_miss_offset_multiplier",
			ServerAdmin = true,
			Description = "The offset with which the NPC will maximum miss the target. (default: 1.25)",
			Variable = true,
			GetOveride = () => AI.npc_deliberate_miss_offset_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_deliberate_miss_offset_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_deliberate_miss_to_hit_alignment_time",
			Parent = "ai",
			FullName = "ai.npc_deliberate_miss_to_hit_alignment_time",
			ServerAdmin = true,
			Description = "The time it takes for the NPC to deliberately miss to the time the NPC tries to hit its target. (default: 1.5)",
			Variable = true,
			GetOveride = () => AI.npc_deliberate_miss_to_hit_alignment_time.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_deliberate_miss_to_hit_alignment_time = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_door_trigger_size",
			Parent = "ai",
			FullName = "ai.npc_door_trigger_size",
			ServerAdmin = true,
			Description = "npc_door_trigger_size defines the size of the trigger box on doors that opens the door as npcs walk close to it (default: 1.5)",
			Variable = true,
			GetOveride = () => AI.npc_door_trigger_size.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_door_trigger_size = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_enable",
			Parent = "ai",
			FullName = "ai.npc_enable",
			ServerAdmin = true,
			Description = "If npc_enable is set to false then npcs won't spawn. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_enable.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_enable = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_families_no_hurt",
			Parent = "ai",
			FullName = "ai.npc_families_no_hurt",
			ServerAdmin = true,
			Description = "If npc_families_no_hurt is true, npcs of the same family won't be able to hurt each other. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_families_no_hurt.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_families_no_hurt = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_gun_noise_silencer_modifier",
			Parent = "ai",
			FullName = "ai.npc_gun_noise_silencer_modifier",
			ServerAdmin = true,
			Description = "The modifier by which a silencer reduce the noise that a gun makes when shot. (Default: 0.15)",
			Variable = true,
			GetOveride = () => AI.npc_gun_noise_silencer_modifier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_gun_noise_silencer_modifier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_htn_player_base_damage_modifier",
			Parent = "ai",
			FullName = "ai.npc_htn_player_base_damage_modifier",
			ServerAdmin = true,
			Description = "Baseline damage modifier for the new HTN Player NPCs to nerf their damage compared to the old NPCs. (default: 1.15f)",
			Variable = true,
			GetOveride = () => AI.npc_htn_player_base_damage_modifier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_htn_player_base_damage_modifier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_htn_player_frustration_threshold",
			Parent = "ai",
			FullName = "ai.npc_htn_player_frustration_threshold",
			ServerAdmin = true,
			Description = "npc_htn_player_frustration_threshold defines where the frustration threshold for NPCs go, where they have the opportunity to change to a more aggressive tactic. (default: 3)",
			Variable = true,
			GetOveride = () => AI.npc_htn_player_frustration_threshold.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_htn_player_frustration_threshold = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_ignore_chairs",
			Parent = "ai",
			FullName = "ai.npc_ignore_chairs",
			ServerAdmin = true,
			Description = "If npc_ignore_chairs is true, npcs won't care about seeking out and sitting in chairs. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_ignore_chairs.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_ignore_chairs = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_junkpile_a_spawn_chance",
			Parent = "ai",
			FullName = "ai.npc_junkpile_a_spawn_chance",
			ServerAdmin = true,
			Description = "npc_junkpile_a_spawn_chance define the chance for scientists to spawn at junkpile a. (Default: 0.1)",
			Variable = true,
			GetOveride = () => AI.npc_junkpile_a_spawn_chance.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_junkpile_a_spawn_chance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_junkpile_dist_aggro_gate",
			Parent = "ai",
			FullName = "ai.npc_junkpile_dist_aggro_gate",
			ServerAdmin = true,
			Description = "npc_junkpile_dist_aggro_gate define at what range (or closer) a junkpile scientist will get aggressive. (Default: 8)",
			Variable = true,
			GetOveride = () => AI.npc_junkpile_dist_aggro_gate.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_junkpile_dist_aggro_gate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_junkpile_g_spawn_chance",
			Parent = "ai",
			FullName = "ai.npc_junkpile_g_spawn_chance",
			ServerAdmin = true,
			Description = "npc_junkpile_g_spawn_chance define the chance for scientists to spawn at junkpile g. (Default: 0.1)",
			Variable = true,
			GetOveride = () => AI.npc_junkpile_g_spawn_chance.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_junkpile_g_spawn_chance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_junkpilespawn_chance",
			Parent = "ai",
			FullName = "ai.npc_junkpilespawn_chance",
			ServerAdmin = true,
			Description = "defines the chance for scientists to spawn at NPC junkpiles. (Default: 0.1)",
			Variable = true,
			GetOveride = () => AI.npc_junkpilespawn_chance.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_junkpilespawn_chance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_max_junkpile_count",
			Parent = "ai",
			FullName = "ai.npc_max_junkpile_count",
			ServerAdmin = true,
			Description = "npc_max_junkpile_count define how many npcs can spawn into the world at junkpiles at the same time (does not include monuments) (Default: 30)",
			Variable = true,
			GetOveride = () => AI.npc_max_junkpile_count.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_max_junkpile_count = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_max_population_military_tunnels",
			Parent = "ai",
			FullName = "ai.npc_max_population_military_tunnels",
			ServerAdmin = true,
			Description = "npc_max_population_military_tunnels defines the size of the npc population at military tunnels. (default: 3)",
			Variable = true,
			GetOveride = () => AI.npc_max_population_military_tunnels.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_max_population_military_tunnels = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_max_roam_multiplier",
			Parent = "ai",
			FullName = "ai.npc_max_roam_multiplier",
			ServerAdmin = true,
			Description = "This is multiplied with the max roam range stat of an NPC to determine how far from its spawn point the NPC is allowed to roam. (default: 3)",
			Variable = true,
			GetOveride = () => AI.npc_max_roam_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_max_roam_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_only_hurt_active_target_in_safezone",
			Parent = "ai",
			FullName = "ai.npc_only_hurt_active_target_in_safezone",
			ServerAdmin = true,
			Description = "If npc_only_hurt_active_target_in_safezone is true, npcs won't any player other than their actively targeted player when in a safe zone. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_only_hurt_active_target_in_safezone.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_only_hurt_active_target_in_safezone = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_patrol_point_cooldown",
			Parent = "ai",
			FullName = "ai.npc_patrol_point_cooldown",
			ServerAdmin = true,
			Description = "npc_patrol_point_cooldown defines the cooldown time on a patrol point until it's available again (default: 5)",
			Variable = true,
			GetOveride = () => AI.npc_patrol_point_cooldown.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_patrol_point_cooldown = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_reasoning_system_tick_rate_multiplier",
			Parent = "ai",
			FullName = "ai.npc_reasoning_system_tick_rate_multiplier",
			ServerAdmin = true,
			Description = "The rate at which we tick the reasoning system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 1)",
			Variable = true,
			GetOveride = () => AI.npc_reasoning_system_tick_rate_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_reasoning_system_tick_rate_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_respawn_delay_max_military_tunnels",
			Parent = "ai",
			FullName = "ai.npc_respawn_delay_max_military_tunnels",
			ServerAdmin = true,
			Description = "npc_respawn_delay_max_military_tunnels defines the maximum delay between spawn ticks at military tunnels. (default: 1920)",
			Variable = true,
			GetOveride = () => AI.npc_respawn_delay_max_military_tunnels.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_respawn_delay_max_military_tunnels = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_respawn_delay_min_military_tunnels",
			Parent = "ai",
			FullName = "ai.npc_respawn_delay_min_military_tunnels",
			ServerAdmin = true,
			Description = "npc_respawn_delay_min_military_tunnels defines the minimum delay between spawn ticks at military tunnels. (default: 480)",
			Variable = true,
			GetOveride = () => AI.npc_respawn_delay_min_military_tunnels.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_respawn_delay_min_military_tunnels = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_sensory_system_tick_rate_multiplier",
			Parent = "ai",
			FullName = "ai.npc_sensory_system_tick_rate_multiplier",
			ServerAdmin = true,
			Description = "The rate at which we tick the sensory system. Minimum value is 1, as it multiplies with the tick-rate of the fixed AI tick rate of 0.1 (Default: 5)",
			Variable = true,
			GetOveride = () => AI.npc_sensory_system_tick_rate_multiplier.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_sensory_system_tick_rate_multiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_spawn_on_cargo_ship",
			Parent = "ai",
			FullName = "ai.npc_spawn_on_cargo_ship",
			ServerAdmin = true,
			Description = "Spawn NPCs on the Cargo Ship. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_spawn_on_cargo_ship.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_spawn_on_cargo_ship = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_spawn_per_tick_max_military_tunnels",
			Parent = "ai",
			FullName = "ai.npc_spawn_per_tick_max_military_tunnels",
			ServerAdmin = true,
			Description = "npc_spawn_per_tick_max_military_tunnels defines how many can maximum spawn at once at military tunnels. (default: 1)",
			Variable = true,
			GetOveride = () => AI.npc_spawn_per_tick_max_military_tunnels.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_spawn_per_tick_max_military_tunnels = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_spawn_per_tick_min_military_tunnels",
			Parent = "ai",
			FullName = "ai.npc_spawn_per_tick_min_military_tunnels",
			ServerAdmin = true,
			Description = "npc_spawn_per_tick_min_military_tunnels defineshow many will minimum spawn at once at military tunnels. (default: 1)",
			Variable = true,
			GetOveride = () => AI.npc_spawn_per_tick_min_military_tunnels.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_spawn_per_tick_min_military_tunnels = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_speed_crouch_run",
			Parent = "ai",
			FullName = "ai.npc_speed_crouch_run",
			ServerAdmin = true,
			Description = "npc_speed_crouch_run define the speed of an npc when in the crouched run state, and should be a number between 0 and 1. (Default: 0.25)",
			Variable = true,
			GetOveride = () => AI.npc_speed_crouch_run.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_speed_crouch_run = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_speed_crouch_walk",
			Parent = "ai",
			FullName = "ai.npc_speed_crouch_walk",
			ServerAdmin = true,
			Description = "npc_speed_walk define the speed of an npc when in the crouched walk state, and should be a number between 0 and 1. (Default: 0.1)",
			Variable = true,
			GetOveride = () => AI.npc_speed_crouch_walk.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_speed_crouch_walk = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_speed_run",
			Parent = "ai",
			FullName = "ai.npc_speed_run",
			ServerAdmin = true,
			Description = "npc_speed_walk define the speed of an npc when in the run state, and should be a number between 0 and 1. (Default: 0.4)",
			Variable = true,
			GetOveride = () => AI.npc_speed_run.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_speed_run = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_speed_sprint",
			Parent = "ai",
			FullName = "ai.npc_speed_sprint",
			ServerAdmin = true,
			Description = "npc_speed_walk define the speed of an npc when in the sprint state, and should be a number between 0 and 1. (Default: 1.0)",
			Variable = true,
			GetOveride = () => AI.npc_speed_sprint.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_speed_sprint = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_speed_walk",
			Parent = "ai",
			FullName = "ai.npc_speed_walk",
			ServerAdmin = true,
			Description = "npc_speed_walk define the speed of an npc when in the walk state, and should be a number between 0 and 1. (Default: 0.18)",
			Variable = true,
			GetOveride = () => AI.npc_speed_walk.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_speed_walk = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_use_new_aim_system",
			Parent = "ai",
			FullName = "ai.npc_use_new_aim_system",
			ServerAdmin = true,
			Description = "If npc_use_new_aim_system is true, npcs will miss on purpose on occasion, where the old system would randomize aim cone. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_use_new_aim_system.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_use_new_aim_system = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_use_thrown_weapons",
			Parent = "ai",
			FullName = "ai.npc_use_thrown_weapons",
			ServerAdmin = true,
			Description = "If npc_use_thrown_weapons is true, npcs will throw grenades, etc. This is an experimental feature. (default: true)",
			Variable = true,
			GetOveride = () => AI.npc_use_thrown_weapons.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_use_thrown_weapons = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_valid_aim_cone",
			Parent = "ai",
			FullName = "ai.npc_valid_aim_cone",
			ServerAdmin = true,
			Description = "npc_valid_aim_cone defines how close their aim needs to be on target in order to fire. (default: 0.8)",
			Variable = true,
			GetOveride = () => AI.npc_valid_aim_cone.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_valid_aim_cone = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npc_valid_mounted_aim_cone",
			Parent = "ai",
			FullName = "ai.npc_valid_mounted_aim_cone",
			ServerAdmin = true,
			Description = "npc_valid_mounted_aim_cone defines how close their aim needs to be on target in order to fire while mounted. (default: 0.92)",
			Variable = true,
			GetOveride = () => AI.npc_valid_mounted_aim_cone.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npc_valid_mounted_aim_cone = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "npcswimming",
			Parent = "ai",
			FullName = "ai.npcswimming",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.npcswimming.ToString(),
			SetOveride = delegate(string str)
			{
				AI.npcswimming = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ocean_patrol_path_iterations",
			Parent = "ai",
			FullName = "ai.ocean_patrol_path_iterations",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.ocean_patrol_path_iterations.ToString(),
			SetOveride = delegate(string str)
			{
				AI.ocean_patrol_path_iterations = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "removeignoreplayer",
			Parent = "ai",
			FullName = "ai.removeignoreplayer",
			ServerAdmin = true,
			Description = "Remove a player (or command user if no player is specified) from the AIs ignore list.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.removeignoreplayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "selectnpclookatserver",
			Parent = "ai",
			FullName = "ai.selectnpclookatserver",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.selectNPCLookatServer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sensetime",
			Parent = "ai",
			FullName = "ai.sensetime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.sensetime.ToString(),
			SetOveride = delegate(string str)
			{
				AI.sensetime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "setdestinationsamplenavmesh",
			Parent = "ai",
			FullName = "ai.setdestinationsamplenavmesh",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.setdestinationsamplenavmesh.ToString(),
			SetOveride = delegate(string str)
			{
				AI.setdestinationsamplenavmesh = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleepwake",
			Parent = "ai",
			FullName = "ai.sleepwake",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.sleepwake.ToString(),
			SetOveride = delegate(string str)
			{
				AI.sleepwake = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleepwakestats",
			Parent = "ai",
			FullName = "ai.sleepwakestats",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.sleepwakestats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spliceupdates",
			Parent = "ai",
			FullName = "ai.spliceupdates",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.spliceupdates.ToString(),
			SetOveride = delegate(string str)
			{
				AI.spliceupdates = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "think",
			Parent = "ai",
			FullName = "ai.think",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.think.ToString(),
			SetOveride = delegate(string str)
			{
				AI.think = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickrate",
			Parent = "ai",
			FullName = "ai.tickrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.tickrate.ToString(),
			SetOveride = delegate(string str)
			{
				AI.tickrate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "usecalculatepath",
			Parent = "ai",
			FullName = "ai.usecalculatepath",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.usecalculatepath.ToString(),
			SetOveride = delegate(string str)
			{
				AI.usecalculatepath = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "usegrid",
			Parent = "ai",
			FullName = "ai.usegrid",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.usegrid.ToString(),
			SetOveride = delegate(string str)
			{
				AI.usegrid = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "usesetdestinationfallback",
			Parent = "ai",
			FullName = "ai.usesetdestinationfallback",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => AI.usesetdestinationfallback.ToString(),
			SetOveride = delegate(string str)
			{
				AI.usesetdestinationfallback = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wakesleepingai",
			Parent = "ai",
			FullName = "ai.wakesleepingai",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				AI.wakesleepingai(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "admincheat",
			Parent = "antihack",
			FullName = "antihack.admincheat",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.admincheat.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.admincheat = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "build_losradius",
			Parent = "antihack",
			FullName = "antihack.build_losradius",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.build_losradius.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.build_losradius = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "build_terraincheck",
			Parent = "antihack",
			FullName = "antihack.build_terraincheck",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.build_terraincheck.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.build_terraincheck = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debuglevel",
			Parent = "antihack",
			FullName = "antihack.debuglevel",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.debuglevel.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.debuglevel = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enforcementlevel",
			Parent = "antihack",
			FullName = "antihack.enforcementlevel",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.enforcementlevel.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.enforcementlevel = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_clientframes",
			Parent = "antihack",
			FullName = "antihack.eye_clientframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_clientframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_clientframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_forgiveness",
			Parent = "antihack",
			FullName = "antihack.eye_forgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_forgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_forgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_history_forgiveness",
			Parent = "antihack",
			FullName = "antihack.eye_history_forgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_history_forgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_history_forgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_history_penalty",
			Parent = "antihack",
			FullName = "antihack.eye_history_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_history_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_history_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_losradius",
			Parent = "antihack",
			FullName = "antihack.eye_losradius",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_losradius.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_losradius = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_noclip_backtracking",
			Parent = "antihack",
			FullName = "antihack.eye_noclip_backtracking",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_noclip_backtracking.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_noclip_backtracking = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_noclip_cutoff",
			Parent = "antihack",
			FullName = "antihack.eye_noclip_cutoff",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_noclip_cutoff.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_noclip_cutoff = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_noclip_margin",
			Parent = "antihack",
			FullName = "antihack.eye_noclip_margin",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_noclip_margin.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_noclip_margin = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_penalty",
			Parent = "antihack",
			FullName = "antihack.eye_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_protection",
			Parent = "antihack",
			FullName = "antihack.eye_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_serverframes",
			Parent = "antihack",
			FullName = "antihack.eye_serverframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_serverframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_serverframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eye_terraincheck",
			Parent = "antihack",
			FullName = "antihack.eye_terraincheck",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.eye_terraincheck.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.eye_terraincheck = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_extrusion",
			Parent = "antihack",
			FullName = "antihack.flyhack_extrusion",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_extrusion.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_extrusion = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_forgiveness_horizontal",
			Parent = "antihack",
			FullName = "antihack.flyhack_forgiveness_horizontal",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_forgiveness_horizontal.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_forgiveness_horizontal = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_forgiveness_horizontal_inertia",
			Parent = "antihack",
			FullName = "antihack.flyhack_forgiveness_horizontal_inertia",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_forgiveness_vertical",
			Parent = "antihack",
			FullName = "antihack.flyhack_forgiveness_vertical",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_forgiveness_vertical.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_forgiveness_vertical = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_forgiveness_vertical_inertia",
			Parent = "antihack",
			FullName = "antihack.flyhack_forgiveness_vertical_inertia",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_forgiveness_vertical_inertia.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_forgiveness_vertical_inertia = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_margin",
			Parent = "antihack",
			FullName = "antihack.flyhack_margin",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_margin.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_margin = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_maxsteps",
			Parent = "antihack",
			FullName = "antihack.flyhack_maxsteps",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_maxsteps.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_maxsteps = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_penalty",
			Parent = "antihack",
			FullName = "antihack.flyhack_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_protection",
			Parent = "antihack",
			FullName = "antihack.flyhack_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_reject",
			Parent = "antihack",
			FullName = "antihack.flyhack_reject",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_reject.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_reject = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flyhack_stepsize",
			Parent = "antihack",
			FullName = "antihack.flyhack_stepsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.flyhack_stepsize.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.flyhack_stepsize = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forceposition",
			Parent = "antihack",
			FullName = "antihack.forceposition",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.forceposition.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.forceposition = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxdeltatime",
			Parent = "antihack",
			FullName = "antihack.maxdeltatime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.maxdeltatime.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.maxdeltatime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxdesync",
			Parent = "antihack",
			FullName = "antihack.maxdesync",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.maxdesync.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.maxdesync = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxviolation",
			Parent = "antihack",
			FullName = "antihack.maxviolation",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.maxviolation.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.maxviolation = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_clientframes",
			Parent = "antihack",
			FullName = "antihack.melee_clientframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_clientframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_clientframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_forgiveness",
			Parent = "antihack",
			FullName = "antihack.melee_forgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_forgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_forgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_losforgiveness",
			Parent = "antihack",
			FullName = "antihack.melee_losforgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_losforgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_losforgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_penalty",
			Parent = "antihack",
			FullName = "antihack.melee_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_protection",
			Parent = "antihack",
			FullName = "antihack.melee_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_serverframes",
			Parent = "antihack",
			FullName = "antihack.melee_serverframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_serverframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_serverframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "melee_terraincheck",
			Parent = "antihack",
			FullName = "antihack.melee_terraincheck",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.melee_terraincheck.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.melee_terraincheck = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "modelstate",
			Parent = "antihack",
			FullName = "antihack.modelstate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.modelstate.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.modelstate = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_backtracking",
			Parent = "antihack",
			FullName = "antihack.noclip_backtracking",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_backtracking.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_backtracking = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_margin",
			Parent = "antihack",
			FullName = "antihack.noclip_margin",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_margin.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_margin = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_maxsteps",
			Parent = "antihack",
			FullName = "antihack.noclip_maxsteps",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_maxsteps.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_maxsteps = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_penalty",
			Parent = "antihack",
			FullName = "antihack.noclip_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_protection",
			Parent = "antihack",
			FullName = "antihack.noclip_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_reject",
			Parent = "antihack",
			FullName = "antihack.noclip_reject",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_reject.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_reject = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "noclip_stepsize",
			Parent = "antihack",
			FullName = "antihack.noclip_stepsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.noclip_stepsize.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.noclip_stepsize = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "objectplacement",
			Parent = "antihack",
			FullName = "antihack.objectplacement",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.objectplacement.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.objectplacement = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_anglechange",
			Parent = "antihack",
			FullName = "antihack.projectile_anglechange",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_anglechange.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_anglechange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_backtracking",
			Parent = "antihack",
			FullName = "antihack.projectile_backtracking",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_backtracking.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_backtracking = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_clientframes",
			Parent = "antihack",
			FullName = "antihack.projectile_clientframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_clientframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_clientframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_desync",
			Parent = "antihack",
			FullName = "antihack.projectile_desync",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_desync.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_desync = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_forgiveness",
			Parent = "antihack",
			FullName = "antihack.projectile_forgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_forgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_forgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_losforgiveness",
			Parent = "antihack",
			FullName = "antihack.projectile_losforgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_losforgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_losforgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_penalty",
			Parent = "antihack",
			FullName = "antihack.projectile_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_protection",
			Parent = "antihack",
			FullName = "antihack.projectile_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_serverframes",
			Parent = "antihack",
			FullName = "antihack.projectile_serverframes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_serverframes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_serverframes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_terraincheck",
			Parent = "antihack",
			FullName = "antihack.projectile_terraincheck",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_terraincheck.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_terraincheck = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_trajectory",
			Parent = "antihack",
			FullName = "antihack.projectile_trajectory",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_trajectory.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_trajectory = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "projectile_velocitychange",
			Parent = "antihack",
			FullName = "antihack.projectile_velocitychange",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.projectile_velocitychange.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.projectile_velocitychange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "relaxationpause",
			Parent = "antihack",
			FullName = "antihack.relaxationpause",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.relaxationpause.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.relaxationpause = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "relaxationrate",
			Parent = "antihack",
			FullName = "antihack.relaxationrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.relaxationrate.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.relaxationrate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reporting",
			Parent = "antihack",
			FullName = "antihack.reporting",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.reporting.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.reporting = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_forgiveness",
			Parent = "antihack",
			FullName = "antihack.speedhack_forgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_forgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_forgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_forgiveness_inertia",
			Parent = "antihack",
			FullName = "antihack.speedhack_forgiveness_inertia",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_forgiveness_inertia.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_forgiveness_inertia = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_penalty",
			Parent = "antihack",
			FullName = "antihack.speedhack_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_protection",
			Parent = "antihack",
			FullName = "antihack.speedhack_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_reject",
			Parent = "antihack",
			FullName = "antihack.speedhack_reject",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_reject.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_reject = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "speedhack_slopespeed",
			Parent = "antihack",
			FullName = "antihack.speedhack_slopespeed",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.speedhack_slopespeed.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.speedhack_slopespeed = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "terrain_kill",
			Parent = "antihack",
			FullName = "antihack.terrain_kill",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.terrain_kill.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.terrain_kill = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "terrain_padding",
			Parent = "antihack",
			FullName = "antihack.terrain_padding",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.terrain_padding.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.terrain_padding = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "terrain_penalty",
			Parent = "antihack",
			FullName = "antihack.terrain_penalty",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.terrain_penalty.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.terrain_penalty = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "terrain_protection",
			Parent = "antihack",
			FullName = "antihack.terrain_protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.terrain_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.terrain_protection = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "terrain_timeslice",
			Parent = "antihack",
			FullName = "antihack.terrain_timeslice",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.terrain_timeslice.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.terrain_timeslice = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickhistoryforgiveness",
			Parent = "antihack",
			FullName = "antihack.tickhistoryforgiveness",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.tickhistoryforgiveness.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.tickhistoryforgiveness = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickhistorytime",
			Parent = "antihack",
			FullName = "antihack.tickhistorytime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.tickhistorytime.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.tickhistorytime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "userlevel",
			Parent = "antihack",
			FullName = "antihack.userlevel",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.AntiHack.userlevel.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.AntiHack.userlevel = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "alarmcooldown",
			Parent = "app",
			FullName = "app.alarmcooldown",
			ServerAdmin = true,
			Description = "Cooldown time before alarms can send another notification (in seconds)",
			Variable = true,
			GetOveride = () => App.alarmcooldown.ToString(),
			SetOveride = delegate(string str)
			{
				App.alarmcooldown = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "appban",
			Parent = "app",
			FullName = "app.appban",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.appban(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "appunban",
			Parent = "app",
			FullName = "app.appunban",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.appunban(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "connections",
			Parent = "app",
			FullName = "app.connections",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.connections(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "info",
			Parent = "app",
			FullName = "app.info",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.info(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "listenip",
			Parent = "app",
			FullName = "app.listenip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => App.listenip.ToString(),
			SetOveride = delegate(string str)
			{
				App.listenip = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxconnections",
			Parent = "app",
			FullName = "app.maxconnections",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => App.maxconnections.ToString(),
			SetOveride = delegate(string str)
			{
				App.maxconnections = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxconnectionsperip",
			Parent = "app",
			FullName = "app.maxconnectionsperip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => App.maxconnectionsperip.ToString(),
			SetOveride = delegate(string str)
			{
				App.maxconnectionsperip = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "notifications",
			Parent = "app",
			FullName = "app.notifications",
			ServerAdmin = true,
			Description = "Enables sending push notifications",
			Variable = true,
			GetOveride = () => App.notifications.ToString(),
			SetOveride = delegate(string str)
			{
				App.notifications = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "pair",
			Parent = "app",
			FullName = "app.pair",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.pair(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "port",
			Parent = "app",
			FullName = "app.port",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => App.port.ToString(),
			SetOveride = delegate(string str)
			{
				App.port = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "publicip",
			Parent = "app",
			FullName = "app.publicip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => App.publicip.ToString(),
			SetOveride = delegate(string str)
			{
				App.publicip = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "queuelimit",
			Parent = "app",
			FullName = "app.queuelimit",
			ServerAdmin = true,
			Description = "Max number of queued messages - set to 0 to disable message processing",
			Variable = true,
			GetOveride = () => App.queuelimit.ToString(),
			SetOveride = delegate(string str)
			{
				App.queuelimit = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "resetlimiter",
			Parent = "app",
			FullName = "app.resetlimiter",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				App.resetlimiter(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "serverid",
			Parent = "app",
			FullName = "app.serverid",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => App.serverid.ToString(),
			SetOveride = delegate(string str)
			{
				App.serverid = str;
			},
			Default = ""
		},
		new ConsoleSystem.Command
		{
			Name = "update",
			Parent = "app",
			FullName = "app.update",
			ServerAdmin = true,
			Description = "Disables updating entirely - emergency use only",
			Variable = true,
			GetOveride = () => App.update.ToString(),
			SetOveride = delegate(string str)
			{
				App.update = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "verbose",
			Parent = "batching",
			FullName = "batching.verbose",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Batching.verbose.ToString(),
			SetOveride = delegate(string str)
			{
				Batching.verbose = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "bradley",
			FullName = "bradley.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Bradley.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				Bradley.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "quickrespawn",
			Parent = "bradley",
			FullName = "bradley.quickrespawn",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Bradley.quickrespawn(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawndelayminutes",
			Parent = "bradley",
			FullName = "bradley.respawndelayminutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Bradley.respawnDelayMinutes.ToString(),
			SetOveride = delegate(string str)
			{
				Bradley.respawnDelayMinutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawndelayvariance",
			Parent = "bradley",
			FullName = "bradley.respawndelayvariance",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Bradley.respawnDelayVariance.ToString(),
			SetOveride = delegate(string str)
			{
				Bradley.respawnDelayVariance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cardgamesay",
			Parent = "chat",
			FullName = "chat.cardgamesay",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Chat.cardgamesay(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "chat",
			FullName = "chat.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Chat.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				Chat.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "historysize",
			Parent = "chat",
			FullName = "chat.historysize",
			ServerAdmin = true,
			Description = "Number of messages to keep in memory for chat history",
			Variable = true,
			GetOveride = () => Chat.historysize.ToString(),
			SetOveride = delegate(string str)
			{
				Chat.historysize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "say",
			Parent = "chat",
			FullName = "chat.say",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Chat.say(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "search",
			Parent = "chat",
			FullName = "chat.search",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				IEnumerable<Chat.ChatEntry> rval19 = Chat.search(arg);
				arg.ReplyWithObject(rval19);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "serverlog",
			Parent = "chat",
			FullName = "chat.serverlog",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Chat.serverlog.ToString(),
			SetOveride = delegate(string str)
			{
				Chat.serverlog = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tail",
			Parent = "chat",
			FullName = "chat.tail",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				IEnumerable<Chat.ChatEntry> rval18 = Chat.tail(arg);
				arg.ReplyWithObject(rval18);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teamsay",
			Parent = "chat",
			FullName = "chat.teamsay",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Chat.teamsay(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "search",
			Parent = "console",
			FullName = "console.search",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				IEnumerable<Output.Entry> rval17 = Console.search(arg);
				arg.ReplyWithObject(rval17);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tail",
			Parent = "console",
			FullName = "console.tail",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				IEnumerable<Output.Entry> rval16 = Console.tail(arg);
				arg.ReplyWithObject(rval16);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "frameminutes",
			Parent = "construct",
			FullName = "construct.frameminutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Construct.frameminutes.ToString(),
			SetOveride = delegate(string str)
			{
				Construct.frameminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "add",
			Parent = "craft",
			FullName = "craft.add",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Craft.add(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cancel",
			Parent = "craft",
			FullName = "craft.cancel",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Craft.cancel(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "canceltask",
			Parent = "craft",
			FullName = "craft.canceltask",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Craft.canceltask(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fasttracktask",
			Parent = "craft",
			FullName = "craft.fasttracktask",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Craft.fasttracktask(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "instant",
			Parent = "craft",
			FullName = "craft.instant",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Craft.instant.ToString(),
			SetOveride = delegate(string str)
			{
				Craft.instant = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "export",
			Parent = "data",
			FullName = "data.export",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Data.export(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "breakheld",
			Parent = "debug",
			FullName = "debug.breakheld",
			ServerAdmin = true,
			Description = "Break the current held object",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.breakheld(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "breakitem",
			Parent = "debug",
			FullName = "debug.breakitem",
			ServerAdmin = true,
			Description = "Break all the items in your inventory whose name match the passed string",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.breakitem(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "callbacks",
			Parent = "debug",
			FullName = "debug.callbacks",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Debugging.callbacks.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.callbacks = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "checkparentingtriggers",
			Parent = "debug",
			FullName = "debug.checkparentingtriggers",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Debugging.checkparentingtriggers.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.checkparentingtriggers = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "checktriggers",
			Parent = "debug",
			FullName = "debug.checktriggers",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Debugging.checktriggers.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.checktriggers = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debugdismounts",
			Parent = "debug",
			FullName = "debug.debugdismounts",
			ServerAdmin = true,
			Description = "Shows some debug info for dismount attempts.",
			Variable = true,
			GetOveride = () => Debugging.DebugDismounts.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.DebugDismounts = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "disablecondition",
			Parent = "debug",
			FullName = "debug.disablecondition",
			ServerAdmin = true,
			Description = "Do not damage any items",
			Variable = true,
			GetOveride = () => Debugging.disablecondition.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.disablecondition = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "drink",
			Parent = "debug",
			FullName = "debug.drink",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.drink(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "eat",
			Parent = "debug",
			FullName = "debug.eat",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.eat(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "flushgroup",
			Parent = "debug",
			FullName = "debug.flushgroup",
			ServerAdmin = true,
			Description = "Takes you in and out of your current network group, causing you to delete and then download all entities in your PVS again",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.flushgroup(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "heal",
			Parent = "debug",
			FullName = "debug.heal",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.heal(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "hurt",
			Parent = "debug",
			FullName = "debug.hurt",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.hurt(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "log",
			Parent = "debug",
			FullName = "debug.log",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Debugging.log.ToString(),
			SetOveride = delegate(string str)
			{
				Debugging.log = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "puzzlereset",
			Parent = "debug",
			FullName = "debug.puzzlereset",
			ServerAdmin = true,
			Description = "reset all puzzles",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.puzzlereset(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "refillvitals",
			Parent = "debug",
			FullName = "debug.refillvitals",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.refillvitals(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "renderinfo",
			Parent = "debug",
			FullName = "debug.renderinfo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.renderinfo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "resetsleepingbagtimers",
			Parent = "debug",
			FullName = "debug.resetsleepingbagtimers",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.ResetSleepingBagTimers(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stall",
			Parent = "debug",
			FullName = "debug.stall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Debugging.stall(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_0_blockcount",
			Parent = "decay",
			FullName = "decay.bracket_0_blockcount",
			ServerAdmin = true,
			Description = "Between 0 and this value are considered bracket 0 and will cost bracket_0_costfraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_0_blockcount.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_0_blockcount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_0_costfraction",
			Parent = "decay",
			FullName = "decay.bracket_0_costfraction",
			ServerAdmin = true,
			Description = "blocks within bracket 0 will cost this fraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_0_costfraction.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_0_costfraction = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_1_blockcount",
			Parent = "decay",
			FullName = "decay.bracket_1_blockcount",
			ServerAdmin = true,
			Description = "Between bracket_0_blockcount and this value are considered bracket 1 and will cost bracket_1_costfraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_1_blockcount.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_1_blockcount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_1_costfraction",
			Parent = "decay",
			FullName = "decay.bracket_1_costfraction",
			ServerAdmin = true,
			Description = "blocks within bracket 1 will cost this fraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_1_costfraction.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_1_costfraction = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_2_blockcount",
			Parent = "decay",
			FullName = "decay.bracket_2_blockcount",
			ServerAdmin = true,
			Description = "Between bracket_1_blockcount and this value are considered bracket 2 and will cost bracket_2_costfraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_2_blockcount.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_2_blockcount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_2_costfraction",
			Parent = "decay",
			FullName = "decay.bracket_2_costfraction",
			ServerAdmin = true,
			Description = "blocks within bracket 2 will cost this fraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_2_costfraction.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_2_costfraction = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_3_blockcount",
			Parent = "decay",
			FullName = "decay.bracket_3_blockcount",
			ServerAdmin = true,
			Description = "Between bracket_2_blockcount and this value (and beyond) are considered bracket 3 and will cost bracket_3_costfraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_3_blockcount.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_3_blockcount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bracket_3_costfraction",
			Parent = "decay",
			FullName = "decay.bracket_3_costfraction",
			ServerAdmin = true,
			Description = "blocks within bracket 3 will cost this fraction per upkeep period to maintain",
			Variable = true,
			GetOveride = () => ConVar.Decay.bracket_3_costfraction.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.bracket_3_costfraction = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debug",
			Parent = "decay",
			FullName = "decay.debug",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Decay.debug.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.debug = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_metal",
			Parent = "decay",
			FullName = "decay.delay_metal",
			ServerAdmin = true,
			Description = "How long should this building grade decay be delayed when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_metal.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_metal = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_override",
			Parent = "decay",
			FullName = "decay.delay_override",
			ServerAdmin = true,
			Description = "When set to a value above 0 everything will decay with this delay",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_override.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_override = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_stone",
			Parent = "decay",
			FullName = "decay.delay_stone",
			ServerAdmin = true,
			Description = "How long should this building grade decay be delayed when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_stone.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_stone = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_toptier",
			Parent = "decay",
			FullName = "decay.delay_toptier",
			ServerAdmin = true,
			Description = "How long should this building grade decay be delayed when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_toptier.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_toptier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_twig",
			Parent = "decay",
			FullName = "decay.delay_twig",
			ServerAdmin = true,
			Description = "How long should this building grade decay be delayed when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_twig.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_twig = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "delay_wood",
			Parent = "decay",
			FullName = "decay.delay_wood",
			ServerAdmin = true,
			Description = "How long should this building grade decay be delayed when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.delay_wood.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.delay_wood = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_metal",
			Parent = "decay",
			FullName = "decay.duration_metal",
			ServerAdmin = true,
			Description = "How long should this building grade take to decay when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_metal.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_metal = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_override",
			Parent = "decay",
			FullName = "decay.duration_override",
			ServerAdmin = true,
			Description = "When set to a value above 0 everything will decay with this duration",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_override.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_override = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_stone",
			Parent = "decay",
			FullName = "decay.duration_stone",
			ServerAdmin = true,
			Description = "How long should this building grade take to decay when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_stone.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_stone = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_toptier",
			Parent = "decay",
			FullName = "decay.duration_toptier",
			ServerAdmin = true,
			Description = "How long should this building grade take to decay when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_toptier.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_toptier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_twig",
			Parent = "decay",
			FullName = "decay.duration_twig",
			ServerAdmin = true,
			Description = "How long should this building grade take to decay when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_twig.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_twig = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "duration_wood",
			Parent = "decay",
			FullName = "decay.duration_wood",
			ServerAdmin = true,
			Description = "How long should this building grade take to decay when not protected by upkeep, in hours",
			Variable = true,
			GetOveride = () => ConVar.Decay.duration_wood.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.duration_wood = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outside_test_range",
			Parent = "decay",
			FullName = "decay.outside_test_range",
			ServerAdmin = true,
			Description = "Maximum distance to test to see if a structure is outside, higher values are slower but accurate for huge buildings",
			Variable = true,
			GetOveride = () => ConVar.Decay.outside_test_range.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.outside_test_range = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scale",
			Parent = "decay",
			FullName = "decay.scale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Decay.scale.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.scale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tick",
			Parent = "decay",
			FullName = "decay.tick",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Decay.tick.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.tick = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "upkeep",
			Parent = "decay",
			FullName = "decay.upkeep",
			ServerAdmin = true,
			Description = "Is upkeep enabled",
			Variable = true,
			GetOveride = () => ConVar.Decay.upkeep.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.upkeep = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "upkeep_grief_protection",
			Parent = "decay",
			FullName = "decay.upkeep_grief_protection",
			ServerAdmin = true,
			Description = "How many minutes can the upkeep cost last after the cupboard was destroyed? default : 1440 (24 hours)",
			Variable = true,
			GetOveride = () => ConVar.Decay.upkeep_grief_protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.upkeep_grief_protection = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "upkeep_heal_scale",
			Parent = "decay",
			FullName = "decay.upkeep_heal_scale",
			ServerAdmin = true,
			Description = "Scale at which objects heal when upkeep conditions are met, default of 1 is same rate at which they decay",
			Variable = true,
			GetOveride = () => ConVar.Decay.upkeep_heal_scale.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.upkeep_heal_scale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "upkeep_inside_decay_scale",
			Parent = "decay",
			FullName = "decay.upkeep_inside_decay_scale",
			ServerAdmin = true,
			Description = "Scale at which objects decay when they are inside, default of 0.1",
			Variable = true,
			GetOveride = () => ConVar.Decay.upkeep_inside_decay_scale.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.upkeep_inside_decay_scale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "upkeep_period_minutes",
			Parent = "decay",
			FullName = "decay.upkeep_period_minutes",
			ServerAdmin = true,
			Description = "How many minutes does the upkeep cost last? default : 1440 (24 hours)",
			Variable = true,
			GetOveride = () => ConVar.Decay.upkeep_period_minutes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Decay.upkeep_period_minutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "record",
			Parent = "demo",
			FullName = "demo.record",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval15 = Demo.record(arg);
				arg.ReplyWithObject(rval15);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "recordlist",
			Parent = "demo",
			FullName = "demo.recordlist",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Demo.recordlist.ToString(),
			SetOveride = delegate(string str)
			{
				Demo.recordlist = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "recordlistmode",
			Parent = "demo",
			FullName = "demo.recordlistmode",
			ServerAdmin = true,
			Saved = true,
			Description = "Controls the behavior of recordlist, 0=whitelist, 1=blacklist",
			Variable = true,
			GetOveride = () => Demo.recordlistmode.ToString(),
			SetOveride = delegate(string str)
			{
				Demo.recordlistmode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "splitmegabytes",
			Parent = "demo",
			FullName = "demo.splitmegabytes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Demo.splitmegabytes.ToString(),
			SetOveride = delegate(string str)
			{
				Demo.splitmegabytes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "splitseconds",
			Parent = "demo",
			FullName = "demo.splitseconds",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Demo.splitseconds.ToString(),
			SetOveride = delegate(string str)
			{
				Demo.splitseconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stop",
			Parent = "demo",
			FullName = "demo.stop",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval14 = Demo.stop(arg);
				arg.ReplyWithObject(rval14);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debug_toggle",
			Parent = "entity",
			FullName = "entity.debug_toggle",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.debug_toggle(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deleteby",
			Parent = "entity",
			FullName = "entity.deleteby",
			ServerAdmin = true,
			Description = "Destroy all entities created by provided users (separate users by space)",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				int num = Entity.DeleteBy(arg);
				arg.ReplyWithObject(num);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deletebytextblock",
			Parent = "entity",
			FullName = "entity.deletebytextblock",
			ServerAdmin = true,
			Description = "Destroy all entities created by users in the provided text block (can use with copied results from ent auth)",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.DeleteByTextBlock(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_entity",
			Parent = "entity",
			FullName = "entity.find_entity",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_entity(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_group",
			Parent = "entity",
			FullName = "entity.find_group",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_group(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_id",
			Parent = "entity",
			FullName = "entity.find_id",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_id(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_parent",
			Parent = "entity",
			FullName = "entity.find_parent",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_parent(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_radius",
			Parent = "entity",
			FullName = "entity.find_radius",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_radius(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_self",
			Parent = "entity",
			FullName = "entity.find_self",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_self(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "find_status",
			Parent = "entity",
			FullName = "entity.find_status",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.find_status(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nudge",
			Parent = "entity",
			FullName = "entity.nudge",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.nudge(arg.GetInt(0));
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spawnlootfrom",
			Parent = "entity",
			FullName = "entity.spawnlootfrom",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Entity.spawnlootfrom(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spawn",
			Parent = "entity",
			FullName = "entity.spawn",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval13 = Entity.svspawn(arg.GetString(0), arg.GetVector3(1, Vector3.zero), arg.GetVector3(2, Vector3.zero));
				arg.ReplyWithObject(rval13);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spawnitem",
			Parent = "entity",
			FullName = "entity.spawnitem",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval12 = Entity.svspawnitem(arg.GetString(0), arg.GetVector3(1, Vector3.zero));
				arg.ReplyWithObject(rval12);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "addtime",
			Parent = "env",
			FullName = "env.addtime",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Env.addtime(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "day",
			Parent = "env",
			FullName = "env.day",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Env.day.ToString(),
			SetOveride = delegate(string str)
			{
				Env.day = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "month",
			Parent = "env",
			FullName = "env.month",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Env.month.ToString(),
			SetOveride = delegate(string str)
			{
				Env.month = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "oceanlevel",
			Parent = "env",
			FullName = "env.oceanlevel",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Env.oceanlevel.ToString(),
			SetOveride = delegate(string str)
			{
				Env.oceanlevel = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "progresstime",
			Parent = "env",
			FullName = "env.progresstime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Env.progresstime.ToString(),
			SetOveride = delegate(string str)
			{
				Env.progresstime = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "time",
			Parent = "env",
			FullName = "env.time",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Env.time.ToString(),
			SetOveride = delegate(string str)
			{
				Env.time = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "year",
			Parent = "env",
			FullName = "env.year",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Env.year.ToString(),
			SetOveride = delegate(string str)
			{
				Env.year = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "limit",
			Parent = "fps",
			FullName = "fps.limit",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => FPS.limit.ToString(),
			SetOveride = delegate(string str)
			{
				FPS.limit = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "set",
			Parent = "gamemode",
			FullName = "gamemode.set",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				gamemode.set(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "setteam",
			Parent = "gamemode",
			FullName = "gamemode.setteam",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				gamemode.setteam(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "alloc",
			Parent = "gc",
			FullName = "gc.alloc",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				GC.alloc(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "collect",
			Parent = "gc",
			FullName = "gc.collect",
			ServerAdmin = true,
			Variable = false,
			Call = delegate
			{
				GC.collect();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "gc",
			FullName = "gc.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GC.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				GC.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "incremental_enabled",
			Parent = "gc",
			FullName = "gc.incremental_enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GC.incremental_enabled.ToString(),
			SetOveride = delegate(string str)
			{
				GC.incremental_enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "incremental_milliseconds",
			Parent = "gc",
			FullName = "gc.incremental_milliseconds",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GC.incremental_milliseconds.ToString(),
			SetOveride = delegate(string str)
			{
				GC.incremental_milliseconds = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "unload",
			Parent = "gc",
			FullName = "gc.unload",
			ServerAdmin = true,
			Variable = false,
			Call = delegate
			{
				GC.unload();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "asyncwarmup",
			Parent = "global",
			FullName = "global.asyncwarmup",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.asyncWarmup.ToString(),
			SetOveride = delegate(string str)
			{
				Global.asyncWarmup = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "breakclothing",
			Parent = "global",
			FullName = "global.breakclothing",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.breakclothing(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "breakitem",
			Parent = "global",
			FullName = "global.breakitem",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.breakitem(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearallsprays",
			Parent = "global",
			FullName = "global.clearallsprays",
			ServerAdmin = true,
			Variable = false,
			Call = delegate
			{
				Global.ClearAllSprays();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearallspraysbyplayer",
			Parent = "global",
			FullName = "global.clearallspraysbyplayer",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.ClearAllSpraysByPlayer(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clearspraysinradius",
			Parent = "global",
			FullName = "global.clearspraysinradius",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.ClearSpraysInRadius(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "colliders",
			Parent = "global",
			FullName = "global.colliders",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.colliders(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "developer",
			Parent = "global",
			FullName = "global.developer",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.developer.ToString(),
			SetOveride = delegate(string str)
			{
				Global.developer = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "error",
			Parent = "global",
			FullName = "global.error",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.error(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forceunloadbundles",
			Parent = "global",
			FullName = "global.forceunloadbundles",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.forceUnloadBundles.ToString(),
			SetOveride = delegate(string str)
			{
				Global.forceUnloadBundles = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "free",
			Parent = "global",
			FullName = "global.free",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.free(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "injure",
			Parent = "global",
			FullName = "global.injure",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.injure(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "kill",
			Parent = "global",
			FullName = "global.kill",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.kill(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxspraysperplayer",
			Parent = "global",
			FullName = "global.maxspraysperplayer",
			ServerAdmin = true,
			Saved = true,
			Description = "If a player sprays more than this, the oldest spray will be destroyed. 0 will disable",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Global.MaxSpraysPerPlayer.ToString(),
			SetOveride = delegate(string str)
			{
				Global.MaxSpraysPerPlayer = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxthreads",
			Parent = "global",
			FullName = "global.maxthreads",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.maxthreads.ToString(),
			SetOveride = delegate(string str)
			{
				Global.maxthreads = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "objects",
			Parent = "global",
			FullName = "global.objects",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.objects(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "perf",
			Parent = "global",
			FullName = "global.perf",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Global.perf.ToString(),
			SetOveride = delegate(string str)
			{
				Global.perf = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "preloadconcurrency",
			Parent = "global",
			FullName = "global.preloadconcurrency",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.preloadConcurrency.ToString(),
			SetOveride = delegate(string str)
			{
				Global.preloadConcurrency = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "queue",
			Parent = "global",
			FullName = "global.queue",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.queue(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "quit",
			Parent = "global",
			FullName = "global.quit",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.quit(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "recover",
			Parent = "global",
			FullName = "global.recover",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.recover(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "report",
			Parent = "global",
			FullName = "global.report",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.report(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn",
			Parent = "global",
			FullName = "global.respawn",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.respawn(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn_sleepingbag",
			Parent = "global",
			FullName = "global.respawn_sleepingbag",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.respawn_sleepingbag(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn_sleepingbag_remove",
			Parent = "global",
			FullName = "global.respawn_sleepingbag_remove",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.respawn_sleepingbag_remove(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "restart",
			Parent = "global",
			FullName = "global.restart",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.restart(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "setinfo",
			Parent = "global",
			FullName = "global.setinfo",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.setinfo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "skipassetwarmup",
			Parent = "global",
			FullName = "global.skipassetwarmup",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.skipassetwarmup.ToString(),
			SetOveride = delegate(string str)
			{
				Global.skipassetwarmup = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleep",
			Parent = "global",
			FullName = "global.sleep",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.sleep(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spectate",
			Parent = "global",
			FullName = "global.spectate",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.spectate(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sprayduration",
			Parent = "global",
			FullName = "global.sprayduration",
			ServerAdmin = true,
			Saved = true,
			Description = "Base time (in seconds) that sprays last",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Global.SprayDuration.ToString(),
			SetOveride = delegate(string str)
			{
				Global.SprayDuration = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sprayoutofauthmultiplier",
			Parent = "global",
			FullName = "global.sprayoutofauthmultiplier",
			ServerAdmin = true,
			Saved = true,
			Description = "Multiplier applied to SprayDuration if a spray isn't in the sprayers auth (cannot go above 1f)",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Global.SprayOutOfAuthMultiplier.ToString(),
			SetOveride = delegate(string str)
			{
				Global.SprayOutOfAuthMultiplier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "status_sv",
			Parent = "global",
			FullName = "global.status_sv",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.status_sv(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "subscriptions",
			Parent = "global",
			FullName = "global.subscriptions",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.subscriptions(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sysinfo",
			Parent = "global",
			FullName = "global.sysinfo",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.sysinfo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sysuid",
			Parent = "global",
			FullName = "global.sysuid",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.sysuid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport",
			Parent = "global",
			FullName = "global.teleport",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport2autheditem",
			Parent = "global",
			FullName = "global.teleport2autheditem",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport2autheditem(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport2death",
			Parent = "global",
			FullName = "global.teleport2death",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport2death(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport2marker",
			Parent = "global",
			FullName = "global.teleport2marker",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport2marker(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport2me",
			Parent = "global",
			FullName = "global.teleport2me",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport2me(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleport2owneditem",
			Parent = "global",
			FullName = "global.teleport2owneditem",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleport2owneditem(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleportany",
			Parent = "global",
			FullName = "global.teleportany",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleportany(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleportlos",
			Parent = "global",
			FullName = "global.teleportlos",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleportlos(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "teleportpos",
			Parent = "global",
			FullName = "global.teleportpos",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.teleportpos(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "textures",
			Parent = "global",
			FullName = "global.textures",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.textures(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "version",
			Parent = "global",
			FullName = "global.version",
			ServerAdmin = true,
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Global.version(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "warmupconcurrency",
			Parent = "global",
			FullName = "global.warmupconcurrency",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Global.warmupConcurrency.ToString(),
			SetOveride = delegate(string str)
			{
				Global.warmupConcurrency = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "halloween",
			FullName = "halloween.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Halloween.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "murdererpopulation",
			Parent = "halloween",
			FullName = "halloween.murdererpopulation",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			Variable = true,
			GetOveride = () => Halloween.murdererpopulation.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.murdererpopulation = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrow_beancan_vs_player_dmg_modifier",
			Parent = "halloween",
			FullName = "halloween.scarecrow_beancan_vs_player_dmg_modifier",
			ServerAdmin = true,
			Description = "Modified damage from beancan explosion vs players (Default: 0.1).",
			Variable = true,
			GetOveride = () => Halloween.scarecrow_beancan_vs_player_dmg_modifier.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrow_beancan_vs_player_dmg_modifier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrow_body_dmg_modifier",
			Parent = "halloween",
			FullName = "halloween.scarecrow_body_dmg_modifier",
			ServerAdmin = true,
			Description = "Modifier to how much damage scarecrows take to the body. (Default: 0.25)",
			Variable = true,
			GetOveride = () => Halloween.scarecrow_body_dmg_modifier.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrow_body_dmg_modifier = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrow_chase_stopping_distance",
			Parent = "halloween",
			FullName = "halloween.scarecrow_chase_stopping_distance",
			ServerAdmin = true,
			Description = "Stopping distance for destinations set while chasing a target (Default: 0.5)",
			Variable = true,
			GetOveride = () => Halloween.scarecrow_chase_stopping_distance.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrow_chase_stopping_distance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrow_throw_beancan_global_delay",
			Parent = "halloween",
			FullName = "halloween.scarecrow_throw_beancan_global_delay",
			ServerAdmin = true,
			Description = "The delay globally on a server between each time a scarecrow throws a beancan (Default: 8 seconds).",
			Variable = true,
			GetOveride = () => Halloween.scarecrow_throw_beancan_global_delay.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrow_throw_beancan_global_delay = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrowpopulation",
			Parent = "halloween",
			FullName = "halloween.scarecrowpopulation",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			Variable = true,
			GetOveride = () => Halloween.scarecrowpopulation.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrowpopulation = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scarecrows_throw_beancans",
			Parent = "halloween",
			FullName = "halloween.scarecrows_throw_beancans",
			ServerAdmin = true,
			Description = "Scarecrows can throw beancans (Default: true).",
			Variable = true,
			GetOveride = () => Halloween.scarecrows_throw_beancans.ToString(),
			SetOveride = delegate(string str)
			{
				Halloween.scarecrows_throw_beancans = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "load",
			Parent = "harmony",
			FullName = "harmony.load",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Harmony.Load(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "unload",
			Parent = "harmony",
			FullName = "harmony.unload",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Harmony.Unload(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cd",
			Parent = "hierarchy",
			FullName = "hierarchy.cd",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Hierarchy.cd(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "del",
			Parent = "hierarchy",
			FullName = "hierarchy.del",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Hierarchy.del(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ls",
			Parent = "hierarchy",
			FullName = "hierarchy.ls",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Hierarchy.ls(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "copyto",
			Parent = "inventory",
			FullName = "inventory.copyto",
			ServerAdmin = true,
			Description = "Copies the players inventory to the player in front of them",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.copyTo(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "defs",
			Parent = "inventory",
			FullName = "inventory.defs",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.defs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deployloadout",
			Parent = "inventory",
			FullName = "inventory.deployloadout",
			ServerAdmin = true,
			Description = "Deploys the given loadout to a target player. eg. inventory.deployLoadout testloadout jim",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.deployLoadout(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deployloadoutinrange",
			Parent = "inventory",
			FullName = "inventory.deployloadoutinrange",
			ServerAdmin = true,
			Description = "Deploys a loadout to players in a radius eg. inventory.deployLoadoutInRange testloadout 30",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.deployLoadoutInRange(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "endloot",
			Parent = "inventory",
			FullName = "inventory.endloot",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.endloot(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "equipslot",
			Parent = "inventory",
			FullName = "inventory.equipslot",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.equipslot(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "equipslottarget",
			Parent = "inventory",
			FullName = "inventory.equipslottarget",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.equipslottarget(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "give",
			Parent = "inventory",
			FullName = "inventory.give",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.give(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "giveall",
			Parent = "inventory",
			FullName = "inventory.giveall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.giveall(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "givearm",
			Parent = "inventory",
			FullName = "inventory.givearm",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.givearm(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "giveid",
			Parent = "inventory",
			FullName = "inventory.giveid",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.giveid(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "giveto",
			Parent = "inventory",
			FullName = "inventory.giveto",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.giveto(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "lighttoggle",
			Parent = "inventory",
			FullName = "inventory.lighttoggle",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.lighttoggle(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "listloadouts",
			Parent = "inventory",
			FullName = "inventory.listloadouts",
			ServerAdmin = true,
			Description = "Prints all saved inventory loadouts",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.listloadouts(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reloaddefs",
			Parent = "inventory",
			FullName = "inventory.reloaddefs",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.reloaddefs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "resetbp",
			Parent = "inventory",
			FullName = "inventory.resetbp",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.resetbp(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "saveloadout",
			Parent = "inventory",
			FullName = "inventory.saveloadout",
			ServerAdmin = true,
			Description = "Saves the current equipped loadout of the calling player. eg. inventory.saveLoadout loaduoutname",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.saveloadout(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "unlockall",
			Parent = "inventory",
			FullName = "inventory.unlockall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Inventory.unlockall(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printmanifest",
			Parent = "manifest",
			FullName = "manifest.printmanifest",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				object rval11 = ConVar.Manifest.PrintManifest();
				arg.ReplyWithObject(rval11);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printmanifestraw",
			Parent = "manifest",
			FullName = "manifest.printmanifestraw",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				object rval10 = ConVar.Manifest.PrintManifestRaw();
				arg.ReplyWithObject(rval10);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "full",
			Parent = "memsnap",
			FullName = "memsnap.full",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				MemSnap.full(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "managed",
			Parent = "memsnap",
			FullName = "memsnap.managed",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				MemSnap.managed(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "native",
			Parent = "memsnap",
			FullName = "memsnap.native",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				MemSnap.native(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "visdebug",
			Parent = "net",
			FullName = "net.visdebug",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Net.visdebug.ToString(),
			SetOveride = delegate(string str)
			{
				Net.visdebug = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "visibilityradiusfaroverride",
			Parent = "net",
			FullName = "net.visibilityradiusfaroverride",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Net.visibilityRadiusFarOverride.ToString(),
			SetOveride = delegate(string str)
			{
				Net.visibilityRadiusFarOverride = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "visibilityradiusnearoverride",
			Parent = "net",
			FullName = "net.visibilityradiusnearoverride",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Net.visibilityRadiusNearOverride.ToString(),
			SetOveride = delegate(string str)
			{
				Net.visibilityRadiusNearOverride = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bulletaccuracy",
			Parent = "heli",
			FullName = "heli.bulletaccuracy",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PatrolHelicopter.bulletAccuracy.ToString(),
			SetOveride = delegate(string str)
			{
				PatrolHelicopter.bulletAccuracy = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bulletdamagescale",
			Parent = "heli",
			FullName = "heli.bulletdamagescale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PatrolHelicopter.bulletDamageScale.ToString(),
			SetOveride = delegate(string str)
			{
				PatrolHelicopter.bulletDamageScale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "call",
			Parent = "heli",
			FullName = "heli.call",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				PatrolHelicopter.call(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "calltome",
			Parent = "heli",
			FullName = "heli.calltome",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				PatrolHelicopter.calltome(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "drop",
			Parent = "heli",
			FullName = "heli.drop",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				PatrolHelicopter.drop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "guns",
			Parent = "heli",
			FullName = "heli.guns",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PatrolHelicopter.guns.ToString(),
			SetOveride = delegate(string str)
			{
				PatrolHelicopter.guns = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "lifetimeminutes",
			Parent = "heli",
			FullName = "heli.lifetimeminutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PatrolHelicopter.lifetimeMinutes.ToString(),
			SetOveride = delegate(string str)
			{
				PatrolHelicopter.lifetimeMinutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "strafe",
			Parent = "heli",
			FullName = "heli.strafe",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				PatrolHelicopter.strafe(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "testpuzzle",
			Parent = "heli",
			FullName = "heli.testpuzzle",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				PatrolHelicopter.testpuzzle(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "autosynctransforms",
			Parent = "physics",
			FullName = "physics.autosynctransforms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.autosynctransforms.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.autosynctransforms = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "batchsynctransforms",
			Parent = "physics",
			FullName = "physics.batchsynctransforms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.batchsynctransforms.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.batchsynctransforms = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bouncethreshold",
			Parent = "physics",
			FullName = "physics.bouncethreshold",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.bouncethreshold.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.bouncethreshold = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "droppedmode",
			Parent = "physics",
			FullName = "physics.droppedmode",
			ServerAdmin = true,
			Description = "The collision detection mode that dropped items and corpses should use",
			Variable = true,
			GetOveride = () => ConVar.Physics.droppedmode.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.droppedmode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "gravity",
			Parent = "physics",
			FullName = "physics.gravity",
			ServerAdmin = true,
			Description = "Gravity multiplier",
			Variable = true,
			GetOveride = () => ConVar.Physics.gravity.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.gravity = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "groundwatchdebug",
			Parent = "physics",
			FullName = "physics.groundwatchdebug",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.groundwatchdebug.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.groundwatchdebug = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "groundwatchdelay",
			Parent = "physics",
			FullName = "physics.groundwatchdelay",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.groundwatchdelay.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.groundwatchdelay = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "groundwatchfails",
			Parent = "physics",
			FullName = "physics.groundwatchfails",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.groundwatchfails.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.groundwatchfails = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "minsteps",
			Parent = "physics",
			FullName = "physics.minsteps",
			ServerAdmin = true,
			Description = "The slowest physics steps will operate",
			Variable = true,
			GetOveride = () => ConVar.Physics.minsteps.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.minsteps = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sendeffects",
			Parent = "physics",
			FullName = "physics.sendeffects",
			ServerAdmin = true,
			Description = "Send effects to clients when physics objects collide",
			Variable = true,
			GetOveride = () => ConVar.Physics.sendeffects.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.sendeffects = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleepthreshold",
			Parent = "physics",
			FullName = "physics.sleepthreshold",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Physics.sleepthreshold.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.sleepthreshold = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "solveriterationcount",
			Parent = "physics",
			FullName = "physics.solveriterationcount",
			ServerAdmin = true,
			Description = "The default solver iteration count permitted for any rigid bodies (default 7). Must be positive",
			Variable = true,
			GetOveride = () => ConVar.Physics.solveriterationcount.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.solveriterationcount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steps",
			Parent = "physics",
			FullName = "physics.steps",
			ServerAdmin = true,
			Description = "The amount of physics steps per second",
			Variable = true,
			GetOveride = () => ConVar.Physics.steps.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Physics.steps = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "abandonmission",
			Parent = "player",
			FullName = "player.abandonmission",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.abandonmission(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cinematic_gesture",
			Parent = "player",
			FullName = "player.cinematic_gesture",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.cinematic_gesture(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cinematic_play",
			Parent = "player",
			FullName = "player.cinematic_play",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.cinematic_play(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cinematic_stop",
			Parent = "player",
			FullName = "player.cinematic_stop",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.cinematic_stop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "copyrotation",
			Parent = "player",
			FullName = "player.copyrotation",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.copyrotation(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "createskull",
			Parent = "player",
			FullName = "player.createskull",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.createskull(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "dismount",
			Parent = "player",
			FullName = "player.dismount",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.dismount(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fillwater",
			Parent = "player",
			FullName = "player.fillwater",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.fillwater(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "gesture_radius",
			Parent = "player",
			FullName = "player.gesture_radius",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.gesture_radius(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "gotosleep",
			Parent = "player",
			FullName = "player.gotosleep",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.gotosleep(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "markhostile",
			Parent = "player",
			FullName = "player.markhostile",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.markhostile(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "mount",
			Parent = "player",
			FullName = "player.mount",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.mount(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printpresence",
			Parent = "player",
			FullName = "player.printpresence",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.printpresence(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printstats",
			Parent = "player",
			FullName = "player.printstats",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.printstats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "resetstate",
			Parent = "player",
			FullName = "player.resetstate",
			ServerAdmin = true,
			Description = "Resets the PlayerState of the given player",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.resetstate(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stopgesture_radius",
			Parent = "player",
			FullName = "player.stopgesture_radius",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.stopgesture_radius(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "swapseat",
			Parent = "player",
			FullName = "player.swapseat",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.swapseat(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickrate_cl",
			Parent = "player",
			FullName = "player.tickrate_cl",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Player.tickrate_cl.ToString(),
			SetOveride = delegate(string str)
			{
				Player.tickrate_cl = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickrate_sv",
			Parent = "player",
			FullName = "player.tickrate_sv",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Player.tickrate_sv.ToString(),
			SetOveride = delegate(string str)
			{
				Player.tickrate_sv = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wakeup",
			Parent = "player",
			FullName = "player.wakeup",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.wakeup(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wakeupall",
			Parent = "player",
			FullName = "player.wakeupall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Player.wakeupall(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "woundforever",
			Parent = "player",
			FullName = "player.woundforever",
			ServerAdmin = true,
			Saved = true,
			Description = "Whether the crawling state expires",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Player.woundforever.ToString(),
			SetOveride = delegate(string str)
			{
				Player.woundforever = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clear_assets",
			Parent = "pool",
			FullName = "pool.clear_assets",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.clear_assets(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clear_memory",
			Parent = "pool",
			FullName = "pool.clear_memory",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.clear_memory(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "clear_prefabs",
			Parent = "pool",
			FullName = "pool.clear_prefabs",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.clear_prefabs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debug",
			Parent = "pool",
			FullName = "pool.debug",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Pool.debug.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Pool.debug = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "pool",
			FullName = "pool.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Pool.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Pool.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "export_prefabs",
			Parent = "pool",
			FullName = "pool.export_prefabs",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.export_prefabs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "mode",
			Parent = "pool",
			FullName = "pool.mode",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Pool.mode.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Pool.mode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "prewarm",
			Parent = "pool",
			FullName = "pool.prewarm",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Pool.prewarm.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Pool.prewarm = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "print_assets",
			Parent = "pool",
			FullName = "pool.print_assets",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.print_assets(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "print_memory",
			Parent = "pool",
			FullName = "pool.print_memory",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.print_memory(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "print_prefabs",
			Parent = "pool",
			FullName = "pool.print_prefabs",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Pool.print_prefabs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "start",
			Parent = "profile",
			FullName = "profile.start",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Profile.start(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stop",
			Parent = "profile",
			FullName = "profile.stop",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.Profile.stop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "hostileduration",
			Parent = "sentry",
			FullName = "sentry.hostileduration",
			ServerAdmin = true,
			Description = "how long until something is considered hostile after it attacked",
			Variable = true,
			GetOveride = () => Sentry.hostileduration.ToString(),
			SetOveride = delegate(string str)
			{
				Sentry.hostileduration = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "targetall",
			Parent = "sentry",
			FullName = "sentry.targetall",
			ServerAdmin = true,
			Description = "target everyone regardless of authorization",
			Variable = true,
			GetOveride = () => Sentry.targetall.ToString(),
			SetOveride = delegate(string str)
			{
				Sentry.targetall = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "arrowarmor",
			Parent = "server",
			FullName = "server.arrowarmor",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.arrowarmor.ToString(),
			SetOveride = delegate(string str)
			{
				Server.arrowarmor = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "arrowdamage",
			Parent = "server",
			FullName = "server.arrowdamage",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.arrowdamage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.arrowdamage = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "artificialtemperaturegrowablerange",
			Parent = "server",
			FullName = "server.artificialtemperaturegrowablerange",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.artificialTemperatureGrowableRange.ToString(),
			SetOveride = delegate(string str)
			{
				Server.artificialTemperatureGrowableRange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "authtimeout",
			Parent = "server",
			FullName = "server.authtimeout",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.authtimeout.ToString(),
			SetOveride = delegate(string str)
			{
				Server.authtimeout = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "backup",
			Parent = "server",
			FullName = "server.backup",
			ServerAdmin = true,
			Description = "Backup server folder",
			Variable = false,
			Call = delegate
			{
				Server.backup();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bansserverendpoint",
			Parent = "server",
			FullName = "server.bansserverendpoint",
			ServerAdmin = true,
			Description = "HTTP API endpoint for centralized banning (see wiki)",
			Variable = true,
			GetOveride = () => Server.bansServerEndpoint.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bansServerEndpoint = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bansserverfailuremode",
			Parent = "server",
			FullName = "server.bansserverfailuremode",
			ServerAdmin = true,
			Description = "Failure mode for centralized banning, set to 1 to reject players from joining if it's down (see wiki)",
			Variable = true,
			GetOveride = () => Server.bansServerFailureMode.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bansServerFailureMode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bansservertimeout",
			Parent = "server",
			FullName = "server.bansservertimeout",
			ServerAdmin = true,
			Description = "Timeout (in seconds) for centralized banning web server requests",
			Variable = true,
			GetOveride = () => Server.bansServerTimeout.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bansServerTimeout = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bleedingarmor",
			Parent = "server",
			FullName = "server.bleedingarmor",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.bleedingarmor.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bleedingarmor = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bleedingdamage",
			Parent = "server",
			FullName = "server.bleedingdamage",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.bleedingdamage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bleedingdamage = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "branch",
			Parent = "server",
			FullName = "server.branch",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.branch.ToString(),
			SetOveride = delegate(string str)
			{
				Server.branch = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bulletarmor",
			Parent = "server",
			FullName = "server.bulletarmor",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.bulletarmor.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bulletarmor = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "bulletdamage",
			Parent = "server",
			FullName = "server.bulletdamage",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.bulletdamage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.bulletdamage = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ceilinglightgrowablerange",
			Parent = "server",
			FullName = "server.ceilinglightgrowablerange",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.ceilingLightGrowableRange.ToString(),
			SetOveride = delegate(string str)
			{
				Server.ceilingLightGrowableRange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ceilinglightheightoffset",
			Parent = "server",
			FullName = "server.ceilinglightheightoffset",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.ceilingLightHeightOffset.ToString(),
			SetOveride = delegate(string str)
			{
				Server.ceilingLightHeightOffset = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "censorplayerlist",
			Parent = "server",
			FullName = "server.censorplayerlist",
			ServerAdmin = true,
			Description = "Censors the Steam player list to make player tracking more difficult",
			Variable = true,
			GetOveride = () => Server.censorplayerlist.ToString(),
			SetOveride = delegate(string str)
			{
				Server.censorplayerlist = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cheatreport",
			Parent = "server",
			FullName = "server.cheatreport",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.cheatreport(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cinematic",
			Parent = "server",
			FullName = "server.cinematic",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.cinematic.ToString(),
			SetOveride = delegate(string str)
			{
				Server.cinematic = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "combatlog",
			Parent = "server",
			FullName = "server.combatlog",
			ServerAdmin = true,
			ServerUser = true,
			Description = "Get the player combat log",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval9 = Server.combatlog(arg);
				arg.ReplyWithObject(rval9);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "combatlog_outgoing",
			Parent = "server",
			FullName = "server.combatlog_outgoing",
			ServerAdmin = true,
			ServerUser = true,
			Description = "Get the player combat log, only showing outgoing damage",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval8 = Server.combatlog_outgoing(arg);
				arg.ReplyWithObject(rval8);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "combatlogdelay",
			Parent = "server",
			FullName = "server.combatlogdelay",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.combatlogdelay.ToString(),
			SetOveride = delegate(string str)
			{
				Server.combatlogdelay = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "combatlogsize",
			Parent = "server",
			FullName = "server.combatlogsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.combatlogsize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.combatlogsize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "composterupdateinterval",
			Parent = "server",
			FullName = "server.composterupdateinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.composterUpdateInterval.ToString(),
			SetOveride = delegate(string str)
			{
				Server.composterUpdateInterval = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "compression",
			Parent = "server",
			FullName = "server.compression",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.compression.ToString(),
			SetOveride = delegate(string str)
			{
				Server.compression = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "corpsedespawn",
			Parent = "server",
			FullName = "server.corpsedespawn",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.corpsedespawn.ToString(),
			SetOveride = delegate(string str)
			{
				Server.corpsedespawn = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "corpses",
			Parent = "server",
			FullName = "server.corpses",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.corpses.ToString(),
			SetOveride = delegate(string str)
			{
				Server.corpses = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "crawlingmaximumhealth",
			Parent = "server",
			FullName = "server.crawlingmaximumhealth",
			ServerAdmin = true,
			Description = "Maximum initial health given when a player dies and moves to crawling wounded state",
			Variable = true,
			GetOveride = () => Server.crawlingmaximumhealth.ToString(),
			SetOveride = delegate(string str)
			{
				Server.crawlingmaximumhealth = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "crawlingminimumhealth",
			Parent = "server",
			FullName = "server.crawlingminimumhealth",
			ServerAdmin = true,
			Description = "Minimum initial health given when a player dies and moves to crawling wounded state",
			Variable = true,
			GetOveride = () => Server.crawlingminimumhealth.ToString(),
			SetOveride = delegate(string str)
			{
				Server.crawlingminimumhealth = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cycletime",
			Parent = "server",
			FullName = "server.cycletime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.cycletime.ToString(),
			SetOveride = delegate(string str)
			{
				Server.cycletime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "debrisdespawn",
			Parent = "server",
			FullName = "server.debrisdespawn",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.debrisdespawn.ToString(),
			SetOveride = delegate(string str)
			{
				Server.debrisdespawn = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "description",
			Parent = "server",
			FullName = "server.description",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.description.ToString(),
			SetOveride = delegate(string str)
			{
				Server.description = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "dropitems",
			Parent = "server",
			FullName = "server.dropitems",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.dropitems.ToString(),
			SetOveride = delegate(string str)
			{
				Server.dropitems = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "encryption",
			Parent = "server",
			FullName = "server.encryption",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.encryption.ToString(),
			SetOveride = delegate(string str)
			{
				Server.encryption = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "entitybatchsize",
			Parent = "server",
			FullName = "server.entitybatchsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.entitybatchsize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.entitybatchsize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "entitybatchtime",
			Parent = "server",
			FullName = "server.entitybatchtime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.entitybatchtime.ToString(),
			SetOveride = delegate(string str)
			{
				Server.entitybatchtime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "entityrate",
			Parent = "server",
			FullName = "server.entityrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.entityrate.ToString(),
			SetOveride = delegate(string str)
			{
				Server.entityrate = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "events",
			Parent = "server",
			FullName = "server.events",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.events.ToString(),
			SetOveride = delegate(string str)
			{
				Server.events = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fps",
			Parent = "server",
			FullName = "server.fps",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.fps(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "funwaterdamagethreshold",
			Parent = "server",
			FullName = "server.funwaterdamagethreshold",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Saved = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Server.funWaterDamageThreshold.ToString(),
			SetOveride = delegate(string str)
			{
				Server.funWaterDamageThreshold = str.ToFloat();
			},
			Default = "0.8"
		},
		new ConsoleSystem.Command
		{
			Name = "funwaterwetnessgain",
			Parent = "server",
			FullName = "server.funwaterwetnessgain",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Saved = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Server.funWaterWetnessGain.ToString(),
			SetOveride = delegate(string str)
			{
				Server.funWaterWetnessGain = str.ToFloat();
			},
			Default = "0.05"
		},
		new ConsoleSystem.Command
		{
			Name = "gamemode",
			Parent = "server",
			FullName = "server.gamemode",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.gamemode.ToString(),
			SetOveride = delegate(string str)
			{
				Server.gamemode = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "globalchat",
			Parent = "server",
			FullName = "server.globalchat",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.globalchat.ToString(),
			SetOveride = delegate(string str)
			{
				Server.globalchat = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "headerimage",
			Parent = "server",
			FullName = "server.headerimage",
			ServerAdmin = true,
			Saved = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.headerimage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.headerimage = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "hostname",
			Parent = "server",
			FullName = "server.hostname",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.hostname.ToString(),
			SetOveride = delegate(string str)
			{
				Server.hostname = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "identity",
			Parent = "server",
			FullName = "server.identity",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.identity.ToString(),
			SetOveride = delegate(string str)
			{
				Server.identity = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "idlekick",
			Parent = "server",
			FullName = "server.idlekick",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.idlekick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.idlekick = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "idlekickadmins",
			Parent = "server",
			FullName = "server.idlekickadmins",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.idlekickadmins.ToString(),
			SetOveride = delegate(string str)
			{
				Server.idlekickadmins = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "idlekickmode",
			Parent = "server",
			FullName = "server.idlekickmode",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.idlekickmode.ToString(),
			SetOveride = delegate(string str)
			{
				Server.idlekickmode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "incapacitatedrecoverchance",
			Parent = "server",
			FullName = "server.incapacitatedrecoverchance",
			ServerAdmin = true,
			Saved = true,
			Description = "Base chance of recovery after incapacitated wounded state",
			Variable = true,
			GetOveride = () => Server.incapacitatedrecoverchance.ToString(),
			SetOveride = delegate(string str)
			{
				Server.incapacitatedrecoverchance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ip",
			Parent = "server",
			FullName = "server.ip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.ip.ToString(),
			SetOveride = delegate(string str)
			{
				Server.ip = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ipqueriespermin",
			Parent = "server",
			FullName = "server.ipqueriespermin",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.ipQueriesPerMin.ToString(),
			SetOveride = delegate(string str)
			{
				Server.ipQueriesPerMin = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "itemdespawn",
			Parent = "server",
			FullName = "server.itemdespawn",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.itemdespawn.ToString(),
			SetOveride = delegate(string str)
			{
				Server.itemdespawn = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "itemdespawn_quick",
			Parent = "server",
			FullName = "server.itemdespawn_quick",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.itemdespawn_quick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.itemdespawn_quick = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "level",
			Parent = "server",
			FullName = "server.level",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.level.ToString(),
			SetOveride = delegate(string str)
			{
				Server.level = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "leveltransfer",
			Parent = "server",
			FullName = "server.leveltransfer",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.leveltransfer.ToString(),
			SetOveride = delegate(string str)
			{
				Server.leveltransfer = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "levelurl",
			Parent = "server",
			FullName = "server.levelurl",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.levelurl.ToString(),
			SetOveride = delegate(string str)
			{
				Server.levelurl = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "listtoolcupboards",
			Parent = "server",
			FullName = "server.listtoolcupboards",
			ServerAdmin = true,
			Description = "Prints all the Tool Cupboards on the server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.listtoolcupboards(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "listvendingmachines",
			Parent = "server",
			FullName = "server.listvendingmachines",
			ServerAdmin = true,
			Description = "Prints all the vending machines on the server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.listvendingmachines(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "logoimage",
			Parent = "server",
			FullName = "server.logoimage",
			ServerAdmin = true,
			Saved = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.logoimage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.logoimage = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxconnectionsperip",
			Parent = "server",
			FullName = "server.maxconnectionsperip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxconnectionsperip.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxconnectionsperip = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketsize",
			Parent = "server",
			FullName = "server.maxpacketsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketsize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketsize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketsize_command",
			Parent = "server",
			FullName = "server.maxpacketsize_command",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketsize_command.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketsize_command = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond",
			Parent = "server",
			FullName = "server.maxpacketspersecond",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_command",
			Parent = "server",
			FullName = "server.maxpacketspersecond_command",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_command.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_command = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_rpc",
			Parent = "server",
			FullName = "server.maxpacketspersecond_rpc",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_rpc.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_rpc = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_rpc_signal",
			Parent = "server",
			FullName = "server.maxpacketspersecond_rpc_signal",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_rpc_signal.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_rpc_signal = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_tick",
			Parent = "server",
			FullName = "server.maxpacketspersecond_tick",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_tick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_tick = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_voice",
			Parent = "server",
			FullName = "server.maxpacketspersecond_voice",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_voice.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_voice = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxpacketspersecond_world",
			Parent = "server",
			FullName = "server.maxpacketspersecond_world",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxpacketspersecond_world.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxpacketspersecond_world = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxplayers",
			Parent = "server",
			FullName = "server.maxplayers",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.maxplayers.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxplayers = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxreceivetime",
			Parent = "server",
			FullName = "server.maxreceivetime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxreceivetime.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxreceivetime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxunack",
			Parent = "server",
			FullName = "server.maxunack",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.maxunack.ToString(),
			SetOveride = delegate(string str)
			{
				Server.maxunack = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "meleearmor",
			Parent = "server",
			FullName = "server.meleearmor",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.meleearmor.ToString(),
			SetOveride = delegate(string str)
			{
				Server.meleearmor = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "meleedamage",
			Parent = "server",
			FullName = "server.meleedamage",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.meleedamage.ToString(),
			SetOveride = delegate(string str)
			{
				Server.meleedamage = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "metabolismtick",
			Parent = "server",
			FullName = "server.metabolismtick",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.metabolismtick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.metabolismtick = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "modifiertickrate",
			Parent = "server",
			FullName = "server.modifiertickrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.modifierTickRate.ToString(),
			SetOveride = delegate(string str)
			{
				Server.modifierTickRate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "motd",
			Parent = "server",
			FullName = "server.motd",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Saved = true,
			Replicated = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.motd.ToString(),
			SetOveride = delegate(string str)
			{
				Server.motd = str;
			},
			Default = ""
		},
		new ConsoleSystem.Command
		{
			Name = "netcache",
			Parent = "server",
			FullName = "server.netcache",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.netcache.ToString(),
			SetOveride = delegate(string str)
			{
				Server.netcache = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "netcachesize",
			Parent = "server",
			FullName = "server.netcachesize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.netcachesize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.netcachesize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "netlog",
			Parent = "server",
			FullName = "server.netlog",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.netlog.ToString(),
			SetOveride = delegate(string str)
			{
				Server.netlog = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nonplanterdeathchancepertick",
			Parent = "server",
			FullName = "server.nonplanterdeathchancepertick",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.nonPlanterDeathChancePerTick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.nonPlanterDeathChancePerTick = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "official",
			Parent = "server",
			FullName = "server.official",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.official.ToString(),
			SetOveride = delegate(string str)
			{
				Server.official = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "optimalplanterqualitysaturation",
			Parent = "server",
			FullName = "server.optimalplanterqualitysaturation",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.optimalPlanterQualitySaturation.ToString(),
			SetOveride = delegate(string str)
			{
				Server.optimalPlanterQualitySaturation = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "packetlog",
			Parent = "server",
			FullName = "server.packetlog",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval7 = Server.packetlog(arg);
				arg.ReplyWithObject(rval7);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "packetlog_enabled",
			Parent = "server",
			FullName = "server.packetlog_enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.packetlog_enabled.ToString(),
			SetOveride = delegate(string str)
			{
				Server.packetlog_enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "plantlightdetection",
			Parent = "server",
			FullName = "server.plantlightdetection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.plantlightdetection.ToString(),
			SetOveride = delegate(string str)
			{
				Server.plantlightdetection = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "planttick",
			Parent = "server",
			FullName = "server.planttick",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Server.planttick.ToString(),
			SetOveride = delegate(string str)
			{
				Server.planttick = str.ToFloat();
			},
			Default = "60"
		},
		new ConsoleSystem.Command
		{
			Name = "planttickscale",
			Parent = "server",
			FullName = "server.planttickscale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.planttickscale.ToString(),
			SetOveride = delegate(string str)
			{
				Server.planttickscale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "playerlistpos",
			Parent = "server",
			FullName = "server.playerlistpos",
			ServerAdmin = true,
			Description = "Prints the position of all players on the server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.playerlistpos(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "playerserverfall",
			Parent = "server",
			FullName = "server.playerserverfall",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.playerserverfall.ToString(),
			SetOveride = delegate(string str)
			{
				Server.playerserverfall = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "playertimeout",
			Parent = "server",
			FullName = "server.playertimeout",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.playertimeout.ToString(),
			SetOveride = delegate(string str)
			{
				Server.playertimeout = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "port",
			Parent = "server",
			FullName = "server.port",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.port.ToString(),
			SetOveride = delegate(string str)
			{
				Server.port = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printeyes",
			Parent = "server",
			FullName = "server.printeyes",
			ServerAdmin = true,
			Description = "Print the current player eyes.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval6 = Server.printeyes(arg);
				arg.ReplyWithObject(rval6);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printpos",
			Parent = "server",
			FullName = "server.printpos",
			ServerAdmin = true,
			Description = "Print the current player position.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval5 = Server.printpos(arg);
				arg.ReplyWithObject(rval5);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printreportstoconsole",
			Parent = "server",
			FullName = "server.printreportstoconsole",
			ServerAdmin = true,
			Saved = true,
			Description = "Should F7 reports from players be printed to console",
			Variable = true,
			GetOveride = () => Server.printReportsToConsole.ToString(),
			SetOveride = delegate(string str)
			{
				Server.printReportsToConsole = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printrot",
			Parent = "server",
			FullName = "server.printrot",
			ServerAdmin = true,
			Description = "Print the current player rotation.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval4 = Server.printrot(arg);
				arg.ReplyWithObject(rval4);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "pve",
			Parent = "server",
			FullName = "server.pve",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.pve.ToString(),
			SetOveride = delegate(string str)
			{
				Server.pve = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "queriespersecond",
			Parent = "server",
			FullName = "server.queriespersecond",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.queriesPerSecond.ToString(),
			SetOveride = delegate(string str)
			{
				Server.queriesPerSecond = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "queryport",
			Parent = "server",
			FullName = "server.queryport",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.queryport.ToString(),
			SetOveride = delegate(string str)
			{
				Server.queryport = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "radiation",
			Parent = "server",
			FullName = "server.radiation",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.radiation.ToString(),
			SetOveride = delegate(string str)
			{
				Server.radiation = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "readcfg",
			Parent = "server",
			FullName = "server.readcfg",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval3 = Server.readcfg(arg);
				arg.ReplyWithObject(rval3);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reportsserverendpoint",
			Parent = "server",
			FullName = "server.reportsserverendpoint",
			ServerAdmin = true,
			Saved = true,
			Description = "HTTP API endpoint for receiving F7 reports",
			Variable = true,
			GetOveride = () => Server.reportsServerEndpoint.ToString(),
			SetOveride = delegate(string str)
			{
				Server.reportsServerEndpoint = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reportsserverendpointkey",
			Parent = "server",
			FullName = "server.reportsserverendpointkey",
			ServerAdmin = true,
			Saved = true,
			Description = "If set, this key will be included with any reports sent via reportsServerEndpoint (for validation)",
			Variable = true,
			GetOveride = () => Server.reportsServerEndpointKey.ToString(),
			SetOveride = delegate(string str)
			{
				Server.reportsServerEndpointKey = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawnresetrange",
			Parent = "server",
			FullName = "server.respawnresetrange",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.respawnresetrange.ToString(),
			SetOveride = delegate(string str)
			{
				Server.respawnresetrange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rewounddelay",
			Parent = "server",
			FullName = "server.rewounddelay",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.rewounddelay.ToString(),
			SetOveride = delegate(string str)
			{
				Server.rewounddelay = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rpclog",
			Parent = "server",
			FullName = "server.rpclog",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval2 = Server.rpclog(arg);
				arg.ReplyWithObject(rval2);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rpclog_enabled",
			Parent = "server",
			FullName = "server.rpclog_enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.rpclog_enabled.ToString(),
			SetOveride = delegate(string str)
			{
				Server.rpclog_enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "salt",
			Parent = "server",
			FullName = "server.salt",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.salt.ToString(),
			SetOveride = delegate(string str)
			{
				Server.salt = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "save",
			Parent = "server",
			FullName = "server.save",
			ServerAdmin = true,
			Description = "Force save the current game",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.save(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "savebackupcount",
			Parent = "server",
			FullName = "server.savebackupcount",
			ServerAdmin = true,
			Saved = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.saveBackupCount.ToString(),
			SetOveride = delegate(string str)
			{
				Server.saveBackupCount = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "savecachesize",
			Parent = "server",
			FullName = "server.savecachesize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.savecachesize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.savecachesize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "saveinterval",
			Parent = "server",
			FullName = "server.saveinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.saveinterval.ToString(),
			SetOveride = delegate(string str)
			{
				Server.saveinterval = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "schematime",
			Parent = "server",
			FullName = "server.schematime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.schematime.ToString(),
			SetOveride = delegate(string str)
			{
				Server.schematime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "secure",
			Parent = "server",
			FullName = "server.secure",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.secure.ToString(),
			SetOveride = delegate(string str)
			{
				Server.secure = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "seed",
			Parent = "server",
			FullName = "server.seed",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.seed.ToString(),
			SetOveride = delegate(string str)
			{
				Server.seed = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sendnetworkupdate",
			Parent = "server",
			FullName = "server.sendnetworkupdate",
			ServerAdmin = true,
			Description = "Send network update for all players",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.sendnetworkupdate(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "setshowholstereditems",
			Parent = "server",
			FullName = "server.setshowholstereditems",
			ServerAdmin = true,
			Description = "Show holstered items on player bodies",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.setshowholstereditems(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "showholstereditems",
			Parent = "server",
			FullName = "server.showholstereditems",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.showHolsteredItems.ToString(),
			SetOveride = delegate(string str)
			{
				Server.showHolsteredItems = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "snapshot",
			Parent = "server",
			FullName = "server.snapshot",
			ServerAdmin = true,
			Description = "This sends a snapshot of all the entities in the client's pvs. This is mostly redundant, but we request this when the client starts recording a demo.. so they get all the information.",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.snapshot(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sprinklereyeheightoffset",
			Parent = "server",
			FullName = "server.sprinklereyeheightoffset",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.sprinklerEyeHeightOffset.ToString(),
			SetOveride = delegate(string str)
			{
				Server.sprinklerEyeHeightOffset = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sprinklerradius",
			Parent = "server",
			FullName = "server.sprinklerradius",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.sprinklerRadius.ToString(),
			SetOveride = delegate(string str)
			{
				Server.sprinklerRadius = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stability",
			Parent = "server",
			FullName = "server.stability",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.stability.ToString(),
			SetOveride = delegate(string str)
			{
				Server.stability = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "start",
			Parent = "server",
			FullName = "server.start",
			ServerAdmin = true,
			Description = "Starts a server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.start(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "statbackup",
			Parent = "server",
			FullName = "server.statbackup",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.statBackup.ToString(),
			SetOveride = delegate(string str)
			{
				Server.statBackup = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stats",
			Parent = "server",
			FullName = "server.stats",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.stats.ToString(),
			SetOveride = delegate(string str)
			{
				Server.stats = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stop",
			Parent = "server",
			FullName = "server.stop",
			ServerAdmin = true,
			Description = "Stops a server",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.stop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tags",
			Parent = "server",
			FullName = "server.tags",
			ServerAdmin = true,
			Saved = true,
			Description = "Comma-separated server browser tag values (see wiki)",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.tags.ToString(),
			SetOveride = delegate(string str)
			{
				Server.tags = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tickrate",
			Parent = "server",
			FullName = "server.tickrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.tickrate.ToString(),
			SetOveride = delegate(string str)
			{
				Server.tickrate = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "updatebatch",
			Parent = "server",
			FullName = "server.updatebatch",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.updatebatch.ToString(),
			SetOveride = delegate(string str)
			{
				Server.updatebatch = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "updatebatchspawn",
			Parent = "server",
			FullName = "server.updatebatchspawn",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.updatebatchspawn.ToString(),
			SetOveride = delegate(string str)
			{
				Server.updatebatchspawn = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "url",
			Parent = "server",
			FullName = "server.url",
			ServerAdmin = true,
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Server.url.ToString(),
			SetOveride = delegate(string str)
			{
				Server.url = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "useminimumplantcondition",
			Parent = "server",
			FullName = "server.useminimumplantcondition",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.useMinimumPlantCondition.ToString(),
			SetOveride = delegate(string str)
			{
				Server.useMinimumPlantCondition = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "worldsize",
			Parent = "server",
			FullName = "server.worldsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Server.worldsize.ToString(),
			SetOveride = delegate(string str)
			{
				Server.worldsize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "woundedmaxfoodandwaterbonus",
			Parent = "server",
			FullName = "server.woundedmaxfoodandwaterbonus",
			ServerAdmin = true,
			Saved = true,
			Description = "Maximum percent chance added to base wounded/incapacitated recovery chance, based on the player's food and water level",
			Variable = true,
			GetOveride = () => Server.woundedmaxfoodandwaterbonus.ToString(),
			SetOveride = delegate(string str)
			{
				Server.woundedmaxfoodandwaterbonus = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "woundedrecoverchance",
			Parent = "server",
			FullName = "server.woundedrecoverchance",
			ServerAdmin = true,
			Saved = true,
			Description = "Base chance of recovery after crawling wounded state",
			Variable = true,
			GetOveride = () => Server.woundedrecoverchance.ToString(),
			SetOveride = delegate(string str)
			{
				Server.woundedrecoverchance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "woundingenabled",
			Parent = "server",
			FullName = "server.woundingenabled",
			ServerAdmin = true,
			Saved = true,
			Variable = true,
			GetOveride = () => Server.woundingenabled.ToString(),
			SetOveride = delegate(string str)
			{
				Server.woundingenabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "writecfg",
			Parent = "server",
			FullName = "server.writecfg",
			ServerAdmin = true,
			Description = "Writes config files",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Server.writecfg(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cargoshipevent",
			Parent = "spawn",
			FullName = "spawn.cargoshipevent",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.cargoshipevent(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fill_groups",
			Parent = "spawn",
			FullName = "spawn.fill_groups",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.fill_groups(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fill_individuals",
			Parent = "spawn",
			FullName = "spawn.fill_individuals",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.fill_individuals(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fill_populations",
			Parent = "spawn",
			FullName = "spawn.fill_populations",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.fill_populations(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "max_density",
			Parent = "spawn",
			FullName = "spawn.max_density",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.max_density.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.max_density = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "max_rate",
			Parent = "spawn",
			FullName = "spawn.max_rate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.max_rate.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.max_rate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "min_density",
			Parent = "spawn",
			FullName = "spawn.min_density",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.min_density.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.min_density = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "min_rate",
			Parent = "spawn",
			FullName = "spawn.min_rate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.min_rate.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.min_rate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "player_base",
			Parent = "spawn",
			FullName = "spawn.player_base",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.player_base.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.player_base = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "player_scale",
			Parent = "spawn",
			FullName = "spawn.player_scale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.player_scale.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.player_scale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "report",
			Parent = "spawn",
			FullName = "spawn.report",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.report(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn_groups",
			Parent = "spawn",
			FullName = "spawn.respawn_groups",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.respawn_groups.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.respawn_groups = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn_individuals",
			Parent = "spawn",
			FullName = "spawn.respawn_individuals",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.respawn_individuals.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.respawn_individuals = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "respawn_populations",
			Parent = "spawn",
			FullName = "spawn.respawn_populations",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.respawn_populations.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.respawn_populations = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "scalars",
			Parent = "spawn",
			FullName = "spawn.scalars",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Spawn.scalars(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tick_individuals",
			Parent = "spawn",
			FullName = "spawn.tick_individuals",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.tick_individuals.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.tick_individuals = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "tick_populations",
			Parent = "spawn",
			FullName = "spawn.tick_populations",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Spawn.tick_populations.ToString(),
			SetOveride = delegate(string str)
			{
				Spawn.tick_populations = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "accuracy",
			Parent = "stability",
			FullName = "stability.accuracy",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.accuracy.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.accuracy = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "collapse",
			Parent = "stability",
			FullName = "stability.collapse",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.collapse.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.collapse = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "refresh_stability",
			Parent = "stability",
			FullName = "stability.refresh_stability",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Stability.refresh_stability(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stabilityqueue",
			Parent = "stability",
			FullName = "stability.stabilityqueue",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.stabilityqueue.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.stabilityqueue = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "strikes",
			Parent = "stability",
			FullName = "stability.strikes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.strikes.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.strikes = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "surroundingsqueue",
			Parent = "stability",
			FullName = "stability.surroundingsqueue",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.surroundingsqueue.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.surroundingsqueue = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "verbose",
			Parent = "stability",
			FullName = "stability.verbose",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Stability.verbose.ToString(),
			SetOveride = delegate(string str)
			{
				Stability.verbose = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "call",
			Parent = "supply",
			FullName = "supply.call",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Supply.call(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "drop",
			Parent = "supply",
			FullName = "supply.drop",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Supply.drop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fixeddelta",
			Parent = "time",
			FullName = "time.fixeddelta",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Time.fixeddelta.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Time.fixeddelta = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxdelta",
			Parent = "time",
			FullName = "time.maxdelta",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Time.maxdelta.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Time.maxdelta = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "pausewhileloading",
			Parent = "time",
			FullName = "time.pausewhileloading",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Time.pausewhileloading.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Time.pausewhileloading = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "timescale",
			Parent = "time",
			FullName = "time.timescale",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Time.timescale.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Time.timescale = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "global_broadcast",
			Parent = "tree",
			FullName = "tree.global_broadcast",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Tree.global_broadcast.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Tree.global_broadcast = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "boat_corpse_seconds",
			Parent = "vehicle",
			FullName = "vehicle.boat_corpse_seconds",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => vehicle.boat_corpse_seconds.ToString(),
			SetOveride = delegate(string str)
			{
				vehicle.boat_corpse_seconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "carwrecks",
			Parent = "vehicle",
			FullName = "vehicle.carwrecks",
			ServerAdmin = true,
			Description = "Determines whether modular cars turn into wrecks when destroyed, or just immediately gib. Default: true",
			Variable = true,
			GetOveride = () => vehicle.carwrecks.ToString(),
			SetOveride = delegate(string str)
			{
				vehicle.carwrecks = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cinematictrains",
			Parent = "vehicle",
			FullName = "vehicle.cinematictrains",
			ServerAdmin = true,
			Description = "If true, trains always explode when destroyed, and hitting a barrier always destroys the train immediately. Default: false",
			Variable = true,
			GetOveride = () => vehicle.cinematictrains.ToString(),
			SetOveride = delegate(string str)
			{
				vehicle.cinematictrains = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fixcars",
			Parent = "vehicle",
			FullName = "vehicle.fixcars",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				vehicle.fixcars(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "stop_all_trains",
			Parent = "vehicle",
			FullName = "vehicle.stop_all_trains",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				vehicle.stop_all_trains(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "swapseats",
			Parent = "vehicle",
			FullName = "vehicle.swapseats",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				vehicle.swapseats(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "trainskeeprunning",
			Parent = "vehicle",
			FullName = "vehicle.trainskeeprunning",
			ServerAdmin = true,
			Description = "Determines whether trains stop automatically when there's no-one on them. Default: false",
			Variable = true,
			GetOveride = () => vehicle.trainskeeprunning.ToString(),
			SetOveride = delegate(string str)
			{
				vehicle.trainskeeprunning = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "vehiclesdroploot",
			Parent = "vehicle",
			FullName = "vehicle.vehiclesdroploot",
			ServerAdmin = true,
			Description = "Determines whether vehicles drop storage items when destroyed. Default: true",
			Variable = true,
			GetOveride = () => vehicle.vehiclesdroploot.ToString(),
			SetOveride = delegate(string str)
			{
				vehicle.vehiclesdroploot = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "attack",
			Parent = "vis",
			FullName = "vis.attack",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.attack.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.attack = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "damage",
			Parent = "vis",
			FullName = "vis.damage",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.damage.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.damage = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "hitboxes",
			Parent = "vis",
			FullName = "vis.hitboxes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.hitboxes.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.hitboxes = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "lineofsight",
			Parent = "vis",
			FullName = "vis.lineofsight",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.lineofsight.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.lineofsight = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "protection",
			Parent = "vis",
			FullName = "vis.protection",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.protection.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.protection = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sense",
			Parent = "vis",
			FullName = "vis.sense",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.sense.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.sense = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "triggers",
			Parent = "vis",
			FullName = "vis.triggers",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.triggers.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.triggers = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "weakspots",
			Parent = "vis",
			FullName = "vis.weakspots",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.Vis.weakspots.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.Vis.weakspots = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "atmosphere_brightness",
			Parent = "weather",
			FullName = "weather.atmosphere_brightness",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.atmosphere_brightness.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.atmosphere_brightness = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "atmosphere_contrast",
			Parent = "weather",
			FullName = "weather.atmosphere_contrast",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.atmosphere_contrast.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.atmosphere_contrast = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "atmosphere_directionality",
			Parent = "weather",
			FullName = "weather.atmosphere_directionality",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.atmosphere_directionality.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.atmosphere_directionality = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "atmosphere_mie",
			Parent = "weather",
			FullName = "weather.atmosphere_mie",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.atmosphere_mie.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.atmosphere_mie = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "atmosphere_rayleigh",
			Parent = "weather",
			FullName = "weather.atmosphere_rayleigh",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.atmosphere_rayleigh.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.atmosphere_rayleigh = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "clear_chance",
			Parent = "weather",
			FullName = "weather.clear_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.clear_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.clear_chance = str.ToFloat();
			},
			Default = "1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_attenuation",
			Parent = "weather",
			FullName = "weather.cloud_attenuation",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_attenuation.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_attenuation = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_brightness",
			Parent = "weather",
			FullName = "weather.cloud_brightness",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_brightness.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_brightness = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_coloring",
			Parent = "weather",
			FullName = "weather.cloud_coloring",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_coloring.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_coloring = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_coverage",
			Parent = "weather",
			FullName = "weather.cloud_coverage",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_coverage.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_coverage = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_opacity",
			Parent = "weather",
			FullName = "weather.cloud_opacity",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_opacity.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_opacity = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_saturation",
			Parent = "weather",
			FullName = "weather.cloud_saturation",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_saturation.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_saturation = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_scattering",
			Parent = "weather",
			FullName = "weather.cloud_scattering",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_scattering.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_scattering = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_sharpness",
			Parent = "weather",
			FullName = "weather.cloud_sharpness",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_sharpness.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_sharpness = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "cloud_size",
			Parent = "weather",
			FullName = "weather.cloud_size",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.cloud_size.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.cloud_size = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "dust_chance",
			Parent = "weather",
			FullName = "weather.dust_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.dust_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.dust_chance = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "fog",
			Parent = "weather",
			FullName = "weather.fog",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.fog.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.fog = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "fog_chance",
			Parent = "weather",
			FullName = "weather.fog_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.fog_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.fog_chance = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "load",
			Parent = "weather",
			FullName = "weather.load",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Weather.load(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "overcast_chance",
			Parent = "weather",
			FullName = "weather.overcast_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.overcast_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.overcast_chance = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "rain",
			Parent = "weather",
			FullName = "weather.rain",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.rain.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.rain = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "rain_chance",
			Parent = "weather",
			FullName = "weather.rain_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.rain_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.rain_chance = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "rainbow",
			Parent = "weather",
			FullName = "weather.rainbow",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.rainbow.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.rainbow = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "report",
			Parent = "weather",
			FullName = "weather.report",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Weather.report(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reset",
			Parent = "weather",
			FullName = "weather.reset",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Weather.reset(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "storm_chance",
			Parent = "weather",
			FullName = "weather.storm_chance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.storm_chance.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.storm_chance = str.ToFloat();
			},
			Default = "0"
		},
		new ConsoleSystem.Command
		{
			Name = "thunder",
			Parent = "weather",
			FullName = "weather.thunder",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.thunder.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.thunder = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "wetness_rain",
			Parent = "weather",
			FullName = "weather.wetness_rain",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Weather.wetness_rain.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.wetness_rain = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wetness_snow",
			Parent = "weather",
			FullName = "weather.wetness_snow",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => Weather.wetness_snow.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.wetness_snow = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wind",
			Parent = "weather",
			FullName = "weather.wind",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Weather.wind.ToString(),
			SetOveride = delegate(string str)
			{
				Weather.wind = str.ToFloat();
			},
			Default = "-1"
		},
		new ConsoleSystem.Command
		{
			Name = "print_approved_skins",
			Parent = "workshop",
			FullName = "workshop.print_approved_skins",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Workshop.print_approved_skins(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cache",
			Parent = "world",
			FullName = "world.cache",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ConVar.World.cache.ToString(),
			SetOveride = delegate(string str)
			{
				ConVar.World.cache = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "monuments",
			Parent = "world",
			FullName = "world.monuments",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.World.monuments(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "renderlabs",
			Parent = "world",
			FullName = "world.renderlabs",
			ServerAdmin = true,
			Client = true,
			Description = "Renders a PNG of the current map's underwater labs, for a specific floor",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.World.renderlabs(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rendermap",
			Parent = "world",
			FullName = "world.rendermap",
			ServerAdmin = true,
			Client = true,
			Description = "Renders a high resolution PNG of the current map",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.World.rendermap(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rendertunnels",
			Parent = "world",
			FullName = "world.rendertunnels",
			ServerAdmin = true,
			Client = true,
			Description = "Renders a PNG of the current map's tunnel network",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ConVar.World.rendertunnels(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "enabled",
			Parent = "xmas",
			FullName = "xmas.enabled",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => XMas.enabled.ToString(),
			SetOveride = delegate(string str)
			{
				XMas.enabled = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "giftsperplayer",
			Parent = "xmas",
			FullName = "xmas.giftsperplayer",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => XMas.giftsPerPlayer.ToString(),
			SetOveride = delegate(string str)
			{
				XMas.giftsPerPlayer = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "refill",
			Parent = "xmas",
			FullName = "xmas.refill",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				XMas.refill(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spawnattempts",
			Parent = "xmas",
			FullName = "xmas.spawnattempts",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => XMas.spawnAttempts.ToString(),
			SetOveride = delegate(string str)
			{
				XMas.spawnAttempts = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "spawnrange",
			Parent = "xmas",
			FullName = "xmas.spawnrange",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => XMas.spawnRange.ToString(),
			SetOveride = delegate(string str)
			{
				XMas.spawnRange = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "endtest",
			Parent = "cui",
			FullName = "cui.endtest",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				cui.endtest(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "test",
			Parent = "cui",
			FullName = "cui.test",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				cui.test(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "dump",
			Parent = "global",
			FullName = "global.dump",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				DiagnosticsConSys.dump(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "use_baked_terrain_mesh",
			Parent = "dungeonnavmesh",
			FullName = "dungeonnavmesh.use_baked_terrain_mesh",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => DungeonNavmesh.use_baked_terrain_mesh.ToString(),
			SetOveride = delegate(string str)
			{
				DungeonNavmesh.use_baked_terrain_mesh = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "use_baked_terrain_mesh",
			Parent = "dynamicnavmesh",
			FullName = "dynamicnavmesh.use_baked_terrain_mesh",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => DynamicNavMesh.use_baked_terrain_mesh.ToString(),
			SetOveride = delegate(string str)
			{
				DynamicNavMesh.use_baked_terrain_mesh = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "chargeneededforsupplies",
			Parent = "excavatorsignalcomputer",
			FullName = "excavatorsignalcomputer.chargeneededforsupplies",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ExcavatorSignalComputer.chargeNeededForSupplies.ToString(),
			SetOveride = delegate(string str)
			{
				ExcavatorSignalComputer.chargeNeededForSupplies = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamconnectiontimeout",
			Parent = "global",
			FullName = "global.steamconnectiontimeout",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamconnectiontimeout.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamconnectiontimeout = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug",
			Parent = "global",
			FullName = "global.steamnetdebug",
			ServerAdmin = true,
			Description = "Turns on varying levels of debug output for the Steam Networking. This will affect performance. (0 = off, 1 = bug, 2 = error, 3 = important, 4 = warning, 5 = message, 6 = verbose, 7 = debug, 8 = everything)",
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_ackrtt",
			Parent = "global",
			FullName = "global.steamnetdebug_ackrtt",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_ackrtt.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_ackrtt = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_message",
			Parent = "global",
			FullName = "global.steamnetdebug_message",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_message.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_message = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_p2prendezvous",
			Parent = "global",
			FullName = "global.steamnetdebug_p2prendezvous",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_p2prendezvous.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_p2prendezvous = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_packetdecode",
			Parent = "global",
			FullName = "global.steamnetdebug_packetdecode",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_packetdecode.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_packetdecode = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_packetgaps",
			Parent = "global",
			FullName = "global.steamnetdebug_packetgaps",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_packetgaps.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_packetgaps = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamnetdebug_sdrrelaypings",
			Parent = "global",
			FullName = "global.steamnetdebug_sdrrelaypings",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SteamNetworking.steamnetdebug_sdrrelaypings.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamnetdebug_sdrrelaypings = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamrelayinit",
			Parent = "global",
			FullName = "global.steamrelayinit",
			ServerAdmin = true,
			Variable = false,
			Call = delegate
			{
				SteamNetworking.steamrelayinit();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamsendbuffer",
			Parent = "global",
			FullName = "global.steamsendbuffer",
			ServerAdmin = true,
			Description = "Upper limit of buffered pending bytes to be sent",
			Variable = true,
			GetOveride = () => SteamNetworking.steamsendbuffer.ToString(),
			SetOveride = delegate(string str)
			{
				SteamNetworking.steamsendbuffer = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "steamstatus",
			Parent = "global",
			FullName = "global.steamstatus",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				string rval = SteamNetworking.steamstatus();
				arg.ReplyWithObject(rval);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ip",
			Parent = "rcon",
			FullName = "rcon.ip",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RCon.Ip.ToString(),
			SetOveride = delegate(string str)
			{
				RCon.Ip = str;
			}
		},
		new ConsoleSystem.Command
		{
			Name = "port",
			Parent = "rcon",
			FullName = "rcon.port",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RCon.Port.ToString(),
			SetOveride = delegate(string str)
			{
				RCon.Port = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "print",
			Parent = "rcon",
			FullName = "rcon.print",
			ServerAdmin = true,
			Description = "If true, rcon commands etc will be printed in the console",
			Variable = true,
			GetOveride = () => RCon.Print.ToString(),
			SetOveride = delegate(string str)
			{
				RCon.Print = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "web",
			Parent = "rcon",
			FullName = "rcon.web",
			ServerAdmin = true,
			Description = "If set to true, use websocket rcon. If set to false use legacy, source engine rcon.",
			Variable = true,
			GetOveride = () => RCon.Web.ToString(),
			SetOveride = delegate(string str)
			{
				RCon.Web = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "movetowardsrate",
			Parent = "frankensteinbrain",
			FullName = "frankensteinbrain.movetowardsrate",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => FrankensteinBrain.MoveTowardsRate.ToString(),
			SetOveride = delegate(string str)
			{
				FrankensteinBrain.MoveTowardsRate = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "decayminutes",
			Parent = "frankensteinpet",
			FullName = "frankensteinpet.decayminutes",
			ServerAdmin = true,
			Description = "How long before a Frankenstein Pet dies un controlled and not asleep on table",
			Variable = true,
			GetOveride = () => FrankensteinPet.decayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				FrankensteinPet.decayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reclaim_fraction_belt",
			Parent = "gamemodesoftcore",
			FullName = "gamemodesoftcore.reclaim_fraction_belt",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GameModeSoftcore.reclaim_fraction_belt.ToString(),
			SetOveride = delegate(string str)
			{
				GameModeSoftcore.reclaim_fraction_belt = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reclaim_fraction_main",
			Parent = "gamemodesoftcore",
			FullName = "gamemodesoftcore.reclaim_fraction_main",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GameModeSoftcore.reclaim_fraction_main.ToString(),
			SetOveride = delegate(string str)
			{
				GameModeSoftcore.reclaim_fraction_main = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reclaim_fraction_wear",
			Parent = "gamemodesoftcore",
			FullName = "gamemodesoftcore.reclaim_fraction_wear",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GameModeSoftcore.reclaim_fraction_wear.ToString(),
			SetOveride = delegate(string str)
			{
				GameModeSoftcore.reclaim_fraction_wear = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "framebudgetms",
			Parent = "growableentity",
			FullName = "growableentity.framebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => GrowableEntity.framebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				GrowableEntity.framebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "growall",
			Parent = "growableentity",
			FullName = "growableentity.growall",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				GrowableEntity.GrowAll(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "decayseconds",
			Parent = "hackablelockedcrate",
			FullName = "hackablelockedcrate.decayseconds",
			ServerAdmin = true,
			Description = "How many seconds until the crate is destroyed without any hack attempts",
			Variable = true,
			GetOveride = () => HackableLockedCrate.decaySeconds.ToString(),
			SetOveride = delegate(string str)
			{
				HackableLockedCrate.decaySeconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "requiredhackseconds",
			Parent = "hackablelockedcrate",
			FullName = "hackablelockedcrate.requiredhackseconds",
			ServerAdmin = true,
			Description = "How many seconds for the crate to unlock",
			Variable = true,
			GetOveride = () => HackableLockedCrate.requiredHackSeconds.ToString(),
			SetOveride = delegate(string str)
			{
				HackableLockedCrate.requiredHackSeconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "horse",
			FullName = "horse.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Horse.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Horse.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "hotairballoon",
			FullName = "hotairballoon.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a HAB loses all its health while outside",
			Variable = true,
			GetOveride = () => HotAirBalloon.outsidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				HotAirBalloon.outsidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "hotairballoon",
			FullName = "hotairballoon.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => HotAirBalloon.population.ToString(),
			SetOveride = delegate(string str)
			{
				HotAirBalloon.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "serviceceiling",
			Parent = "hotairballoon",
			FullName = "hotairballoon.serviceceiling",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => HotAirBalloon.serviceCeiling.ToString(),
			SetOveride = delegate(string str)
			{
				HotAirBalloon.serviceCeiling = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "backtracking",
			Parent = "ioentity",
			FullName = "ioentity.backtracking",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => IOEntity.backtracking.ToString(),
			SetOveride = delegate(string str)
			{
				IOEntity.backtracking = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "framebudgetms",
			Parent = "ioentity",
			FullName = "ioentity.framebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => IOEntity.framebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				IOEntity.framebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "responsetime",
			Parent = "ioentity",
			FullName = "ioentity.responsetime",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => IOEntity.responsetime.ToString(),
			SetOveride = delegate(string str)
			{
				IOEntity.responsetime = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "framebudgetms",
			Parent = "junkpilewater",
			FullName = "junkpilewater.framebudgetms",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => JunkPileWater.framebudgetms.ToString(),
			SetOveride = delegate(string str)
			{
				JunkPileWater.framebudgetms = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "megaphonevoicerange",
			Parent = "megaphone",
			FullName = "megaphone.megaphonevoicerange",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => Megaphone.MegaphoneVoiceRange.ToString(),
			SetOveride = delegate(string str)
			{
				Megaphone.MegaphoneVoiceRange = str.ToFloat();
			},
			Default = "100"
		},
		new ConsoleSystem.Command
		{
			Name = "add",
			Parent = "meta",
			FullName = "meta.add",
			ServerAdmin = true,
			Client = true,
			Description = "add <convar> <amount> - adds amount to convar",
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				Meta.add(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "insidedecayminutes",
			Parent = "minicopter",
			FullName = "minicopter.insidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a minicopter loses all its health while indoors",
			Variable = true,
			GetOveride = () => MiniCopter.insidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				MiniCopter.insidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "minicopter",
			FullName = "minicopter.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a minicopter loses all its health while outside",
			Variable = true,
			GetOveride = () => MiniCopter.outsidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				MiniCopter.outsidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "minicopter",
			FullName = "minicopter.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => MiniCopter.population.ToString(),
			SetOveride = delegate(string str)
			{
				MiniCopter.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "brokendownminutes",
			Parent = "mlrs",
			FullName = "mlrs.brokendownminutes",
			ServerAdmin = true,
			Description = "How many minutes before the MLRS recovers from use and can be used again",
			Variable = true,
			GetOveride = () => MLRS.brokenDownMinutes.ToString(),
			SetOveride = delegate(string str)
			{
				MLRS.brokenDownMinutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "modularcar",
			FullName = "modularcar.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How many minutes before a ModularCar loses all its health while outside",
			Variable = true,
			GetOveride = () => ModularCar.outsidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				ModularCar.outsidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "modularcar",
			FullName = "modularcar.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => ModularCar.population.ToString(),
			SetOveride = delegate(string str)
			{
				ModularCar.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "use_baked_terrain_mesh",
			Parent = "monumentnavmesh",
			FullName = "monumentnavmesh.use_baked_terrain_mesh",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => MonumentNavMesh.use_baked_terrain_mesh.ToString(),
			SetOveride = delegate(string str)
			{
				MonumentNavMesh.use_baked_terrain_mesh = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "deepwaterdecayminutes",
			Parent = "motorrowboat",
			FullName = "motorrowboat.deepwaterdecayminutes",
			ServerAdmin = true,
			Description = "How long before a boat loses all its health while in deep water",
			Variable = true,
			GetOveride = () => MotorRowboat.deepwaterdecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				MotorRowboat.deepwaterdecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "motorrowboat",
			FullName = "motorrowboat.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a boat loses all its health while outside. If it's in deep water, deepwaterdecayminutes is used",
			Variable = true,
			GetOveride = () => MotorRowboat.outsidedecayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				MotorRowboat.outsidedecayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "motorrowboat",
			FullName = "motorrowboat.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => MotorRowboat.population.ToString(),
			SetOveride = delegate(string str)
			{
				MotorRowboat.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "update",
			Parent = "note",
			FullName = "note.update",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				note.update(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleeperhostiledelay",
			Parent = "npcautoturret",
			FullName = "npcautoturret.sleeperhostiledelay",
			ServerAdmin = true,
			Description = "How many seconds until a sleeping player is considered hostile",
			Variable = true,
			GetOveride = () => NPCAutoTurret.sleeperhostiledelay.ToString(),
			SetOveride = delegate(string str)
			{
				NPCAutoTurret.sleeperhostiledelay = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "controldistance",
			Parent = "petbrain",
			FullName = "petbrain.controldistance",
			ServerAdmin = true,
			ClientAdmin = true,
			Client = true,
			Replicated = true,
			Variable = true,
			GetOveride = () => PetBrain.ControlDistance.ToString(),
			SetOveride = delegate(string str)
			{
				PetBrain.ControlDistance = str.ToFloat();
			},
			Default = "100"
		},
		new ConsoleSystem.Command
		{
			Name = "drownindeepwater",
			Parent = "petbrain",
			FullName = "petbrain.drownindeepwater",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PetBrain.DrownInDeepWater.ToString(),
			SetOveride = delegate(string str)
			{
				PetBrain.DrownInDeepWater = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "drowntimer",
			Parent = "petbrain",
			FullName = "petbrain.drowntimer",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PetBrain.DrownTimer.ToString(),
			SetOveride = delegate(string str)
			{
				PetBrain.DrownTimer = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "idlewhenownermounted",
			Parent = "petbrain",
			FullName = "petbrain.idlewhenownermounted",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PetBrain.IdleWhenOwnerMounted.ToString(),
			SetOveride = delegate(string str)
			{
				PetBrain.IdleWhenOwnerMounted = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "idlewhenownerofflineordead",
			Parent = "petbrain",
			FullName = "petbrain.idlewhenownerofflineordead",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PetBrain.IdleWhenOwnerOfflineOrDead.ToString(),
			SetOveride = delegate(string str)
			{
				PetBrain.IdleWhenOwnerOfflineOrDead = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forcebirthday",
			Parent = "playerinventory",
			FullName = "playerinventory.forcebirthday",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => PlayerInventory.forceBirthday.ToString(),
			SetOveride = delegate(string str)
			{
				PlayerInventory.forceBirthday = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "polarbear",
			FullName = "polarbear.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Polarbear.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Polarbear.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "reclaim_expire_minutes",
			Parent = "reclaimmanager",
			FullName = "reclaimmanager.reclaim_expire_minutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => ReclaimManager.reclaim_expire_minutes.ToString(),
			SetOveride = delegate(string str)
			{
				ReclaimManager.reclaim_expire_minutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "acceptinvite",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.acceptinvite",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.acceptinvite(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "addtoteam",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.addtoteam",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.addtoteam(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "contacts",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.contacts",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.contacts.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.contacts = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "fakeinvite",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.fakeinvite",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.fakeinvite(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forgetafterminutes",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.forgetafterminutes",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.forgetafterminutes.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.forgetafterminutes = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "kickmember",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.kickmember",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.kickmember(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "leaveteam",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.leaveteam",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.leaveteam(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxplayerrelationships",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.maxplayerrelationships",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.maxplayerrelationships.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.maxplayerrelationships = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxteamsize",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.maxteamsize",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.maxTeamSize.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.maxTeamSize = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "mugshotupdateinterval",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.mugshotupdateinterval",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.mugshotUpdateInterval.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.mugshotUpdateInterval = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "promote",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.promote",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.promote(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rejectinvite",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.rejectinvite",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.rejectinvite(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "seendistance",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.seendistance",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => RelationshipManager.seendistance.ToString(),
			SetOveride = delegate(string str)
			{
				RelationshipManager.seendistance = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sendinvite",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.sendinvite",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.sendinvite(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sleeptoggle",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.sleeptoggle",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.sleeptoggle(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "trycreateteam",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.trycreateteam",
			ServerUser = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.trycreateteam(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wipe_all_contacts",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.wipe_all_contacts",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.wipe_all_contacts(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wipecontacts",
			Parent = "relationshipmanager",
			FullName = "relationshipmanager.wipecontacts",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RelationshipManager.wipecontacts(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "rhibpopulation",
			Parent = "rhib",
			FullName = "rhib.rhibpopulation",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => RHIB.rhibpopulation.ToString(),
			SetOveride = delegate(string str)
			{
				RHIB.rhibpopulation = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "ridablehorse",
			FullName = "ridablehorse.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => RidableHorse.Population.ToString(),
			SetOveride = delegate(string str)
			{
				RidableHorse.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "sethorsebreed",
			Parent = "ridablehorse",
			FullName = "ridablehorse.sethorsebreed",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				RidableHorse.setHorseBreed(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_dormant",
			Parent = "aimanager",
			FullName = "aimanager.ai_dormant",
			ServerAdmin = true,
			Description = "If ai_dormant is true, any npc outside the range of players will render itself dormant and take up less resources, but wildlife won't simulate as well.",
			Variable = true,
			GetOveride = () => AiManager.ai_dormant.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_dormant = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_dormant_max_wakeup_per_tick",
			Parent = "aimanager",
			FullName = "aimanager.ai_dormant_max_wakeup_per_tick",
			ServerAdmin = true,
			Description = "ai_dormant_max_wakeup_per_tick defines the maximum number of dormant agents we will wake up in a single tick. (default: 30)",
			Variable = true,
			GetOveride = () => AiManager.ai_dormant_max_wakeup_per_tick.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_dormant_max_wakeup_per_tick = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_htn_animal_tick_budget",
			Parent = "aimanager",
			FullName = "aimanager.ai_htn_animal_tick_budget",
			ServerAdmin = true,
			Description = "ai_htn_animal_tick_budget defines the maximum amount of milliseconds ticking htn animal agents are allowed to consume. (default: 4 ms)",
			Variable = true,
			GetOveride = () => AiManager.ai_htn_animal_tick_budget.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_htn_animal_tick_budget = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_htn_player_junkpile_tick_budget",
			Parent = "aimanager",
			FullName = "aimanager.ai_htn_player_junkpile_tick_budget",
			ServerAdmin = true,
			Description = "ai_htn_player_junkpile_tick_budget defines the maximum amount of milliseconds ticking htn player junkpile agents are allowed to consume. (default: 4 ms)",
			Variable = true,
			GetOveride = () => AiManager.ai_htn_player_junkpile_tick_budget.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_htn_player_junkpile_tick_budget = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_htn_player_tick_budget",
			Parent = "aimanager",
			FullName = "aimanager.ai_htn_player_tick_budget",
			ServerAdmin = true,
			Description = "ai_htn_player_tick_budget defines the maximum amount of milliseconds ticking htn player agents are allowed to consume. (default: 4 ms)",
			Variable = true,
			GetOveride = () => AiManager.ai_htn_player_tick_budget.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_htn_player_tick_budget = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_htn_use_agency_tick",
			Parent = "aimanager",
			FullName = "aimanager.ai_htn_use_agency_tick",
			ServerAdmin = true,
			Description = "If ai_htn_use_agency_tick is true, the ai manager's agency system will tick htn agents at the ms budgets defined in ai_htn_player_tick_budget and ai_htn_animal_tick_budget. If it's false, each agent registers with the invoke system individually, with no frame-budget restrictions. (default: true)",
			Variable = true,
			GetOveride = () => AiManager.ai_htn_use_agency_tick.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_htn_use_agency_tick = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "ai_to_player_distance_wakeup_range",
			Parent = "aimanager",
			FullName = "aimanager.ai_to_player_distance_wakeup_range",
			ServerAdmin = true,
			Description = "If an agent is beyond this distance to a player, it's flagged for becoming dormant.",
			Variable = true,
			GetOveride = () => AiManager.ai_to_player_distance_wakeup_range.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.ai_to_player_distance_wakeup_range = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_disable",
			Parent = "aimanager",
			FullName = "aimanager.nav_disable",
			ServerAdmin = true,
			Description = "If set to true the navmesh won't generate.. which means Ai that uses the navmesh won't be able to move",
			Variable = true,
			GetOveride = () => AiManager.nav_disable.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.nav_disable = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_obstacles_carve_state",
			Parent = "aimanager",
			FullName = "aimanager.nav_obstacles_carve_state",
			ServerAdmin = true,
			Description = "nav_obstacles_carve_state defines which obstacles can carve the terrain. 0 - No carving, 1 - Only player construction carves, 2 - All obstacles carve.",
			Variable = true,
			GetOveride = () => AiManager.nav_obstacles_carve_state.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.nav_obstacles_carve_state = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "nav_wait",
			Parent = "aimanager",
			FullName = "aimanager.nav_wait",
			ServerAdmin = true,
			Description = "If true we'll wait for the navmesh to generate before completely starting the server. This might cause your server to hitch and lag as it generates in the background.",
			Variable = true,
			GetOveride = () => AiManager.nav_wait.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.nav_wait = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "pathfindingiterationsperframe",
			Parent = "aimanager",
			FullName = "aimanager.pathfindingiterationsperframe",
			ServerAdmin = true,
			Description = "The maximum amount of nodes processed each frame in the asynchronous pathfinding process. Increasing this value will cause the paths to be processed faster, but can cause some hiccups in frame rate. Default value is 100, a good range for tuning is between 50 and 500.",
			Variable = true,
			GetOveride = () => AiManager.pathfindingIterationsPerFrame.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.pathfindingIterationsPerFrame = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "setdestination_navmesh_failsafe",
			Parent = "aimanager",
			FullName = "aimanager.setdestination_navmesh_failsafe",
			ServerAdmin = true,
			Description = "If set to true, npcs will attempt to place themselves on the navmesh if not on a navmesh when set destination is called.",
			Variable = true,
			GetOveride = () => AiManager.setdestination_navmesh_failsafe.ToString(),
			SetOveride = delegate(string str)
			{
				AiManager.setdestination_navmesh_failsafe = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cover_point_sample_step_height",
			Parent = "coverpointvolume",
			FullName = "coverpointvolume.cover_point_sample_step_height",
			ServerAdmin = true,
			Description = "cover_point_sample_step_height defines the height of the steps we do vertically for the cover point volume's cover point generation (smaller steps gives more accurate cover points, but at a higher processing cost). (default: 2.0)",
			Variable = true,
			GetOveride = () => CoverPointVolume.cover_point_sample_step_height.ToString(),
			SetOveride = delegate(string str)
			{
				CoverPointVolume.cover_point_sample_step_height = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "cover_point_sample_step_size",
			Parent = "coverpointvolume",
			FullName = "coverpointvolume.cover_point_sample_step_size",
			ServerAdmin = true,
			Description = "cover_point_sample_step_size defines the size of the steps we do horizontally for the cover point volume's cover point generation (smaller steps gives more accurate cover points, but at a higher processing cost). (default: 6.0)",
			Variable = true,
			GetOveride = () => CoverPointVolume.cover_point_sample_step_size.ToString(),
			SetOveride = delegate(string str)
			{
				CoverPointVolume.cover_point_sample_step_size = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "staticrepairseconds",
			Parent = "samsite",
			FullName = "samsite.staticrepairseconds",
			ServerAdmin = true,
			Description = "how long until static sam sites auto repair",
			Variable = true,
			GetOveride = () => SamSite.staticrepairseconds.ToString(),
			SetOveride = delegate(string str)
			{
				SamSite.staticrepairseconds = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "altitudeaboveterrain",
			Parent = "santasleigh",
			FullName = "santasleigh.altitudeaboveterrain",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SantaSleigh.altitudeAboveTerrain.ToString(),
			SetOveride = delegate(string str)
			{
				SantaSleigh.altitudeAboveTerrain = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "desiredaltitude",
			Parent = "santasleigh",
			FullName = "santasleigh.desiredaltitude",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SantaSleigh.desiredAltitude.ToString(),
			SetOveride = delegate(string str)
			{
				SantaSleigh.desiredAltitude = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "drop",
			Parent = "santasleigh",
			FullName = "santasleigh.drop",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				SantaSleigh.drop(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "scraptransporthelicopter",
			FullName = "scraptransporthelicopter.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => ScrapTransportHelicopter.population.ToString(),
			SetOveride = delegate(string str)
			{
				ScrapTransportHelicopter.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "disable",
			Parent = "simpleshark",
			FullName = "simpleshark.disable",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SimpleShark.disable.ToString(),
			SetOveride = delegate(string str)
			{
				SimpleShark.disable = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forcesurfaceamount",
			Parent = "simpleshark",
			FullName = "simpleshark.forcesurfaceamount",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SimpleShark.forceSurfaceAmount.ToString(),
			SetOveride = delegate(string str)
			{
				SimpleShark.forceSurfaceAmount = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "forcepayoutindex",
			Parent = "slotmachine",
			FullName = "slotmachine.forcepayoutindex",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => SlotMachine.ForcePayoutIndex.ToString(),
			SetOveride = delegate(string str)
			{
				SlotMachine.ForcePayoutIndex = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "allowpassengeronly",
			Parent = "snowmobile",
			FullName = "snowmobile.allowpassengeronly",
			ServerAdmin = true,
			Description = "Allow mounting as a passenger when there's no driver",
			Variable = true,
			GetOveride = () => Snowmobile.allowPassengerOnly.ToString(),
			SetOveride = delegate(string str)
			{
				Snowmobile.allowPassengerOnly = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "allterrain",
			Parent = "snowmobile",
			FullName = "snowmobile.allterrain",
			ServerAdmin = true,
			Description = "If true, snowmobile goes fast on all terrain types",
			Variable = true,
			GetOveride = () => Snowmobile.allTerrain.ToString(),
			SetOveride = delegate(string str)
			{
				Snowmobile.allTerrain = str.ToBool();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "outsidedecayminutes",
			Parent = "snowmobile",
			FullName = "snowmobile.outsidedecayminutes",
			ServerAdmin = true,
			Description = "How long before a snowmobile loses all its health while outside",
			Variable = true,
			GetOveride = () => Snowmobile.outsideDecayMinutes.ToString(),
			SetOveride = delegate(string str)
			{
				Snowmobile.outsideDecayMinutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "stag",
			FullName = "stag.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Stag.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Stag.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxcalllength",
			Parent = "telephonemanager",
			FullName = "telephonemanager.maxcalllength",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => TelephoneManager.MaxCallLength.ToString(),
			SetOveride = delegate(string str)
			{
				TelephoneManager.MaxCallLength = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "maxconcurrentcalls",
			Parent = "telephonemanager",
			FullName = "telephonemanager.maxconcurrentcalls",
			ServerAdmin = true,
			Variable = true,
			GetOveride = () => TelephoneManager.MaxConcurrentCalls.ToString(),
			SetOveride = delegate(string str)
			{
				TelephoneManager.MaxConcurrentCalls = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "printallphones",
			Parent = "telephonemanager",
			FullName = "telephonemanager.printallphones",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				TelephoneManager.PrintAllPhones(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "decayminutes",
			Parent = "traincar",
			FullName = "traincar.decayminutes",
			ServerAdmin = true,
			Description = "How long before a train car despawns",
			Variable = true,
			GetOveride = () => TrainCar.decayminutes.ToString(),
			SetOveride = delegate(string str)
			{
				TrainCar.decayminutes = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "traincar",
			FullName = "traincar.population",
			ServerAdmin = true,
			Description = "Population active on the server",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => TrainCar.population.ToString(),
			SetOveride = delegate(string str)
			{
				TrainCar.population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "wagons_per_engine",
			Parent = "traincar",
			FullName = "traincar.wagons_per_engine",
			ServerAdmin = true,
			Description = "Ratio of wagons to train engines that spawn",
			Variable = true,
			GetOveride = () => TrainCar.wagons_per_engine.ToString(),
			SetOveride = delegate(string str)
			{
				TrainCar.wagons_per_engine = str.ToInt();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "decayminutesafterunload",
			Parent = "traincarunloadable",
			FullName = "traincarunloadable.decayminutesafterunload",
			ServerAdmin = true,
			Description = "How long before an unloadable train car despawns afer being unloaded",
			Variable = true,
			GetOveride = () => TrainCarUnloadable.decayminutesafterunload.ToString(),
			SetOveride = delegate(string str)
			{
				TrainCarUnloadable.decayminutesafterunload = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "max_couple_speed",
			Parent = "traincouplingcontroller",
			FullName = "traincouplingcontroller.max_couple_speed",
			ServerAdmin = true,
			Description = "Maximum difference in velocity for train cars to couple",
			Variable = true,
			GetOveride = () => TrainCouplingController.max_couple_speed.ToString(),
			SetOveride = delegate(string str)
			{
				TrainCouplingController.max_couple_speed = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "wolf",
			FullName = "wolf.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Wolf.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Wolf.Population = str.ToFloat();
			}
		},
		new ConsoleSystem.Command
		{
			Name = "report",
			Parent = "ziplinelaunchpoint",
			FullName = "ziplinelaunchpoint.report",
			ServerAdmin = true,
			Variable = false,
			Call = delegate(ConsoleSystem.Arg arg)
			{
				ZiplineLaunchPoint.report(arg);
			}
		},
		new ConsoleSystem.Command
		{
			Name = "population",
			Parent = "zombie",
			FullName = "zombie.population",
			ServerAdmin = true,
			Description = "Population active on the server, per square km",
			ShowInAdminUI = true,
			Variable = true,
			GetOveride = () => Zombie.Population.ToString(),
			SetOveride = delegate(string str)
			{
				Zombie.Population = str.ToFloat();
			}
		}
	};
}

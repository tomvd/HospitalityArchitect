using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

[StaticConstructorOnStartup]
public class Command_SetGuestType : Command
{
	private CompHotelGuestBed hotelGuestBed;
	private MarketingService _marketingService;

	public Command_SetGuestType(CompHotelGuestBed hotelGuestBed)
	{
		_marketingService = hotelGuestBed.parent.Map.GetComponent<MarketingService>();
		this.hotelGuestBed = hotelGuestBed;
		if (hotelGuestBed.guestType != null)
		{
			defaultLabel = hotelGuestBed.guestType.LabelCap;
		}
		defaultDesc = "SetGuestType";//.Translate(); TODO translate... some day
	}

	public override void ProcessInput(Event ev)
	{
		base.ProcessInput(ev);
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		var marketingData = _marketingService.MarketingData
			.Where(data => !data.Key.GetModExtension<GuestTypeDef>().dayVisitor)
			.OrderBy(data => data.Key.GetModExtension<GuestTypeDef>().bedBudget).ToList();
		foreach (var data in marketingData)
		{
			list.Add(new FloatMenuOption(data.Key.LabelCap, delegate
			{
				hotelGuestBed.guestType = data.Key;
			}, (Texture2D)null, Color.white));			
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}
}
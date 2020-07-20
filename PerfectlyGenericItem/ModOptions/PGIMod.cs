using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerfectlyGenericItem
{

	public sealed class PGIMod : Mod
	{
		public static PGIMod Instance;
		public PGISettings settings;

		public override string SettingsCategory() => "Perfectly Generic Item";
		public PGIMod(ModContentPack content) : base(content)
		{
			this.settings = GetSettings<PGISettings>();
			PGIMod.Instance = this;
			PGISettings.Instance = base.GetSettings<PGISettings>();
		}

		public override void DoSettingsWindowContents(Rect rect)
		{
			Listing_Standard list = new Listing_Standard()
			{
				ColumnWidth = rect.width
			};
			float lineheight = (Text.LineHeight + list.verticalSpacing);
			list.Begin(rect);

			Listing_Standard listing_FungalLabel = list.BeginSection(lineheight);
			listing_FungalLabel.ColumnWidth *= 0.488f;
			listing_FungalLabel.CheckboxLabeled("PGI_RemoveOption".Translate(), ref settings.removePGI, "PGI_RemoveOptionToolTip".Translate());
			list.EndSection(listing_FungalLabel);

			list.End();

		}
		public override void WriteSettings()
		{
			base.WriteSettings();
		}

	}
}

using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;

namespace HideoutRequirementIndicator
{
    public class HideoutRequirementIndicatorMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            DoPatching();
        }

        public static void DoPatching()
        {
            var harmony = new HarmonyLib.Harmony("VIP.TommySoucy.HideoutRequirementIndicator");

            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    class ShowPatch
    {
        // This postfix essentially overrides the sprite and its color after it has been set by Show()
        // Just to make it different in case it is a hideout requirement
        [HarmonyPatch(typeof(EFT.UI.DragAndDrop.QuestItemViewPanel), nameof(EFT.UI.DragAndDrop.QuestItemViewPanel.Show))]
        static void Postfix(EFT.Profile profile, EFT.InventoryLogic.Item item, EFT.UI.SimpleTooltip tooltip, EFT.UI.DragAndDrop.QuestItemViewPanel __instance,
                            ref Image ____questIconImage, ref Sprite ____foundInRaidSprite, ref string ___string_3, ref EFT.UI.SimpleTooltip ___simpleTooltip_0)
        {
            string template = item.TemplateId;
            bool foundNeeded = false;
            bool foundFullfilled = false;
            List<string> areaNames = new List<string>();

            GClass1251 hideoutInstance = Comfort.Common.Singleton<GClass1251>.Instance;
            foreach (EFT.Hideout.AreaData ad in hideoutInstance.AreaDatas)
            {
                EFT.Hideout.RelatedRequirements requirements = ad.NextStage.Requirements;

                foreach (GClass1278 requirement in requirements)
                {
                    EFT.Hideout.ItemRequirement itemRequirement = requirement as EFT.Hideout.ItemRequirement;
                    if (itemRequirement != null)
                    {
                        string requirementTemplate = itemRequirement.TemplateId;
                        if (template == requirementTemplate)
                        {
                            // A requirement but already have the amount we need
                            if (requirement.Fulfilled)
                            {
                                // Even if we have enough of this item to fulfill a requirement in one area
                                // we might still need it, and if thats the case we want to show that color, not fulfilled color, so you know you still need more of it
                                // So only set color to fulfilled if not needed
                                if (!foundNeeded && !foundFullfilled)
                                {
                                    // Following calls base class method ShowGameObject()
                                    // To call base methods without reverse patch, must modify IL code for this line from callvirt to call
                                    (__instance as EFT.UI.UIElement).ShowGameObject(false);
                                    ____questIconImage.sprite = ____foundInRaidSprite;
                                    ____questIconImage.color = new Color(0.23137f, 0.93725f, 1);

                                    foundFullfilled = true;
                                }

                                areaNames.Add("<color=#3bdfff>" + ad.Template.Name + "</color>");
                            }
                            else
                            {
                                if (!foundNeeded)
                                {
                                    (__instance as EFT.UI.UIElement).ShowGameObject(false);
                                    ____questIconImage.sprite = ____foundInRaidSprite;
                                    ____questIconImage.color = new Color(0.23922f, 1, 0.44314f);

                                    foundNeeded = true;
                                }

                                areaNames.Add("<color=#3dff71>" + ad.Template.Name + "</color>");
                            }
                        }
                    }
                }
            }

            if (foundNeeded || foundFullfilled)
            {
                // Build string of list of areas this is needed for
                string areaNamesString = "";
                for (int i = 0; i < areaNames.Count; ++i)
                {
                    areaNamesString += (i == 0 ? "" : (areaNames.Count == 2 ? "" : ",") + (i == areaNames.Count - 1 ? " and " : " ")) + areaNames[i];
                }

                if (___string_3 != null && (item.MarkedAsSpawnedInSession || item.QuestItem))
                {
                    ___string_3 += string.Format(" and needed for {0}".Localized(), areaNamesString);
                }
                else
                {
                    ___string_3 = string.Format("Needed for {0}".Localized(), areaNamesString);
                }

                // If this is not a quest item or found in raid, the original returns and the tooltip never gets set, so we need to set it ourselves
                ___simpleTooltip_0 = tooltip;
            }
            else
            {
                // Just to make sure the change is not permanent, because the color is never set back to the default white by EFT
                // Because if an item was a requirement, its sprite's color set to green/blue, then it stopped being a requirement, but it was found in raid/is quest item
                // the sprite would still show up green/blue
                ____questIconImage.color = Color.white;
            }
        }
    }
}

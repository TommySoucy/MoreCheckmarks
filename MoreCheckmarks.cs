using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System;
using ComponentAce.Compression.Libs.zlib;
using Comfort.Common;
using EFT;

using Requirement = GClass1278; // EFT.Hideout.RelatedRequirements as Data field (list)
using HideoutInstance = GClass1251; // search for AreaDatas (Member)
using ClientConfig = GClass333;
using EFT.UI.DragAndDrop;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;

namespace MoreCheckmarks
{

    public class MoreCheckmarksMod : MelonMod
    {
        // For config request
        private static bool patched = false;
        private static string backEndSessionID;
        public static string backendUrl;

        // Config settings
        public static bool fulfilledAnyCanBeUpgraded = false;
        public static int questPriority = 0;
        public static int hideoutPriority = 1;
        public static int wishlistPriority = 2;
        public static bool showLockedModules = true;
        public static Color needMoreColor = new Color(1, 0.37255f, 0.37255f);
        public static Color fulfilledColor = new Color(0.30588f, 1, 0.27843f);
        public static Color wishlistColor = new Color(0.23137f, 0.93725f, 1);

        // Assets
        public static Sprite whiteCheckmark;

        // To pass to second patch
        public static bool setColor = false;

        public override void OnUpdate()
        {
            if (!patched)
            {
                try 
                {
                    backEndSessionID = Singleton<ClientApplication>.Instance.GetClientBackEndSession().GetPhpSessionId();
                    backendUrl = ClientConfig.Config.BackendUrl;

                    patched = true;

                    Init();

                    DoPatching();
                }
                catch { }
            }
        }

        private static void Init()
        {
            LoadConfig();

            LoadAssets();
        }

        private static void LoadConfig()
        {
            // ONCE INTEGRATED INTO SINGLEPLAYER PATCHES OF JET, THIS SHOULD USE Request CLASS FROM JET HTTP UTILITIES TO GET CONFIG FROM SERVERSIDE
            // I took what was essential to communicate with server because I didn't want to copy the whole thing here
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var fullUri = "/client/config/checkmarks";
            if (!Uri.IsWellFormedUriString(fullUri, UriKind.Absolute))
            {
                fullUri = backendUrl + fullUri;
            }
            WebRequest request = WebRequest.Create(new Uri(fullUri));

            if (!string.IsNullOrEmpty(backEndSessionID))
            {
                request.Headers.Add("Cookie", $"PHPSESSID={backEndSessionID}");
                request.Headers.Add("SessionId", backEndSessionID);
            }

            request.Headers.Add("Accept-Encoding", "deflate");

            request.Method = "GET";

            string json = "";

            try
            {
                WebResponse response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (stream == null)
                        {
                            json = "";
                        }
                        stream.CopyTo(ms);
                        json = SimpleZlib.Decompress(ms.ToArray(), null);
                    }
                }
            }
            catch { }

            if (string.IsNullOrWhiteSpace(json))
            {
                MelonLogger.Msg("Failed to fetch serverside config, loading local instead");

                LoadLocalConfig();
            }

            try
            {
                var jObject = JObject.Parse(json);
                fulfilledAnyCanBeUpgraded = bool.Parse(jObject["fulfilledAnyCanBeUpgraded"].ToString());
                questPriority = int.Parse(jObject["questPriority"].ToString());
                hideoutPriority = int.Parse(jObject["hideoutPriority"].ToString());
                wishlistPriority = int.Parse(jObject["wishlistPriority"].ToString());
                showLockedModules = bool.Parse(jObject["showLockedModules"].ToString());
                needMoreColor = ParseColor(jObject["needMoreColor"].ToString());
                fulfilledColor = ParseColor(jObject["fulfilledColor"].ToString());
                wishlistColor = ParseColor(jObject["wishlistColor"].ToString());

                MelonLogger.Msg("Configs loaded from serverside");
            }
            catch
            {
                MelonLogger.Msg("Failed to fetch serverside config, loading local instead");

                LoadLocalConfig();
            }
        }

        private static void LoadLocalConfig()
        {
            try
            {
                string[] lines = File.ReadAllLines("Mods/MoreCheckmarksConfig.txt");

                foreach (string line in lines)
                {
                    if (line.Length == 0 || line[0] == '#')
                    {
                        continue;
                    }

                    string trimmedLine = line.Trim();
                    string[] tokens = trimmedLine.Split('=');

                    if (tokens.Length == 0)
                    {
                        continue;
                    }

                    if (tokens[0].IndexOf("fulfilledAnyCanBeUpgraded") == 0)
                    {
                        if (tokens[1].IndexOf("true") > -1)
                        {
                            fulfilledAnyCanBeUpgraded = true;
                        }
                    }
                    else if (tokens[0].IndexOf("questPriority") == 0)
                    {
                        questPriority = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("hideoutPriority") == 0)
                    {
                        hideoutPriority = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("wishlistPriority") == 0)
                    {
                        wishlistPriority = int.Parse(tokens[1].Trim());
                    }
                    else if (tokens[0].IndexOf("showLockedModules") == 0)
                    {
                        if (tokens[1].IndexOf("false") > -1)
                        {
                            showLockedModules = false;
                        }
                    }
                    else if (tokens[0].IndexOf("needMoreColor") == 0)
                    {
                        int parenthesisIndex = tokens[1].IndexOf("(");
                        if (parenthesisIndex > -1)
                        {
                            needMoreColor = ParseColor(tokens[1].Substring(parenthesisIndex));
                        }
                    }
                    else if (tokens[0].IndexOf("fulfilledColor") == 0)
                    {
                        int parenthesisIndex = tokens[1].IndexOf("(");
                        if (parenthesisIndex > -1)
                        {
                            fulfilledColor = ParseColor(tokens[1].Substring(parenthesisIndex));
                        }
                    }
                    else if (tokens[0].IndexOf("wishlistColor") == 0)
                    {
                        int parenthesisIndex = tokens[1].IndexOf("(");
                        if (parenthesisIndex > -1)
                        {
                            wishlistColor = ParseColor(tokens[1].Substring(parenthesisIndex));
                        }
                    }
                }

                MelonLogger.Msg("Configs loaded from local");
            }
            catch(FileNotFoundException) { /* In case of file not found, we don't want to do anything, user prob deleted it for a reason */ }
            catch(Exception ex) { MelonLogger.Msg("Couldn't read MoreCheckmarksConfig.txt, using default settings instead. Error: "+ex.Message); }
        }

        private static void LoadAssets()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile("Mods/MoreCheckmarksAssets");

            if(assetBundle == null)
            {
                MelonLogger.Msg("Failed to load assets, inspect window checkmark may be miscolored");
            }
            else
            {
                whiteCheckmark = assetBundle.LoadAsset<Sprite>("WhiteCheckmark");

                MelonLogger.Msg("Assets loaded");
            }
        }

        public static void DoPatching()
        {
            var harmony = new HarmonyLib.Harmony("VIP.TommySoucy.MoreCheckmarks");

            harmony.PatchAll();
        }

        public static Color ParseColor(string colorString)
        {
            string trimmed = colorString.Trim(new char[] { '(', ')' });
            string[] values = trimmed.Split(',');

            return new Color(float.Parse(values[0]),float.Parse(values[1]),float.Parse(values[2]));
        }
    }

    [HarmonyPatch]
    class QuestItemViewPanelShowPatch
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
            List<bool> areaFulfilled = new List<bool>();
            int requiredCount = 0;
            int possessedCount = 0;
            bool questItem = item.QuestItem || (___string_3 != null && ___string_3.Contains("quest"));

            HideoutInstance hideoutInstance = Comfort.Common.Singleton<HideoutInstance>.Instance;
            foreach (EFT.Hideout.AreaData ad in hideoutInstance.AreaDatas)
            {
                EFT.Hideout.Stage actualNextStage = ad.NextStage;

                // If we don't want to get requirement of locked to construct areas, skip if it is locked to construct
                if (!MoreCheckmarksMod.showLockedModules && ad.Status == EFT.Hideout.EAreaStatus.LockedToConstruct)
                {
                    continue;
                }

                // If the area has no future upgrade, skip
                if (ad.Status == EFT.Hideout.EAreaStatus.NoFutureUpgrades)
                {
                    continue;
                }

                // If in process of constructing or upgrading, go to actual next stage if it exists
                if(ad.Status == EFT.Hideout.EAreaStatus.Constructing ||
                   ad.Status == EFT.Hideout.EAreaStatus.Upgrading)
                {
                    actualNextStage = ad.StageAt(ad.NextStage.Level + 1);

                    // If there are not StageAt given level, it will return a new stage, so level will be 0
                    if (actualNextStage.Level == 0)
                    {
                        continue;
                    }
                }

                EFT.Hideout.RelatedRequirements requirements = actualNextStage.Requirements;

                foreach (Requirement requirement in requirements)
                {
                    EFT.Hideout.ItemRequirement itemRequirement = requirement as EFT.Hideout.ItemRequirement;
                    if (itemRequirement != null)
                    {
                        string requirementTemplate = itemRequirement.TemplateId;
                        if (template == requirementTemplate)
                        {
                            // Sum up the total amount of this item required in entire hideout and update possessed amount
                            requiredCount += itemRequirement.IntCount;
                            possessedCount = itemRequirement.ItemsCount;
                            
                            // A requirement but already have the amount we need
                            if (requirement.Fulfilled)
                            {
                                // Even if we have enough of this item to fulfill a requirement in one area
                                // we might still need it, and if thats the case we want to show that color, not fulfilled color, so you know you still need more of it
                                // So only set color to fulfilled if not needed
                                if (!foundNeeded && !foundFullfilled)
                                {
                                    foundFullfilled = true;
                                }

                                areaNames.Add("<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.fulfilledColor) + ">" + ad.Template.Name + "</color>");
                            }
                            else
                            {
                                if (!foundNeeded)
                                {
                                    foundNeeded = true;
                                }

                                areaNames.Add("<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.needMoreColor) + ">" + ad.Template.Name + "</color>");
                            }
                        }
                    }
                }
            }

            bool wishlist = ItemUiContext.Instance.IsInWishList(item.TemplateId);

            if (foundNeeded)
            {
                if (wishlist && MoreCheckmarksMod.wishlistPriority > MoreCheckmarksMod.hideoutPriority)
                {
                    SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.wishlistColor, true);
                }
                else
                {
                    SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.needMoreColor, false);
                }

                SetTooltip(areaNames, ref ___string_3, ref ___simpleTooltip_0, ref tooltip, item, questItem, wishlist);
            }
            else if (foundFullfilled)
            {
                if (wishlist && MoreCheckmarksMod.wishlistPriority > MoreCheckmarksMod.hideoutPriority)
                {
                    SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.wishlistColor, true);
                }
                else
                {
                    if (MoreCheckmarksMod.fulfilledAnyCanBeUpgraded)
                    {
                        SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.fulfilledColor, false);
                    }
                    else // We only want blue checkmark when ALL requiring this item can be upgraded (if all other requirements are fulfilled too but thats implied)
                    {
                        // Check if we trully do not need more of this item for now
                        if (possessedCount >= requiredCount)
                        {
                            SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.fulfilledColor, false);
                        }
                        else // Still need more
                        {
                            SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.needMoreColor, false);
                        }
                    }
                }

                SetTooltip(areaNames, ref ___string_3, ref ___simpleTooltip_0, ref tooltip, item, questItem, wishlist);
            }
            else if (wishlist) // We don't want to color it for hideout, but it is in wishlist
            {
                SetTooltip(areaNames, ref ___string_3, ref ___simpleTooltip_0, ref tooltip, item, questItem, true);

                SetCheckmark(profile, item.TemplateId, questItem, __instance, ____questIconImage, ____foundInRaidSprite, MoreCheckmarksMod.wishlistColor, true);
            }
            else
            {
                // Just to make sure the change is not permanent, because the color is never set back to the default white by EFT
                // Because if an item was a requirement, its sprite's color set to green/blue, then it stopped being a requirement, but it was found in raid/is quest item
                // the sprite would still show up green/blue
                ____questIconImage.color = Color.white;

                MoreCheckmarksMod.setColor = false;
            }
        }

        private static void SetCheckmark(EFT.Profile profile, string templateID, bool questItem, EFT.UI.DragAndDrop.QuestItemViewPanel __instance, Image ____questIconImage,
                                         Sprite sprite, Color color, bool wishlist)
        {
            // At this point we got the color that was prioritized between wishlist and hideout, now have to compare with quest
            // Set checkmark depending on priority settings
            if (!questItem || MoreCheckmarksMod.questPriority < (wishlist ? MoreCheckmarksMod.wishlistPriority : MoreCheckmarksMod.hideoutPriority))
            {
                // Following calls base class method ShowGameObject()
                // To call base methods without reverse patch, must modify IL code for this line from callvirt to call
                (__instance as EFT.UI.UIElement).ShowGameObject(false);
                ____questIconImage.sprite = sprite;
                ____questIconImage.color = color;

                MoreCheckmarksMod.setColor = true;
            }
            else
            {
                MoreCheckmarksMod.setColor = false;
            }
        }

        private static void SetTooltip(List<string> areaNames, ref string ___string_3, ref EFT.UI.SimpleTooltip ___simpleTooltip_0, ref EFT.UI.SimpleTooltip tooltip,
                                       EFT.InventoryLogic.Item item, bool questItem, bool wishlist)
        {
            // Build string of list of areas this is needed for
            string areaNamesString = "";
            for (int i = 0; i < areaNames.Count; ++i)
            {
                areaNamesString += (i == 0 ? "" : (areaNames.Count == 2 ? "" : ",") + (i == areaNames.Count - 1 ? " and " : " ")) + areaNames[i];
            }

            if (!areaNamesString.Equals(""))
            {
                if (___string_3 != null && (item.MarkedAsSpawnedInSession || questItem))
                {
                    ___string_3 += string.Format(" and needed for {0}".Localized(), areaNamesString);

                    if (wishlist)
                    {
                        ___string_3 += string.Format(", and on {0}".Localized(), "<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.wishlistColor) + ">Wish List</color>");
                    }
                }
                else
                {
                    ___string_3 = string.Format("Needed for {0}".Localized(), areaNamesString);

                    if (wishlist)
                    {
                        ___string_3 += string.Format(", and on {0}".Localized(), "<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.wishlistColor) + ">Wish List</color>");
                    }
                }
            }
            else // Means the method was called for wishlist
            {
                if (___string_3 != null && (item.MarkedAsSpawnedInSession || questItem))
                {
                    ___string_3 += string.Format(" and on {0}".Localized(), "<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.wishlistColor) + ">Wish List</color>");
                }
                else
                {
                    ___string_3 = string.Format("On {0}".Localized(), "<color=#" + ColorUtility.ToHtmlStringRGB(MoreCheckmarksMod.wishlistColor) + ">Wish List</color>");
                }
            }

            // If this is not a quest item or found in raid, the original returns and the tooltip never gets set, so we need to set it ourselves
            ___simpleTooltip_0 = tooltip;
        }
    }

    [HarmonyPatch]
    class ItemSpecificationPanelShowPatch
    {
        // This postfix will run after the inspect window sets its checkmark if there is one
        // If there is one, the postfix for the QuestItemViewPanel will always have run before
        // This patch just changes the sprite to a default white one if needed, so we can set its color to whatever we need
        [HarmonyPatch(typeof(EFT.UI.ItemSpecificationPanel), "method_2")]
        static void Postfix(ref Item ___item_0, ref QuestItemViewPanel ____questItemViewPanel)
        {
            // If the checkmark exists and if the color of the checkmark is custom
            if (____questItemViewPanel != null && MoreCheckmarksMod.setColor)
            {
                // Get access to QuestItemViewPanel's private _questIconImage
                BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                FieldInfo iconImageField = typeof(QuestItemViewPanel).GetField("_questIconImage", bindFlags);
                Image _questIconImage = iconImageField.GetValue(____questItemViewPanel) as Image;

                if (_questIconImage != null)
                {
                    _questIconImage.sprite = MoreCheckmarksMod.whiteCheckmark;
                }
            }
        }
    }
}

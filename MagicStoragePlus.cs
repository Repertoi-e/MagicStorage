#define PATCH 

using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.DataStructures;
using Terraria.Localization;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MagicStoragePlus
{
#if PATCH
    [HarmonyPatch(typeof(Main))]
    [HarmonyPatch("DrawInventory")]
    public class MainDrawInventoryPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);

            // Search for the first 4 consecative Ldc_I4 instructions (a Color constructor after if statements which move the trash can in other conditions)
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, 150), new CodeMatch(OpCodes.Ldc_I4, 150), new CodeMatch(OpCodes.Ldc_I4, 150), new CodeMatch(OpCodes.Ldc_I4, 150), new CodeMatch(OpCodes.Newobj));

            matcher.Instruction.opcode = OpCodes.Nop;
            matcher.Instruction.operand = null;
            matcher.Advance(1);

            Label bailOut;
            matcher.CreateLabelAt(matcher.Pos, out bailOut);

            var trashOffsetCode = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.playerInventory))),
                new CodeInstruction(OpCodes.Brfalse_S, bailOut),

                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))),
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.myPlayer))),
                new CodeInstruction(OpCodes.Ldelem_Ref),

                new CodeInstruction(OpCodes.Callvirt, typeof(Player).GetMethod("GetModPlayer", new Type[] { }).MakeGenericMethod(typeof(StoragePlayer))),
                new CodeInstruction(OpCodes.Ldfld, typeof(StoragePlayer).GetField(nameof(StoragePlayer.StorageAccess))),
                new CodeInstruction(OpCodes.Ldfld, typeof(Point16).GetField(nameof(Point16.X))),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Blt_S, bailOut),

                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Ldsflda, typeof(UI).GetField(nameof(UI.TrashSlotOffset))),
                new CodeInstruction(OpCodes.Ldfld, typeof(Point16).GetField(nameof(Point16.X))),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stloc_0),

                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldsflda, typeof(UI).GetField(nameof(UI.TrashSlotOffset))),
                new CodeInstruction(OpCodes.Ldfld, typeof(Point16).GetField(nameof(Point16.Y))),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Stloc_1),

                new CodeInstruction(OpCodes.Ldc_R4, 0.755f),
                new CodeInstruction(OpCodes.Stsfld, typeof(Main).GetField(nameof(Main.inventoryScale)))
            };
            matcher.InsertAndAdvance(trashOffsetCode);

            matcher.Advance(1);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4, 150));

            matcher.MatchForward(true, new CodeMatch(OpCodes.Call, typeof(ChestUI).GetMethod("Draw", new Type[] { typeof(Microsoft.Xna.Framework.Graphics.SpriteBatch) })));
            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldsfld), new CodeMatch(OpCodes.Ldsfld), new CodeMatch(OpCodes.Ldelem_Ref), new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldc_I4_M1), new CodeMatch(OpCodes.Bne_Un), new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npcShop))), new CodeMatch(OpCodes.Brtrue));

            var iconsLabel = matcher.Instruction.operand;
            matcher.Advance(1);

            var icon1Code = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))),
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.myPlayer))),
                new CodeInstruction(OpCodes.Ldelem_Ref),

                new CodeInstruction(OpCodes.Callvirt, typeof(Player).GetMethod("GetModPlayer", new Type[] { }).MakeGenericMethod(typeof(StoragePlayer))),
                new CodeInstruction(OpCodes.Ldfld, typeof(StoragePlayer).GetField(nameof(StoragePlayer.StorageAccess))),
                new CodeInstruction(OpCodes.Ldfld, typeof(Point16).GetField(nameof(Point16.X))),

                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Bgt, iconsLabel)
            };
            matcher.InsertAndAdvance(icon1Code);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Callvirt, typeof(Player).GetMethod("QuickStackAllChests")));
            var quickStackCode = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Call, typeof(StorageUI).GetMethod("TryQuickStack", new Type[] { typeof(bool) })),
                new CodeInstruction(OpCodes.Pop)
            };
            matcher.InsertAndAdvance(quickStackCode);

            matcher.MatchForward(true, new CodeMatch(OpCodes.Ldsfld), new CodeMatch(OpCodes.Ldsfld), new CodeMatch(OpCodes.Ldelem_Ref), new CodeMatch(OpCodes.Ldfld), new CodeMatch(OpCodes.Ldc_I4_M1), new CodeMatch(OpCodes.Bne_Un), new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npcShop))), new CodeMatch(OpCodes.Brtrue));

            iconsLabel = matcher.Instruction.operand;
            matcher.Advance(1);

            var icon2Code = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))),
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.myPlayer))),
                new CodeInstruction(OpCodes.Ldelem_Ref),

                new CodeInstruction(OpCodes.Callvirt, typeof(Player).GetMethod("GetModPlayer", new Type[] { }).MakeGenericMethod(typeof(StoragePlayer))),
                new CodeInstruction(OpCodes.Ldfld, typeof(StoragePlayer).GetField(nameof(StoragePlayer.StorageAccess))),
                new CodeInstruction(OpCodes.Ldfld, typeof(Point16).GetField(nameof(Point16.X))),

                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Bgt, iconsLabel)
            };
            matcher.InsertAndAdvance(icon2Code);

            return matcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(Recipe))]
    [HarmonyPatch("FindRecipes")]
    public class RecipeFindRecipesPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);

            // Search for the first time we check if the player has opened a chest, this happens when adding the items in the opened inventory to the dictionary 
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))), new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.myPlayer))), new CodeMatch(OpCodes.Ldelem_Ref), new CodeMatch(OpCodes.Ldfld, typeof(Player).GetField(nameof(Player.chest))), new CodeMatch(OpCodes.Ldc_I4_M1), new CodeMatch(OpCodes.Beq));
            var findRecipesCode = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 6),
                new CodeInstruction(OpCodes.Call, typeof(MagicStoragePlus).GetMethod(nameof(MagicStoragePlus.FindRecipesPatch), new Type[] { typeof(Dictionary<int, int>).MakeByRefType() }))
            };
            matcher.InsertAndAdvance(findRecipesCode);

            return matcher.InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(Recipe))]
    [HarmonyPatch("Create")]
    public class RecipeCreatePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var matcher = new CodeMatcher(instructions, il);

            // Search for the first time we check if the player has opened a chest, this happens when adding the items in the opened inventory to the dictionary 
            matcher.MatchForward(false, new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))), new CodeMatch(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.myPlayer))), new CodeMatch(OpCodes.Ldelem_Ref), new CodeMatch(OpCodes.Ldfld, typeof(Player).GetField(nameof(Player.chest))), new CodeMatch(OpCodes.Ldc_I4_M1), new CodeMatch(OpCodes.Beq));

            matcher.Instruction.opcode = OpCodes.Nop;
            matcher.Instruction.operand = null;
            matcher.Advance(1);

            var getItemCode = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Ldloca_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(StorageUI).GetMethod(nameof(StorageUI.DoWithdrawItemForCraft),new Type[] { typeof(Recipe), typeof(Item).MakeByRefType(), typeof(int).MakeByRefType() })),
            
                // The old instruction we destroyed 
                new CodeInstruction(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player)))
            };
            matcher.InsertAndAdvance(getItemCode);

            return matcher.InstructionEnumeration();
        }
    }
#endif

    public class MagicStoragePlus : Mod
    {
        public static MagicStoragePlus Instance;
        public string[] AllMods { get; private set; }

        public UserInterface Interface;
        public StorageUI StorageUI;

        public static Mod BluemagicMod;
        public static Mod LegendMod;

        public static readonly Version RequiredVersion = new Version(0, 11);

#if PATCH
        public static Harmony HarmonyInstance;
#endif

        public override void Load()
        {
            if (ModLoader.version < RequiredVersion)
                throw new Exception("Magic storage requires a tModLoader version of at least " + RequiredVersion);

            Instance = this;
            UI.Initialize();

            AddTranslations();

            LegendMod = ModLoader.GetMod("LegendOfTerraria3");
            BluemagicMod = ModLoader.GetMod("Bluemagic");

            // Apply IL patches
#if PATCH
            HarmonyInstance = new Harmony("repertoire.tmodloader.mod.MagicStoragePlus");
            HarmonyInstance.PatchAll();
#endif

            if (!Main.dedServ)
            {
                Interface = new UserInterface();

                StorageUI = new StorageUI();
                StorageUI.Activate();
            }
        }

        GameTime LastUpdateUIGameTime;
        public override void UpdateUI(GameTime gameTime)
        {
            LastUpdateUIGameTime = gameTime;
            if (Interface?.CurrentState != null)
                Interface.Update(gameTime);
        }

        public override void Unload()
        {
#if PATCH
            HarmonyInstance.UnpatchAll();
#endif
            Instance = null;
            BluemagicMod = null;
            LegendMod = null;
        }

        private void AddTranslations()
        {
            ModTranslation text = CreateTranslation("SetTo");
            text.SetDefault("Set to: X={0}, Y={1}");
            text.AddTranslation(GameCulture.Polish, "Ustawione na: X={0}, Y={1}");
            text.AddTranslation(GameCulture.French, "Mis à: X={0}, Y={1}");
            text.AddTranslation(GameCulture.Spanish, "Ajustado a: X={0}, Y={1}");
            text.AddTranslation(GameCulture.Chinese, "已设置为: X={0}, Y={1}");
            AddTranslation(text);

            text = CreateTranslation("SnowBiomeBlock");
            text.SetDefault("Snow Biome Block");
            text.AddTranslation(GameCulture.French, "Bloc de biome de neige");
            text.AddTranslation(GameCulture.Spanish, "Bloque de Biomas de la Nieve");
            text.AddTranslation(GameCulture.Chinese, "雪地环境方块");
            AddTranslation(text);

            text = CreateTranslation("Search");
            text.SetDefault("Search");
            text.AddTranslation(GameCulture.Russian, "Поиск");
            text.AddTranslation(GameCulture.French, "Rechercher");
            text.AddTranslation(GameCulture.Spanish, "Buscar");
            text.AddTranslation(GameCulture.Chinese, "搜索");
            AddTranslation(text);

            text = CreateTranslation("SearchName");
            text.SetDefault("Search Name");
            text.AddTranslation(GameCulture.Russian, "Поиск по имени");
            text.AddTranslation(GameCulture.French, "Recherche par nom");
            text.AddTranslation(GameCulture.Spanish, "búsqueda por nombre");
            text.AddTranslation(GameCulture.Chinese, "搜索名称");
            AddTranslation(text);

            text = CreateTranslation("SearchMod");
            text.SetDefault("Search Mod");
            text.AddTranslation(GameCulture.Russian, "Поиск по моду");
            text.AddTranslation(GameCulture.French, "Recherche par mod");
            text.AddTranslation(GameCulture.Spanish, "búsqueda por mod");
            text.AddTranslation(GameCulture.Chinese, "搜索模组");
            AddTranslation(text);

            text = CreateTranslation("Sort");
            text.SetDefault("Sort");
            text.AddTranslation(GameCulture.Russian, "Sортировка");
            text.AddTranslation(GameCulture.French, "Tri");
            text.AddTranslation(GameCulture.Spanish, "Clasificación");
            // text.AddTranslation(GameCulture.Chinese, "默认排序"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("SortDefault");
            text.SetDefault("Default");
            text.AddTranslation(GameCulture.Russian, "Стандартная");
            text.AddTranslation(GameCulture.French, "Standard");
            text.AddTranslation(GameCulture.Spanish, "Ordenar por defecto");
            // text.AddTranslation(GameCulture.Chinese, "默认排序"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("SortID");
            text.SetDefault("ID");
            text.AddTranslation(GameCulture.Russian, "По ID");
            text.AddTranslation(GameCulture.French, "ID");
            text.AddTranslation(GameCulture.Spanish, "ID");
            // text.AddTranslation(GameCulture.Chinese, "按ID排序"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("SortName");
            text.SetDefault("Name");
            text.AddTranslation(GameCulture.Russian, "По имени");
            text.AddTranslation(GameCulture.French, "Nom");
            text.AddTranslation(GameCulture.Spanish, "Nombre");
            // text.AddTranslation(GameCulture.Chinese, "按名称排序"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("SortQuantity");
            text.SetDefault("Quantity");
            text.AddTranslation(GameCulture.Russian, "По стакам");
            text.AddTranslation(GameCulture.French, "Piles");
            text.AddTranslation(GameCulture.Spanish, "Pilas");
            // text.AddTranslation(GameCulture.Chinese, "按堆栈排序"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("Filter");
            text.SetDefault("Filter");
            text.AddTranslation(GameCulture.Russian, "Фильтр");
            text.AddTranslation(GameCulture.French, "Filtrer");
            text.AddTranslation(GameCulture.Spanish, "Filtrar");
            // text.AddTranslation(GameCulture.Chinese, "筛选全部"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterAll");
            text.SetDefault("All");
            text.AddTranslation(GameCulture.Russian, "Всё");
            text.AddTranslation(GameCulture.French, "Tout");
            text.AddTranslation(GameCulture.Spanish, "Todo");
            // text.AddTranslation(GameCulture.Chinese, "筛选全部"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterWeapons");
            text.SetDefault("Weapons");
            text.AddTranslation(GameCulture.Russian, "Оружия");
            text.AddTranslation(GameCulture.French, "Armes");
            text.AddTranslation(GameCulture.Spanish, "Armas");
            //  text.AddTranslation(GameCulture.Chinese, "筛选武器"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterTools");
            text.SetDefault("Tools");
            text.AddTranslation(GameCulture.Russian, "Инструменты");
            text.AddTranslation(GameCulture.French, "Outils");
            text.AddTranslation(GameCulture.Spanish, "Herramientas");
            // text.AddTranslation(GameCulture.Chinese, "筛选工具"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterEquipment");
            text.SetDefault("Equipment");
            text.AddTranslation(GameCulture.Russian, "Снаряжения");
            text.AddTranslation(GameCulture.French, "Équipement");
            text.AddTranslation(GameCulture.Spanish, "Equipamiento");
            // text.AddTranslation(GameCulture.Chinese, "筛选装备"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterPotions");
            text.SetDefault("Potions");
            text.AddTranslation(GameCulture.Russian, "Зелья");
            text.AddTranslation(GameCulture.French, "Potions");
            text.AddTranslation(GameCulture.Spanish, "Poción");
            // text.AddTranslation(GameCulture.Chinese, "筛选药水"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterPlaceables");
            text.SetDefault("Placeables");
            text.AddTranslation(GameCulture.Russian, "Размещаемое");
            text.AddTranslation(GameCulture.French, "Placeable");
            text.AddTranslation(GameCulture.Spanish, "Metido");
            // text.AddTranslation(GameCulture.Chinese, "筛选放置物"); @TODO: Chinese
            AddTranslation(text);

            text = CreateTranslation("FilterMisc");
            text.SetDefault("Misc");
            text.AddTranslation(GameCulture.Russian, "Разное");
            text.AddTranslation(GameCulture.French, "Miscellanées");
            text.AddTranslation(GameCulture.Spanish, "Otros");
            // text.AddTranslation(GameCulture.Chinese, "筛选杂项"); @TODO: Chinese
            AddTranslation(text);

            /*
            text = CreateTranslation("CraftingStations");
            text.SetDefault("Crafting Stations");
            text.AddTranslation(GameCulture.Russian, "Станции создания");
            text.AddTranslation(GameCulture.French, "Stations d'artisanat");
            text.AddTranslation(GameCulture.Spanish, "Estaciones de elaboración");
            text.AddTranslation(GameCulture.Chinese, "制作站");
            AddTranslation(text);

            text = CreateTranslation("Recipes");
            text.SetDefault("Recipes");
            text.AddTranslation(GameCulture.Russian, "Рецепты");
            text.AddTranslation(GameCulture.French, "Recettes");
            text.AddTranslation(GameCulture.Spanish, "Recetas");
            text.AddTranslation(GameCulture.Chinese, "合成配方");
            AddTranslation(text);

            text = CreateTranslation("SelectedRecipe");
            text.SetDefault("Selected Recipe");
            text.AddTranslation(GameCulture.French, "Recette sélectionnée");
            text.AddTranslation(GameCulture.Spanish, "Receta seleccionada");
            text.AddTranslation(GameCulture.Chinese, "选择配方");
            AddTranslation(text);

            text = CreateTranslation("Ingredients");
            text.SetDefault("Ingredients");
            text.AddTranslation(GameCulture.French, "Ingrédients");
            text.AddTranslation(GameCulture.Spanish, "Ingredientes");
            text.AddTranslation(GameCulture.Chinese, "材料");
            AddTranslation(text);

            text = CreateTranslation("StoredItems");
            text.SetDefault("Stored Ingredients");
            text.AddTranslation(GameCulture.French, "Ingrédients Stockés");
            text.AddTranslation(GameCulture.Spanish, "Ingredientes almacenados");
            text.AddTranslation(GameCulture.Chinese, "存储中的材料");
            AddTranslation(text);

            text = CreateTranslation("RecipeAvailable");
            text.SetDefault("Show available recipes");
            text.AddTranslation(GameCulture.French, "Afficher les recettes disponibles");
            text.AddTranslation(GameCulture.Spanish, "Mostrar recetas disponibles");
            text.AddTranslation(GameCulture.Chinese, "显示可合成配方");
            AddTranslation(text);

            text = CreateTranslation("RecipeAll");
            text.SetDefault("Show all recipes");
            text.AddTranslation(GameCulture.French, "Afficher toutes les recettes");
            text.AddTranslation(GameCulture.Spanish, "Mostrar todas las recetas");
            text.AddTranslation(GameCulture.Chinese, "显示全部配方");
            AddTranslation(text);
            */
        }

        public override void PostSetupContent()
        {
            var type = Assembly.GetAssembly(typeof(Mod)).GetType("Terraria.ModLoader.Mod");

            FieldInfo loadModsField = type.GetField("items", BindingFlags.Instance | BindingFlags.NonPublic);
            AllMods = ModLoader.Mods.Where(x => ((Dictionary<string, ModItem>)loadModsField.GetValue(x)).Count > 0).Select(x => x.Name).ToArray();
        }

        public override void AddRecipeGroups()
        {
            RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Chest",
            ItemID.Chest,
            ItemID.GoldChest,
            ItemID.ShadowChest,
            ItemID.EbonwoodChest,
            ItemID.RichMahoganyChest,
            ItemID.PearlwoodChest,
            ItemID.IvyChest,
            ItemID.IceChest,
            ItemID.LivingWoodChest,
            ItemID.SkywareChest,
            ItemID.ShadewoodChest,
            ItemID.WebCoveredChest,
            ItemID.LihzahrdChest,
            ItemID.WaterChest,
            ItemID.JungleChest,
            ItemID.CorruptionChest,
            ItemID.CrimsonChest,
            ItemID.HallowedChest,
            ItemID.FrozenChest,
            ItemID.DynastyChest,
            ItemID.HoneyChest,
            ItemID.SteampunkChest,
            ItemID.PalmWoodChest,
            ItemID.MushroomChest,
            ItemID.BorealWoodChest,
            ItemID.SlimeChest,
            ItemID.GreenDungeonChest,
            ItemID.PinkDungeonChest,
            ItemID.BlueDungeonChest,
            ItemID.BoneChest,
            ItemID.CactusChest,
            ItemID.FleshChest,
            ItemID.ObsidianChest,
            ItemID.PumpkinChest,
            ItemID.SpookyChest,
            ItemID.GlassChest,
            ItemID.MartianChest,
            ItemID.GraniteChest,
            ItemID.MeteoriteChest,
            ItemID.MarbleChest);
            RecipeGroup.RegisterGroup("MagicStoragePlus:AnyChest", group);
            group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("Mods.MagicStoragePlus.SnowBiomeBlock"), ItemID.SnowBlock, ItemID.IceBlock, ItemID.PurpleIceBlock, ItemID.PinkIceBlock);
            if (BluemagicMod != null)
            {
                group.ValidItems.Add(BluemagicMod.ItemType("DarkBlueIce"));
            }
            RecipeGroup.RegisterGroup("MagicStoragePlus:AnySnowBiomeBlock", group);
            group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Diamond), ItemID.Diamond, ItemType("ShadowDiamond"));
            if (LegendMod != null)
            {
                group.ValidItems.Add(LegendMod.ItemType("GemChrysoberyl"));
                group.ValidItems.Add(LegendMod.ItemType("GemAlexandrite"));
            }
            RecipeGroup.RegisterGroup("MagicStoragePlus:AnyDiamond", group);
            if (LegendMod != null)
            {
                group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Amethyst), ItemID.Amethyst, LegendMod.ItemType("GemOnyx"), LegendMod.ItemType("GemSpinel"));
                RecipeGroup.RegisterGroup("MagicStoragePlus:AnyAmethyst", group);
                group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Topaz), ItemID.Topaz, LegendMod.ItemType("GemGarnet"));
                RecipeGroup.RegisterGroup("MagicStoragePlus:AnyTopaz", group);
                group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Sapphire), ItemID.Sapphire, LegendMod.ItemType("GemCharoite"));
                RecipeGroup.RegisterGroup("MagicStoragePlus:AnySapphire", group);
                group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Emerald), LegendMod.ItemType("GemPeridot"));
                RecipeGroup.RegisterGroup("MagicStoragePlus:AnyEmerald", group);
                group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Ruby), ItemID.Ruby, LegendMod.ItemType("GemOpal"));
                RecipeGroup.RegisterGroup("MagicStoragePlus:AnyRuby", group);
            }
        }

        public static void FindRecipesPatch(ref Dictionary<int, int> dict)
        {
            var heart = StoragePlayer.GetStorageHeart();
            if (heart != null)
            {
                foreach (Item item in heart.GetStoredItems())
                {
                    if (item.stack == 0) continue;

                    if (dict.ContainsKey(item.netID))
                        dict[item.netID] += item.stack;
                    else
                        dict[item.netID] = item.stack;
                }
            }
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetHelper.HandlePacket(reader, whoAmI);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "MagicStoragePlus: StorageAccess",
                    () =>
                    {
                        if (LastUpdateUIGameTime != null && Interface?.CurrentState != null)
                            Interface.Draw(Main.spriteBatch, LastUpdateUIGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        public override void PostUpdateInput()
        {
            if (!Main.instance.IsActive)
                return;
            UI.Update();
        }
    }
}


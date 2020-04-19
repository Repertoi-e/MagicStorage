using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;

namespace MagicStoragePlus.Sorting
{
    public class CompareDefault : CompareFunction
    {
        public CompareDefault()
        {
            SortClassList.Initialize();
        }

        public override int Compare(Item item1, Item item2)
        {
            return SortClassList.Compare(item1, item2);
        }
    }

    public static class SortClassList
    {
        static bool initialized = false;
        static List<DefaultSortClass> classes = new List<DefaultSortClass>();

        public static int Compare(Item item1, Item item2)
        {
            int class1 = classes.Count;
            int class2 = classes.Count;
            for (int k = 0; k < classes.Count; k++)
            {
                if (classes[k].Pass(item1))
                {
                    class1 = k;
                    break;
                }
            }
            for (int k = 0; k < classes.Count; k++)
            {
                if (classes[k].Pass(item2))
                {
                    class2 = k;
                    break;
                }
            }
            if (class1 != class2)
            {
                return class1 - class2;
            }
            return classes[class1].Compare(item1, item2);
        }

        public static void Initialize()
        {
            if (initialized) return;

            classes.Add(new DefaultSortClass(MeleeWeapon, CompareRarity));
            classes.Add(new DefaultSortClass(RangedWeapon, CompareRarity));
            classes.Add(new DefaultSortClass(MagicWeapon, CompareRarity));
            classes.Add(new DefaultSortClass(SummonWeapon, CompareRarity));
            classes.Add(new DefaultSortClass(ThrownWeapon, CompareRarity));
            classes.Add(new DefaultSortClass(Weapon, CompareRarity));
            classes.Add(new DefaultSortClass(Ammo, CompareRarity));
            classes.Add(new DefaultSortClass(Picksaw, ComparePicksaw));
            classes.Add(new DefaultSortClass(Hamaxe, CompareHamaxe));
            classes.Add(new DefaultSortClass(Pickaxe, ComparePickaxe));
            classes.Add(new DefaultSortClass(Axe, CompareAxe));
            classes.Add(new DefaultSortClass(Hammer, CompareHammer));
            classes.Add(new DefaultSortClass(TerraformingTool, CompareTerraformingPriority));
            classes.Add(new DefaultSortClass(AmmoTool, CompareRarity));
            classes.Add(new DefaultSortClass(Armor, CompareRarity));
            classes.Add(new DefaultSortClass(VanityArmor, CompareRarity));
            classes.Add(new DefaultSortClass(Accessory, CompareAccessory));
            classes.Add(new DefaultSortClass(Grapple, CompareRarity));
            classes.Add(new DefaultSortClass(Mount, CompareRarity));
            classes.Add(new DefaultSortClass(Cart, CompareRarity));
            classes.Add(new DefaultSortClass(LightPet, CompareRarity));
            classes.Add(new DefaultSortClass(VanityPet, CompareRarity));
            classes.Add(new DefaultSortClass(Dye, CompareDye));
            classes.Add(new DefaultSortClass(HairDye, CompareHairDye));
            classes.Add(new DefaultSortClass(HealthPotion, CompareHealing));
            classes.Add(new DefaultSortClass(ManaPotion, CompareMana));
            classes.Add(new DefaultSortClass(Elixir, CompareElixir));
            classes.Add(new DefaultSortClass(BuffPotion, CompareRarity));
            classes.Add(new DefaultSortClass(BossSpawn, CompareBossSpawn));
            classes.Add(new DefaultSortClass(Painting, ComparePainting));
            classes.Add(new DefaultSortClass(Wiring, CompareWiring));
            classes.Add(new DefaultSortClass(Material, CompareMaterial));
            classes.Add(new DefaultSortClass(Rope, CompareRope));
            classes.Add(new DefaultSortClass(Extractible, CompareExtractible));
            classes.Add(new DefaultSortClass(Misc, CompareMisc));
            classes.Add(new DefaultSortClass(FrameImportantTile, CompareName));
            classes.Add(new DefaultSortClass(CommonTile, CompareName));
        }

        static bool MeleeWeapon(Item item)
        {
            return item.maxStack == 1 && item.damage > 0 && item.ammo == 0 && item.melee && item.pick < 1 && item.hammer < 1 && item.axe < 1;
        }

        static bool RangedWeapon(Item item)
        {
            return item.maxStack == 1 && item.damage > 0 && item.ammo == 0 && item.ranged;
        }

        static bool MagicWeapon(Item item)
        {
            return item.maxStack == 1 && item.damage > 0 && item.ammo == 0 && item.magic;
        }

        static bool SummonWeapon(Item item)
        {
            return item.maxStack == 1 && item.damage > 0 && item.summon;
        }

        static bool ThrownWeapon(Item item)
        {
            return item.damage > 0 && (item.ammo == 0 || item.notAmmo) && item.shoot > 0 && item.thrown;
        }

        static bool Weapon(Item item)
        {
            return item.damage > 0 && item.ammo == 0 && item.pick == 0 && item.axe == 0 && item.hammer == 0;
        }

        static bool Ammo(Item item)
        {
            return item.ammo > 0 && item.damage > 0;
        }

        static bool Picksaw(Item item)
        {
            return item.pick > 0 && item.axe > 0;
        }

        static bool Hamaxe(Item item)
        {
            return item.hammer > 0 && item.axe > 0;
        }

        static bool Pickaxe(Item item)
        {
            return item.pick > 0;
        }

        static bool Axe(Item item)
        {
            return item.axe > 0;
        }

        static bool Hammer(Item item)
        {
            return item.hammer > 0;
        }

        static bool TerraformingTool(Item item)
        {
            return ItemID.Sets.SortingPriorityTerraforming[item.type] >= 0;
        }

        static bool AmmoTool(Item item)
        {
            return item.ammo > 0;
        }

        static bool Armor(Item item)
        {
            return (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0) && !item.vanity;
        }

        static bool VanityArmor(Item item)
        {
            return (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0) && item.vanity;
        }

        static bool Accessory(Item item)
        {
            return item.accessory;
        }

        static bool Grapple(Item item)
        {
            return Main.projHook[item.shoot];
        }

        static bool Mount(Item item)
        {
            return item.mountType != -1 && !MountID.Sets.Cart[item.mountType];
        }

        static bool Cart(Item item)
        {
            return item.mountType != -1 && MountID.Sets.Cart[item.mountType];
        }

        static bool LightPet(Item item)
        {
            return item.buffType > 0 && Main.lightPet[item.buffType];
        }

        static bool VanityPet(Item item)
        {
            return item.buffType > 0 && Main.vanityPet[item.buffType];
        }

        static bool Dye(Item item)
        {
            return item.dye > 0;
        }

        static bool HairDye(Item item)
        {
            return item.hairDye >= 0;
        }

        static bool HealthPotion(Item item)
        {
            return item.consumable && item.healLife > 0 && item.healMana < 1;
        }

        static bool ManaPotion(Item item)
        {
            return item.consumable && item.healLife < 1 && item.healMana > 0;
        }

        static bool Elixir(Item item)
        {
            return item.consumable && item.healLife > 0 && item.healMana > 0;
        }

        static bool BuffPotion(Item item)
        {
            return item.consumable && item.buffType > 0;
        }

        static bool BossSpawn(Item item)
        {
            return ItemID.Sets.SortingPriorityBossSpawns[item.type] >= 0;
        }

        static bool Painting(Item item)
        {
            return ItemID.Sets.SortingPriorityPainting[item.type] >= 0 || item.paint > 0;
        }

        static bool Wiring(Item item)
        {
            return ItemID.Sets.SortingPriorityWiring[item.type] >= 0 || item.mech;
        }

        static bool Material(Item item)
        {
            return ItemID.Sets.SortingPriorityMaterials[item.type] >= 0;
        }

        static bool Rope(Item item)
        {
            return ItemID.Sets.SortingPriorityRopes[item.type] >= 0;
        }

        static bool Extractible(Item item)
        {
            return ItemID.Sets.SortingPriorityExtractibles[item.type] >= 0;
        }

        static bool Misc(Item item)
        {
            return item.createTile < 0 && item.createWall < 1;
        }

        static bool FrameImportantTile(Item item)
        {
            return item.createTile >= 0 && Main.tileFrameImportant[item.createTile];
        }

        static bool CommonTile(Item item)
        {
            return item.createTile >= 0 || item.createWall > 0;
        }

        static int CompareRarity(Item item1, Item item2)
        {
            return item2.rare - item1.rare;
        }

        static int ComparePicksaw(Item item1, Item item2)
        {
            int result = item1.pick - item2.pick;
            if (result == 0)
                result = item1.axe - item2.axe;
            return result;
        }

        static int CompareHamaxe(Item item1, Item item2)
        {
            int result = item1.axe - item2.axe;
            if (result == 0)
                result = item1.hammer - item2.hammer;
            return result;
        }

        static int ComparePickaxe(Item item1, Item item2)
        {
            return item1.pick - item2.pick;
        }

        static int CompareAxe(Item item1, Item item2)
        {
            return item1.axe - item2.axe;
        }

        static int CompareHammer(Item item1, Item item2)
        {
            return item1.hammer - item2.hammer;
        }

        static int CompareTerraformingPriority(Item item1, Item item2)
        {
            return ItemID.Sets.SortingPriorityTerraforming[item1.type] - ItemID.Sets.SortingPriorityTerraforming[item2.type];
        }

        static int CompareAccessory(Item item1, Item item2)
        {
            int result = item1.vanity.CompareTo(item2.vanity);
            if (result == 0)
            {
                result = CompareRarity(item1, item2);
            }
            return result;
        }

        static int CompareDye(Item item1, Item item2)
        {
            int result = CompareRarity(item1, item2);
            if (result == 0)
            {
                result = item2.dye - item1.dye;
            }
            return result;
        }

        static int CompareHairDye(Item item1, Item item2)
        {
            int result = CompareRarity(item1, item2);
            if (result == 0)
            {
                result = item2.hairDye - item1.hairDye;
            }
            return result;
        }

        static int CompareHealing(Item item1, Item item2)
        {
            return item2.healLife - item1.healLife;
        }

        static int CompareMana(Item item1, Item item2)
        {
            return item2.mana - item1.mana;
        }

        static int CompareElixir(Item item1, Item item2)
        {
            int result = CompareHealing(item1, item2);
            if (result == 0)
            {
                result = CompareMana(item1, item2);
            }
            return result;
        }

        static int CompareBossSpawn(Item item1, Item item2)
        {
            return ItemID.Sets.SortingPriorityBossSpawns[item1.type] - ItemID.Sets.SortingPriorityBossSpawns[item2.type];
        }

        static int ComparePainting(Item item1, Item item2)
        {
            int result = ItemID.Sets.SortingPriorityPainting[item2.type] - ItemID.Sets.SortingPriorityPainting[item1.type];
            if (result == 0)
            {
                result = item1.paint - item2.paint;
            }
            return result;
        }

        static int CompareWiring(Item item1, Item item2)
        {
            int result = ItemID.Sets.SortingPriorityWiring[item2.type] - ItemID.Sets.SortingPriorityWiring[item1.type];
            if (result == 0)
            {
                result = CompareRarity(item1, item2);
            }
            return result;
        }

        static int CompareMaterial(Item item1, Item item2)
        {
            return ItemID.Sets.SortingPriorityMaterials[item2.type] - ItemID.Sets.SortingPriorityMaterials[item1.type];
        }

        static int CompareRope(Item item1, Item item2)
        {
            return ItemID.Sets.SortingPriorityRopes[item2.type] - ItemID.Sets.SortingPriorityRopes[item1.type];
        }

        static int CompareExtractible(Item item1, Item item2)
        {
            return ItemID.Sets.SortingPriorityExtractibles[item2.type] - ItemID.Sets.SortingPriorityExtractibles[item1.type];
        }

        static int CompareMisc(Item item1, Item item2)
        {
            int result = CompareRarity(item1, item2);
            if (result == 0)
            {
                result = item2.value - item1.value;
            }
            return result;
        }

        static int CompareName(Item item1, Item item2)
        {
            return string.Compare(item1.Name, item2.Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class DefaultSortClass
    {
        public Func<Item, bool> Pass;
        public Func<Item, Item, int> Compare;

        public DefaultSortClass(Func<Item, bool> pass, Func<Item, Item, int> compare)
        {
            Pass = pass;
            Compare = compare;
        }
    }
}